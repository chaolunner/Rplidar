using System.Collections.Generic;
using System.Net.Sockets;
using System.Collections;
using System.Threading;
using UnityEngine;
using System.Net;
using System;

public class LidarDescriptor
{
    public int Length; // unit: byte
    public bool IsSingle;
    public int Type;
}

public class LidarInfo
{
    public int MajorModel;
    public int SubModel;
    public float Firmware;
    public int Hardware;
    public string SerialNumber;
}

public enum LidarHealthStatuses
{
    Good = 0,
    Warning = 1,
    Error = 2,
}

public class LidarHealth
{
    public LidarHealthStatuses Status;
    public int ErrorCode;
}

public class LidarClient : MonoBehaviour
{
    public bool IsDebug;
    public string Host = "192.168.0.7";
    public int Port = 20108;
    public string ReceivedData = "";
    [Range(0, 1023)]
    public int MotorPWM = 660;

    private IPAddress ip;
    private IPEndPoint ipe;
    private Socket commandSocket;
    private Thread commandThread;
    private byte[] command;
    private Socket responseSocket;
    private Thread responseThread;
    private LidarDescriptor lastDescriptor = new LidarDescriptor();
    private LidarInfo lastInfo = new LidarInfo();
    private LidarHealth lastHealth = new LidarHealth();

    private const byte RPLIDAR_ANS_SYNC_BYTE1 = 0xA5;
    private const byte RPLIDAR_ANS_SYNC_BYTE2 = 0x5A;
    private const byte RPLIDAR_CMD_GET_INFO = 0x50;
    private const byte RPLIDAR_CMD_GET_HEALTH = 0x52;
    private const byte RPLIDAR_CMD_STOP = 0x25;
    private const byte RPLIDAR_CMD_RESET = 0x40;
    private const byte RPLIDAR_CMD_SCAN = 0x20;
    private const byte RPLIDAR_CMD_FORCE_SCAN = 0x21;
    private const byte RPLIDAR_CMD_SET_MOTOR_PWM = 0xF0;

    private const int STOP_MOTOR_PWM = 0;
    private const int DEFAULT_MOTOR_PWM = 660;
    private const int MAX_MOTOR_PWM = 1023;
    private const int RPLIDAR_RESP_MEASUREMENT_QUALITY_SHIFT = 2;
    private const int RPLIDAR_RESP_MEASUREMENT_ANGLE_SHIFT = 1;

    private const int DESCRIPTOR_LEN = 7;
    private const int INFO_LEN = 20;
    private const int HEALTH_LEN = 3;

    private const int INFO_TYPE = 4;
    private const int HEALTH_TYPE = 6;
    private const int SCAN_TYPE = 129;

    private void Start()
    {
        ip = IPAddress.Parse(Host);
        ipe = new IPEndPoint(ip, Port);

        commandSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        responseSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        LoadClient();

        StartCoroutine(StartScan());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StartCoroutine(Quit());
        }
    }

    private IEnumerator StartScan()
    {
        while (!commandSocket.Connected)
        {
            yield return null;
        }
        SendCommand(RPLIDAR_CMD_GET_INFO);
        yield return new WaitForSeconds(0.5f);
        SendCommand(RPLIDAR_CMD_GET_HEALTH);
        yield return new WaitForSeconds(0.5f);
        if (lastHealth.Status == LidarHealthStatuses.Error)
        {
            SendCommand(RPLIDAR_CMD_RESET);
            yield return new WaitForSeconds(0.5f);
            SendCommand(RPLIDAR_CMD_GET_HEALTH);
            yield return new WaitForSeconds(0.5f);
            if (lastHealth.Status == LidarHealthStatuses.Error)
            {
                Debug.LogError("RPLidar hardware failure. Error code: " + lastHealth.ErrorCode);
            }
        }
        else if (lastHealth.Status == LidarHealthStatuses.Warning)
        {
            Output("Warning sensor status detected!. Error code: " + lastHealth.ErrorCode, LogType.Warning);
        }
        SendCommand(RPLIDAR_CMD_SET_MOTOR_PWM, BitConverter.GetBytes(MotorPWM));
        yield return new WaitForSeconds(0.5f);
        SendCommand(RPLIDAR_CMD_SCAN, null);
        Output("Start Scan");
    }

    private IEnumerator StopScan()
    {
        SendCommand(RPLIDAR_CMD_SET_MOTOR_PWM, BitConverter.GetBytes(STOP_MOTOR_PWM));
        yield return new WaitForSeconds(0.5f);
        SendCommand(RPLIDAR_CMD_STOP);
        Output("Stop Scan");
    }

    private IEnumerator Quit()
    {
        yield return StartCoroutine(StopScan());
        yield return new WaitForSeconds(1);
#if UNITY_EDITOR
        if (Application.isEditor)
        {
            UnityEditor.EditorApplication.isPlaying = false;
        }
#else
        Application.Quit();
#endif
    }

    public void LoadClient()
    {
        commandThread = new Thread(SendCommand);
        commandThread.IsBackground = true;
        commandThread.Start();
        responseThread = new Thread(ReceiveData);
        responseThread.IsBackground = true;
        responseThread.Start();
    }

    public void SendCommand(byte cmd, byte[] payload = null)
    {
        command = ProcessCommand(cmd, payload);
    }

    private byte[] ProcessCommand(byte cmd, byte[] payload)
    {
        var cmdBytes = new List<byte>();
        cmdBytes.Add(RPLIDAR_ANS_SYNC_BYTE1);
        cmdBytes.Add(cmd);
        if (payload != null)
        {
            cmdBytes.Add((byte)payload.Length);
            cmdBytes.AddRange(payload);
            var checksum = new byte();
            foreach (var item in cmdBytes)
            {
                checksum ^= item;
            }
            cmdBytes.Add(checksum);
        }
        return cmdBytes.ToArray();
    }

    public void SendCommand()
    {
        while (true)
        {
            if (!commandSocket.Connected)
            {
                commandSocket.Connect(ipe);
            }
            try
            {
                if (command != null)
                {
                    commandSocket.Send(command);
                    command = null;
                }
            }
            catch (Exception err)
            {
                Debug.LogError(err.Message);
            }
        }
    }

    public void ReceiveData()
    {
        while (true)
        {
            if (!responseSocket.Connected)
            {
                responseSocket.Connect(ipe);
            }
            try
            {
                byte[] raw = new byte[4096];
                int bytes = responseSocket.Receive(raw);
                GetDescriptorAndInfo(raw, bytes);
                GetDescriptorAndHeath(raw, bytes);
                ReceivedData = "";
                if (GetDescriptor(raw, bytes) == null && lastDescriptor.Length == 5)
                {
                    for (int i = 0; i < bytes / 5; i++)
                    {
                        var index = 5 * i;
                        var new_scan = ((raw[index] >> 0 & 0x1) == 1) ? true : false;
                        var inversed_new_scan = ((raw[index] >> 1 & 0x1) == 1) ? true : false;
                        var quality = raw[index] >> 2;
                        if (new_scan == inversed_new_scan)
                        {
                            Output("New scan flags mismatch");
                            break;
                        }
                        var check_bit = raw[index + 1] >> 0 & 0x1;
                        if (check_bit != 1)
                        {
                            Output("Check bit not equal to 1");
                            break;
                        }
                        var angle = ((raw[index + 1] >> 1) + (raw[index + 2] << 7)) / 64f;
                        var distance = (raw[index + 3] + (raw[index + 4] << 8)) / 4f;

                        ReceivedData += string.Format("{0}, {1}, {2} \n", angle, distance, quality);
                    }
                }
            }
            catch (Exception err)
            {
                Debug.LogError(err.Message);
                break;
            }
        }
    }

    private void OnDestroy()
    {
        commandThread.Abort();
        responseThread.Abort();
        commandSocket.Close();
        responseSocket.Close();
    }

    private string ByteToBit(byte b)
    {
        return ""
                + (byte)((b >> 7) & 0x1) + (byte)((b >> 6) & 0x1)
                + (byte)((b >> 5) & 0x1) + (byte)((b >> 4) & 0x1)
                + (byte)((b >> 3) & 0x1) + (byte)((b >> 2) & 0x1)
                + (byte)((b >> 1) & 0x1) + (byte)((b >> 0) & 0x1);
    }

    private LidarDescriptor GetDescriptor(byte[] raw, int bytes)
    {
        if (bytes != DESCRIPTOR_LEN)
        {
            Output("Descriptor length mismatch");
            return null;
        }
        if (raw[0] != RPLIDAR_ANS_SYNC_BYTE1 || raw[1] != RPLIDAR_ANS_SYNC_BYTE2)
        {
            Output("Incorrect descriptor starting bytes");
            return null;
        }

        lastDescriptor.Length = raw[2] + (raw[3] << 8) + (raw[4] << 16) + ((raw[5] & 0b00111111) << 24);
        lastDescriptor.IsSingle = (raw[5] & 0b11000000) == 0;
        lastDescriptor.Type = raw[6];

        Output(string.Format("Descriptor Length: {0}, Is Single: {1}, Type: {2}", lastDescriptor.Length, lastDescriptor.IsSingle, lastDescriptor.Type));

        return lastDescriptor;
    }

    private LidarInfo GetDescriptorAndInfo(byte[] raw, int bytes)
    {
        if (GetDescriptor(raw, DESCRIPTOR_LEN) != null)
        {
            return GetInfo(raw, bytes, DESCRIPTOR_LEN);
        }
        return GetInfo(raw, bytes);
    }

    private LidarInfo GetInfo(byte[] raw, int bytes, int offset = 0)
    {
        if (bytes != (offset + INFO_LEN))
        {
            Output("Wrong get_info reply length");
            return null;
        }
        if (!lastDescriptor.IsSingle)
        {
            Output("Not a single response mode");
            return null;
        }
        if (lastDescriptor.Type != INFO_TYPE)
        {
            Output("Wrong response data type");
            return null;
        }

        lastInfo.MajorModel = raw[offset + 0] >> 4;
        lastInfo.SubModel = raw[offset + 0] & 0b00001111;
        lastInfo.Firmware = float.Parse(raw[offset + 2] + "." + raw[offset + 1]);
        lastInfo.Hardware = raw[offset + 3];
        lastInfo.SerialNumber = "";
        for (int i = offset + 4; i < offset + INFO_LEN; i++)
        {
            lastInfo.SerialNumber += raw[i].ToString("X2");
        }

        Output(string.Format("Info MajorModel: {0}, SubModel: {1}, Firmware: {2}, Hardware: {3}, SerialNumber: {4}", lastInfo.MajorModel, lastInfo.SubModel, lastInfo.Firmware, lastInfo.Hardware, lastInfo.SerialNumber));

        return lastInfo;
    }

    private LidarHealth GetDescriptorAndHeath(byte[] raw, int bytes)
    {
        if (GetDescriptor(raw, DESCRIPTOR_LEN) != null)
        {
            return GetHeath(raw, bytes, DESCRIPTOR_LEN);
        }
        return GetHeath(raw, bytes);
    }

    private LidarHealth GetHeath(byte[] raw, int bytes, int offset = 0)
    {
        if (bytes != (offset + HEALTH_LEN))
        {
            Output("Wrong get_info reply length");
            return null;
        }
        if (!lastDescriptor.IsSingle)
        {
            Output("Not a single response mode");
            return null;
        }
        if (lastDescriptor.Type != HEALTH_TYPE)
        {
            Output("Wrong response data type");
            return null;
        }

        lastHealth.Status = (LidarHealthStatuses)raw[offset + 0];
        lastHealth.ErrorCode = (raw[offset + 1] << 8) + raw[offset + 2];

        Output(string.Format("Health Status: {0}, Error Code: {1}", lastHealth.Status, lastHealth.ErrorCode));

        return lastHealth;
    }

    private void Output(string msg, LogType type = LogType.Log)
    {
#if UNITY_EDITOR
        if (IsDebug)
        {
            if (type == LogType.Warning)
            {
                Debug.LogWarning(msg);
            }
            else if (type == LogType.Error)
            {
                Debug.LogError(msg);
            }
            else
            {
                Debug.Log(msg);
            }
        }
#endif
    }
}
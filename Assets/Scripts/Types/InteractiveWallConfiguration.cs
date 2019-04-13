using System.Collections.Generic;
using SubjectNerd.Utilities;
using UnityEngine;
using System;

[Serializable]
public class InteractiveWallConfiguration
{
    [Reorderable]
    public List<AreaConfiguration> Areas = new List<AreaConfiguration>();
    [Reorderable]
    public List<string> VideoFilePaths = new List<string>();
    [Reorderable]
    public List<string> TextFilePaths = new List<string>();
}

[Serializable]
public struct AreaConfiguration
{
    public bool EnableLidarMap;
    public Vector2 WallSize;
    public string LidarHost;
    public int LidarPort;
    public Vector2 LidarPosition;
    public Vector3 LidarRotation;
    public float LidarInputMinX;
    public float LidarInputMinY;
    public float LidarInputMaxX;
    public float LidarInputMaxY;
    public float LidarInputMaxDistance;
    public float VideoSwitchSpeed;
    public Vector2 VideoSize;
    public Vector2 VideoOffset;
    public ScaleMode VideoScaleMode;
    public Vector2 TextSize;
    public Vector2 TextOffset;
}

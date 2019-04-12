using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;

public static class AppDataSystem
{
    public static T Save<T>(T appData, string fileName)
    {
        var directory = GetDirectory<T>();
        var fullFilePath = $"{directory}/{fileName}.json";

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (!File.Exists(fullFilePath))
        {
            var fileStream = File.Create(fullFilePath);
            fileStream.Close();
        }

        File.WriteAllText(fullFilePath, JsonUtility.ToJson(appData));
        return appData;
    }

    public static T Load<T>(string fileName) where T : new()
    {
        var fullFilePath = $"{GetDirectory<T>()}/{fileName}.json";

        if (!File.Exists(fullFilePath)) return Save(new T(), $"{fileName}");

        var appData = JsonUtility.FromJson<T>(File.ReadAllText(fullFilePath));
        return appData;
    }

    public static List<T> LoadAll<T>() where T : new()
    {
        var directoryInfo = new DirectoryInfo($"{GetDirectory<T>()}/");
        var fileInfos = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories).Where(fi => !fi.Name.Contains("meta")).ToList();

        return fileInfos.Select(t => JsonUtility.FromJson<T>(File.ReadAllText(t.FullName))).ToList();
    }

    private static string GetDirectory<T>()
    {
        var appDataDirectory = Application.dataPath;
        var streamingAssetsDirectory = GetStreamingAssetsPath();
        var configurationFileDirectory = $"{appDataDirectory}/{streamingAssetsDirectory}/ProjectPathConfigurations";
        var fullFilePath = $"{configurationFileDirectory}/ProjectPathConfiguration.json";

        if (!Directory.Exists(configurationFileDirectory))
        {
            Directory.CreateDirectory(configurationFileDirectory);
        }

        var streamWriter = new StreamWriter(fullFilePath);
        streamWriter.WriteAsync(JsonUtility.ToJson(new ProjectPathConfiguration(streamingAssetsDirectory, "Assets/Scenes"))).Wait();
        streamWriter.Close();

        var configurationFile = JsonUtility.FromJson<ProjectPathConfiguration>(File.ReadAllText(fullFilePath));

        if (configurationFile != null) return $"{appDataDirectory}/{configurationFile.ResourceDataPath}/{typeof(T)}s";

        return $"{appDataDirectory}/{streamingAssetsDirectory}/{typeof(T)}s";
    }

    public static string GetStreamingAssetsPath()
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            return "Raw";
        }

        return "StreamingAssets";
    }
}
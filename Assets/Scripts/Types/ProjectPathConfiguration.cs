using System;

[Serializable]
public class ProjectPathConfiguration
{
    public string ResourceDataPath;
    public string ScenesPath;

    public ProjectPathConfiguration(string resourceDataPath, string scenesPath)
    {
        ResourceDataPath = resourceDataPath;
        ScenesPath = scenesPath;
    }

    public ProjectPathConfiguration() { }
}
using UnityEditor.Build.Reporting;
using System.Collections.Generic;
using UnityEditor.Callbacks;
using EditorUtilities;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.IO;
using System;

public class BuildSystem
{
    private const string BUILD_PLAYER_PREFERENCES = "-buildPlayerPreference";
    private const string BUILD_NUMBER = "-buildNumber";
    private static readonly List<BuildPlayerPreference> buildPlayerPreferences = Resources.LoadAll<BuildPlayerPreference>("Data/BuildPlayerPreferences").ToList();
    private static BuildPlayerPreference buildPlayerPreference;

    public static void Build()
    {
        var commandLineArguments = Environment.GetCommandLineArgs();
        var scenes = new List<string>();
        var buildNumber = "";

        for (int i = 1; i < commandLineArguments.Length; i++)
        {
            if (commandLineArguments[i - 1].Equals(BUILD_PLAYER_PREFERENCES))
            {
                buildPlayerPreference = buildPlayerPreferences.FirstOrDefault(p => p.name == commandLineArguments[i]);
            }

            if (commandLineArguments[i - 1].Equals(BUILD_NUMBER))
            {
                buildNumber = commandLineArguments[i];
            }
        }

        if (buildPlayerPreference == null)
            return;

        foreach (var scene in buildPlayerPreference.Scenes)
        {
            scenes.Add(scene.ScenePath);
        }

        var buildPath = GenerateBuildPath(buildPlayerPreference, int.Parse(buildNumber));

        var buildReport = BuildPipeline.BuildPlayer(scenes.ToArray(), buildPath, buildPlayerPreference.BuildTarget.ToBuildTarget(), BuildOptions.None);

#if UNITY_2018_1_OR_NEWER
        if (buildReport.summary.result == BuildResult.Failed)
        {
            EditorApplication.Exit(1);
        }
        else
        {
            EditorApplication.Exit(0);

        }
#else
        if (string.IsNullOrEmpty(buildResult))
        {
            EditorApplication.Exit(0);
        }
        else
        {
            EditorApplication.Exit(1);
        }

        if (!string.IsNullOrEmpty(buildResult)) throw new Exception("Build failed: " + buildResult);
#endif
    }

    private static string GenerateBuildPath(BuildPlayerPreference buildPlayerPreference, int buildNumber)
    {
        var buildPath = "";

        if (buildPlayerPreference.BuildTarget.ToBuildTarget() == BuildTarget.StandaloneWindows64)
        {
            buildPath = buildPlayerPreference.BuildDirectory + buildNumber + "/" + "Volvo" + buildPlayerPreference.name + buildNumber + ".exe";
        }
        else if (buildPlayerPreference.BuildTarget.ToBuildTarget() == BuildTarget.iOS)
        {
            buildPath = buildPlayerPreference.BuildDirectory;
        }

        return buildPath;
    }

    [PostProcessBuild(999)]
    public static void DeleteUnusedStreamingAssets(BuildTarget target, string pathToBuiltProject)
    {
        if (target == BuildTarget.iOS) return;

        var buildDataSuffix = GetBuildDataSuffix(target);

        var buildPath = Path.Combine(Path.GetDirectoryName(pathToBuiltProject), Path.GetFileNameWithoutExtension(pathToBuiltProject) + buildDataSuffix);// + "/" + resourcesPath;
        buildPath += "/" + GetStreamingAssetsPath(target);
        buildPath = buildPath.Replace("\\", "/");
        var directories = Directory.GetDirectories(buildPath).ToList();

        if (buildPlayerPreference == null)
        {
            foreach (var scene in EditorBuildSettings.scenes)
            {
                buildPlayerPreference = buildPlayerPreferences.FirstOrDefault(p => p.Scenes.Any(sp => sp.ScenePath == scene.path));

                if (buildPlayerPreference == null) return;

                break;
            }
        }

        foreach (var streamingAssetDirectory in buildPlayerPreference.StreamingAssetDirectories)
        {
            for (var i = 0; i < directories.Count; i++)
            {
                if (directories[i].Contains(streamingAssetDirectory))
                {
                    directories.Remove(directories[i]);
                }
            }
        }

        foreach (var directory in directories)
        {
            Debug.Log("Deleting: " + directory);
            Directory.Delete(directory, true);
        }
    }

    private static string GetBuildDataSuffix(BuildTarget target)
    {
        if (target != BuildTarget.iOS) return "_Data";

        return "Data";
    }

    private static string GetStreamingAssetsPath(BuildTarget target)
    {
        if (target != BuildTarget.iOS) return "StreamingAssets";

        return "Raw";
    }
}
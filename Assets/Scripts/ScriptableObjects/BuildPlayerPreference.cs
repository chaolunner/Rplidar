using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "BuildPlayerPreference")]
public class BuildPlayerPreference : ScriptableObject
{
    public BuildTargetType BuildTarget;
    public string BuildDirectory;
    public List<SceneReference> Scenes = new List<SceneReference>();
    public List<string> StreamingAssetDirectories = new List<string>();
}

public enum BuildTargetType
{
    NoTarget = -2,
    //
    // Summary:
    //     OBSOLETE: Use iOS. Build an iOS player.
    iPhone = -1,
    BB10 = -1,
    MetroPlayer = -1,
    //
    // Summary:
    //     Build a macOS standalone (Intel 64-bit).
    StandaloneOSX = 2,
    StandaloneOSXUniversal = 2,
    //
    // Summary:
    //     Build a macOS Intel 32-bit standalone. (This build target is deprecated)
    StandaloneOSXIntel = 4,
    //
    // Summary:
    //     Build a Windows standalone.
    StandaloneWindows = 5,
    //
    // Summary:
    //     Build a web player. (This build target is deprecated. Building for web player
    //     will no longer be supported in future versions of Unity.)
    WebPlayer = 6,
    //
    // Summary:
    //     Build a streamed web player.
    WebPlayerStreamed = 7,
    //
    // Summary:
    //     Build an iOS player.
    iOS = 9,
    PS3 = 10,
    XBOX360 = 11,
    //
    // Summary:
    //     Build an Android .apk standalone app.
    Android = 13,
    //
    // Summary:
    //     Build a Linux standalone.
    StandaloneLinux = 17,
    //
    // Summary:
    //     Build a Windows 64-bit standalone.
    StandaloneWindows64 = 19,
    //
    // Summary:
    //     WebGL.
    WebGL = 20,
    //
    // Summary:
    //     Build an Windows Store Apps player.
    WSAPlayer = 21,
    //
    // Summary:
    //     Build a Linux 64-bit standalone.
    StandaloneLinux64 = 24,
    //
    // Summary:
    //     Build a Linux universal standalone.
    StandaloneLinuxUniversal = 25,
    WP8Player = 26,
    //
    // Summary:
    //     Build a macOS Intel 64-bit standalone. (This build target is deprecated)
    StandaloneOSXIntel64 = 27,
    BlackBerry = 28,
    Tizen = 29,
    //
    // Summary:
    //     Build a PS Vita Standalone.
    PSP2 = 30,
    //
    // Summary:
    //     Build a PS4 Standalone.
    PS4 = 31,
    PSM = 32,
    //
    // Summary:
    //     Build a Xbox One Standalone.
    XboxOne = 33,
    SamsungTV = 34,
    //
    // Summary:
    //     Build to Nintendo 3DS platform.
    N3DS = 35,
    WiiU = 36,
    //
    // Summary:
    //     Build to Apple's tvOS platform.
    tvOS = 37,
    //
    // Summary:
    //     Build a Nintendo Switch player.
    Switch = 38
}
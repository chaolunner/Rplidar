using RenderHeads.Media.AVProVideo;
using System.Collections.Generic;
using AlphaECS.Unity;
using UnityEngine;

public class VideoControlComponent : ComponentBehaviour
{
    public int CurrentIndex;
    public ScaleMode ScaleMode;
    public List<string> VideoFilePaths = new List<string>();
    [HideInInspector] public List<DisplayUGUI> DisplayUGUIs = new List<DisplayUGUI>();
    [HideInInspector] public List<MediaPlayer> MediaPlayers = new List<MediaPlayer>();
}

using System.Collections.Generic;
using AlphaECS.Unity;
using UnityEngine;
using UniRx;

public class ContentSwitchComponent : ComponentBehaviour
{
    [HideInInspector] public bool IsReady;
    [HideInInspector] public bool IsPlaying;
    public int ID;
    public int TargetDisplay;
    public float Speed = 1;
    public AnimationCurve EaseCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public Vector2 Size;
    public Vector2 Offset;
    public Vector2 Direction;
    public Vector2ReactiveProperty ScreenSize;
    [HideInInspector] public List<GameObject> Contents = new List<GameObject>();
}

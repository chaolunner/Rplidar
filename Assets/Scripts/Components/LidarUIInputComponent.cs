using AlphaECS.Unity;
using UnityEngine;

public class LidarUIInputComponent : ComponentBehaviour
{
    public int ID;
    public int TargetDisplay;
    public LidarInput2D LidarInput2D;
    public GameObject LidarUIPointPrefab;
    public GameObject LiderInputScopePrefab;
    public Vector2 Size;
    public Vector2 Offset;
    public Quaternion Rotation;
}

using UnityEngine;

public class LidarDragEvent
{
    public int ID;
    public GameObject Target;
    public Vector2 Position;
    public Vector2 Distance;

    public LidarDragEvent(int id, GameObject target, Vector2 position, Vector2 distance)
    {
        ID = id;
        Target = target;
        Position = position;
        Distance = distance;
    }
}

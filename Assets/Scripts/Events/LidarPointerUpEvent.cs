using UnityEngine;

public class LidarPointerUpEvent
{
    public int ID;
    public GameObject Target;
    public Vector2 Position;

    public LidarPointerUpEvent(int id, GameObject target, Vector2 position)
    {
        ID = id;
        Target = target;
        Position = position;
    }
}

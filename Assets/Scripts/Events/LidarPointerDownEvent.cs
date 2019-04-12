using UnityEngine;

public class LidarPointerDownEvent
{
    public int ID;
    public GameObject Target;
    public Vector2 Position;

    public LidarPointerDownEvent(int id, GameObject target, Vector2 position)
    {
        ID = id;
        Target = target;
        Position = position;
    }
}

using AlphaECS.Unity;

public enum Tag
{
    Video,
    Text,
}

public class TagComponent : ComponentBehaviour
{
    public Tag Tag;
}

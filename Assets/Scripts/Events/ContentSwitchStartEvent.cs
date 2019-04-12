public class ContentSwitchStartEvent
{
    public ContentSwitchComponent ContentSwitchComponent;
    public int Previous;
    public int Current;

    public ContentSwitchStartEvent(ContentSwitchComponent contentSwitchComponent, int previous, int current)
    {
        ContentSwitchComponent = contentSwitchComponent;
        Previous = previous;
        Current = current;
    }
}

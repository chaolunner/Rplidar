public class ContentSwitchCompletedEvent
{
    public ContentSwitchComponent ContentSwitchComponent;
    public int Previous;
    public int Current;

    public ContentSwitchCompletedEvent(ContentSwitchComponent contentSwitchComponent, int previous, int current)
    {
        ContentSwitchComponent = contentSwitchComponent;
        Previous = previous;
        Current = current;
    }
}

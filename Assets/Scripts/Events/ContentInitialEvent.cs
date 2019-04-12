public class ContentInitialEvent
{
    public ContentSwitchComponent ContentSwitchComponent;
    public int Previous;
    public int Current;

    public ContentInitialEvent(ContentSwitchComponent contentSwitchComponent, int previous, int current)
    {
        ContentSwitchComponent = contentSwitchComponent;
        Previous = previous;
        Current = current;
    }
}

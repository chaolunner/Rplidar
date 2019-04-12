public class ContentSwitchStayEvent
{
    public ContentSwitchComponent ContentSwitchComponent;
    public int Previous;
    public int Current;

    public ContentSwitchStayEvent(ContentSwitchComponent contentSwitchComponent, int previous, int current)
    {
        ContentSwitchComponent = contentSwitchComponent;
        Previous = previous;
        Current = current;
    }
}

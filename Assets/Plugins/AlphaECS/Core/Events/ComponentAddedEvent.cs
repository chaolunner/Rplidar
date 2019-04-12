namespace AlphaECS
{
    public class ComponentAddedEvent
    {
        public IEntity Entity { get; private set; }
		public object Component { get; private set; }

		public ComponentAddedEvent(IEntity entity, object component)
        {
            Entity = entity;
            Component = component;
        }
    }
}
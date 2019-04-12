using System;
using System.Collections.Generic;
using UniRx;

namespace AlphaECS
{
    public class Pool : IPool
    {
        private readonly IList<IEntity> _entities;

        public string Name { get; private set; }
        public IEnumerable<IEntity> Entities { get { return _entities;} }
        public IIdentityGenerator IdentityGenerator { get; private set; }
        public IEventSystem EventSystem { get; private set; }

        public Pool(string name, IIdentityGenerator identityGenerator, IEventSystem eventSystem)
        {
            _entities = new List<IEntity>();
            Name = name;
            IdentityGenerator = identityGenerator;
            EventSystem = eventSystem;
        }

		public IEntity CreateEntity(IBlueprint blueprint = null)
		{
			return CreateEntity(Guid.NewGuid().ToString(), blueprint);
		}

		public IEntity CreateEntity(string id = null, IBlueprint blueprint = null)
		{
			id = !string.IsNullOrEmpty(id) ? id : Guid.NewGuid ().ToString ();
			var entity = new Entity(id, EventSystem);

			_entities.Add(entity);

			EventSystem.Publish(new EntityAddedEvent(entity, this));

			if (blueprint != null)
			{ blueprint.Apply(entity); }

			return entity;
		}

        public void RemoveEntity(IEntity entity)
        {
			_entities.Remove(entity);
			entity.Dispose();
			EventSystem.Publish(new EntityRemovedEvent(entity, this));
        }
    }
}

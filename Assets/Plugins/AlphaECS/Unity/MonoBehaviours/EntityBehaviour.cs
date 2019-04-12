using SimpleJSON;
//using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
//using SubjectNerd.Utilities;

namespace AlphaECS.Unity
{
	public class EntityBehaviour : ComponentBehaviour
	{
		public IPool Pool
		{
			get
			{
				if (Proxy != null)
				{
					return Proxy.Pool;
				}
				else if (pool != null)
				{
					return pool;
				}
				else if (string.IsNullOrEmpty(PoolName))
				{
					return (pool = PoolManager.GetPool());
				}
				else if (PoolManager.Pools.All(x => x.Name != PoolName))
				{
					return (pool = PoolManager.CreatePool(PoolName));
				}
				else
				{
					return (pool = PoolManager.GetPool(PoolName));
				}
			}
			set { pool = value; }
		}
		private IPool pool;

		public IEntity Entity
		{
			get
			{
				if (Proxy != null)
				{
					return Proxy.Entity;
				}
				return entity == null ? (entity = Pool.CreateEntity(Id)) : entity;
			}
			set
			{
				// TODO add some logic for killing rogue entities
				if (entity != null)
				{
//					Pool.RemoveEntity (entity); ???
				}
				entity = value;
			}
		}
		private IEntity entity;

		[SerializeField]
		public EntityBehaviour Proxy;

		[SerializeField] [HideInInspector]
		public string Id;

		[SerializeField] [HideInInspector]
		public string PoolName;

		[SerializeField] [HideInInspector]
		public bool RemoveEntityOnDestroy = true;

		[SerializeField] [HideInInspector]
		public UnityEngine.Object[] Components;

		[SerializeField] [HideInInspector]
        public List<BlueprintBase> Blueprints = new List<BlueprintBase>();

		public IPoolManager PoolManager
		{
			get
			{
				return poolManager == null ? ProjectContext.Instance.Container.Resolve<IPoolManager>() : poolManager;
			}
			set { poolManager = value; }
		}
		private IPoolManager poolManager;

        //TODO -> previously, we had this in awake to get around an issue where entities present in the scene that never had Awake() called
        //we not properly being disposed of -> OnDestroy + Disposer.Dispose() is not called
        //however, this made setup not so straightforward when instantiating entities from prefabs
        //need to work out a proper solution for both cases
        public override void Initialize(IEventSystem eventSystem)
        {
            base.Initialize(eventSystem);

            if (!Entity.HasComponent<ViewComponent>())
            {
                var viewComponent = new ViewComponent();
                AddTransformToView(viewComponent);
                Entity.AddComponent(viewComponent);
            }
            else
            {
                AddTransformToView(Entity.GetComponent<ViewComponent>());
            }

            foreach (var blueprint in Blueprints)
            {
                blueprint.Apply(this.Entity);
            }

            if (Components != null && Components.Length > 0)
            {
                Entity.AddComponents(Components.Select(c => c as object).Where(o => o != null).ToArray());
            }
            else
            {
                var monoBehaviours = GetComponents<Component>().Where(c => c != null && c.GetType() != typeof(Transform) && c.GetType() != typeof(EntityBehaviour)).ToArray();
                Entity.AddComponents(monoBehaviours);
            }
        }

        public override void OnDestroy()
		{
			if (EcsApplication.IsQuitting) { return; }
			if (!RemoveEntityOnDestroy) { return; }
			if (Proxy != null) { return; }

			this.Pool.RemoveEntity (Entity);

			base.OnDestroy();
		}

		private void AddTransformToView(ViewComponent viewComponent)
		{
			//ensure that the root EntityBehaviour's transform is first
			if (Proxy == null)
			{
				viewComponent.Transforms.Insert (0, this.transform);
			}
			else
			{
				viewComponent.Transforms.Add(this.transform);
			}
		}
	}
}
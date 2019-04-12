using System;
using System.Collections;
using UniRx;
using UnityEngine;
using Zenject;
using AlphaECS;
using System.Collections.Generic;

namespace AlphaECS.Unity
{
    public abstract class SystemBehaviour : MonoBehaviour, ISystem, IDisposableContainer, IDisposable
    {
        public IEventSystem EventSystem { get; set; }
		public IPoolManager PoolManager { get; set; }
		public GroupFactory GroupFactory { get; set; }

        protected DiContainer Container { get { return ProjectContext.Instance.Container; } }

        [Inject]
        public PrefabFactory PrefabFactory { get; set; }

		[Inject]
		public FastString StringFactory { get; set; }

        private CompositeDisposable _disposer = new CompositeDisposable();
        public CompositeDisposable Disposer
        {
            get { return _disposer; }
            private set { _disposer = value; }
        }

		private List<Group> groups = new List<Group>();
			
        [Inject]
		public virtual void Initialize(IEventSystem eventSystem, IPoolManager poolManager, GroupFactory groupFactory)
        {
			EventSystem = eventSystem;
			PoolManager = poolManager;
			GroupFactory = groupFactory;
        }

		public virtual void OnEnable() { }

		public virtual void OnDisable()
		{
			//if (EcsApplication.IsQuitting) { return; }
			Disposer.Clear ();
		}

		public Group CreateGroup(Type[] types)
		{
			var group = GroupFactory.Create (types);
			groups.Add (group);
			return group;
		}

		//public Group CreateGroup(HashSet<Type> types)
		//{
		//	var group = GroupFactory.Create (types);
		//	groups.Add (group);
		//	return group;
		//}

        public Group CreateGroup(HashSet<Type> types, params Func<IEntity, IReadOnlyReactiveProperty<bool>>[] predicates)
        {
            var group = GroupFactory
                .WithPredicates(predicates)
                .Create(types);
            groups.Add(group);
            return group;
        }

        public virtual void OnDestroy()
		{
			//if (EcsApplication.IsQuitting) { return; }
			Dispose();
		}

        public virtual void Dispose()
        {
            Disposer.Clear();
			foreach (var group in groups)
			{ group.Dispose (); }
            Disposer.Dispose();
        }

		public virtual void OnApplicationQuit()
		{
			EcsApplication.IsQuitting = true;
            Dispose();
        }
    }
}

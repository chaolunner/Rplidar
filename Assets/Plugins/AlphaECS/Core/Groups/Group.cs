using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using Zenject;
using UnityEngine;
using System.Linq;

namespace AlphaECS
{
	public class Group : IGroup, IDisposableContainer, IDisposable
    {
		public IEventSystem EventSystem { get; set; }
		public IPool EntityPool { get; set; }

		public string Name { get; set; }

		private ReactiveCollection<IEntity> cachedEntities = new ReactiveCollection<IEntity>();
		private HashSet<IEntity> cachedEntitySet = new HashSet<IEntity>();

		ReactiveCollection<IEntity> _entities = new ReactiveCollection<IEntity>();
		public ReactiveCollection<IEntity> Entities
		{
			get { return _entities; }
			set { _entities = value; }
		}

		public HashSet<Type> Components { get; set; }

        protected List<Func<IEntity, IReadOnlyReactiveProperty<bool>>> _predicates = new List<Func<IEntity, IReadOnlyReactiveProperty<bool>>> ();
        public List<Func<IEntity, IReadOnlyReactiveProperty<bool>>> Predicates
		{
			get { return _predicates; }
			private set { _predicates = value; }
		}
		protected Dictionary<IEntity, IDisposable> entityPredicateTable = new Dictionary<IEntity, IDisposable> ();

		protected CompositeDisposable _disposer = new CompositeDisposable();
		public CompositeDisposable Disposer
		{
			get { return _disposer; }
			private set { _disposer = value; }
		}

        public Group(){}

        public Group(Type[] components, List<Func<IEntity, IReadOnlyReactiveProperty<bool>>> predicates)
        {
			Components = new HashSet<Type>(components);

			foreach (var predicate in predicates)
			{
				Predicates.Add (predicate);
			}
        }

        public Group(HashSet<Type> components, List<Func<IEntity, IReadOnlyReactiveProperty<bool>>> predicates)
		{
			Components = components;

			foreach (var predicate in predicates)
			{
				Predicates.Add (predicate);
			}
		}

		[Inject]
		public virtual void Initialize(IEventSystem eventSystem, IPoolManager poolManager)
		{
			EventSystem = eventSystem;
			EntityPool = poolManager.GetPool ();

			cachedEntities.ObserveAdd ().Select (x => x.Value).Subscribe (entity =>
			{
				cachedEntitySet.Add(entity);

				if(Predicates.Count == 0)
				{
					PreAdd(entity);
					AddEntity(entity);
					return;
				}
				
                var bools = new List<IReadOnlyReactiveProperty<bool>>();
				foreach (var predicate in Predicates)
				{
					bools.Add(predicate.Invoke(entity));
				}
				var onLatest = Observable.CombineLatest(bools.ToArray());
				var predicateDisposable = onLatest.DistinctUntilChanged().Subscribe(values =>
				{
					if(values.All(value => value == true))
					{
						PreAdd(entity);
						AddEntity(entity);
					}
					else if(Entities.Contains(entity))
					{
						PreRemove(entity);
						RemoveEntity(entity);
					}
				}).AddTo (this.Disposer);
				entityPredicateTable.Add(entity, predicateDisposable);
			}).AddTo (this.Disposer);

			cachedEntities.ObserveRemove ().Select (x => x.Value).Subscribe (entity =>
			{
                cachedEntitySet.Remove(entity);

				if(entityPredicateTable.ContainsKey(entity))
				{
					entityPredicateTable[entity].Dispose();
					entityPredicateTable.Remove(entity);
				}

                if (Entities.Contains(entity))
                {
                    PreRemove(entity);
                    RemoveEntity(entity);
                }
			}).AddTo (this.Disposer);

			foreach (IEntity entity in EntityPool.Entities)
			{
				if (entity.HasComponents (Components))
				{
					cachedEntities.Add(entity);
				}
			}

			EventSystem.OnEvent<EntityAddedEvent> ().Subscribe (evt =>
			{
				if(!cachedEntitySet.Contains(evt.Entity) && evt.Entity.HasComponents (Components))
				{
					cachedEntities.Add(evt.Entity);
				}
			}).AddTo(this.Disposer);

			EventSystem.OnEvent<EntityRemovedEvent> ().Subscribe (evt =>
			{
				if(cachedEntitySet.Contains(evt.Entity))
				{
					cachedEntities.Remove(evt.Entity);
				}
			}).AddTo(this.Disposer);

			EventSystem.OnEvent<ComponentAddedEvent> ().Subscribe (evt =>
			{
				if(Components.Contains(evt.Component.GetType()) && !cachedEntitySet.Contains(evt.Entity) && evt.Entity.HasComponents (Components))
				{
					cachedEntities.Add(evt.Entity);
				}
			}).AddTo(this.Disposer);

            EventSystem.OnEvent<ComponentsAddedEvent> ().Subscribe (evt =>
			{
				if(!cachedEntitySet.Contains(evt.Entity) && evt.Entity.HasComponents (Components))
				{
					cachedEntities.Add(evt.Entity);
				}
			}).AddTo(this.Disposer);

			EventSystem.OnEvent<ComponentRemovedEvent> ().Subscribe (evt =>
			{
				if(Components.Contains(evt.Component.GetType()) && cachedEntitySet.Contains(evt.Entity))
				{
					cachedEntities.Remove(evt.Entity);
				}
			}).AddTo(this.Disposer);
		}

		protected void AddEntity(IEntity entity)
		{
			Entities.Add (entity);
		}

		protected void RemoveEntity(IEntity entity)
		{
			Entities.Remove (entity);
		}

//		protected bool IsCached(IEntity entity)
//		{
//			for (var i = 0; i < cachedEntities.Count; i++)
//			{
//				if (cachedEntities [i] == entity)
//				{
//					return true;
//				}
//			}
//			return false;
//		}
			
		public void Dispose ()
		{
			foreach (var kvp in entityPredicateTable)
			{ kvp.Value.Dispose (); }
			entityPredicateTable.Clear ();
			Disposer.Dispose ();
		}

		Subject<IEntity> onPreAdd;

		protected virtual void PreAdd(IEntity entity)
		{
			if (onPreAdd != null) onPreAdd.OnNext (entity);
		}

		public IObservable<IEntity> OnPreAdd()
		{
			return onPreAdd ?? (onPreAdd = new Subject<IEntity> ());
		}

		Subject<IEntity> onPreRemove;

		protected virtual void PreRemove(IEntity entity)
		{
			if (onPreRemove != null) onPreRemove.OnNext (entity);
		}

		public IObservable<IEntity> OnPreRemove()
		{
			return onPreRemove ?? (onPreRemove = new Subject<IEntity> ());
		}
    }
}
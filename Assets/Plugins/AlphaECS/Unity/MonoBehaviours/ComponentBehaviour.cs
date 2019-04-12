using System;
using System.Collections;
using UniRx;
using UnityEngine;
using Zenject;

namespace AlphaECS.Unity
{
    public abstract class ComponentBehaviour : MonoBehaviour, IDisposable
    {
        protected IEventSystem EventSystem { get; set; }

        private CompositeDisposable _disposer = new CompositeDisposable();
        public CompositeDisposable Disposer
        {
            get { return _disposer; }
            private set { _disposer = value; }
        }
			
        public virtual void OnDestroy()
        {
            Dispose();

            if (EcsApplication.IsQuitting) { return; }

            if (EventSystem == null)
			{
				if (Debug.isDebugBuild)
				{
					Debug.LogWarning ("A COMPONENT ON " + this.gameObject.name + " WAS NOT INJECTED PROPERLY!");
				}
				return;
			}
            EventSystem.Publish(new ComponentDestroyed() { Component = this });
        }

        [Inject]
		public virtual void Initialize(IEventSystem eventSystem)
        {
			EventSystem = eventSystem;
            EventSystem.Publish(new ComponentCreated() { Component = this });
        }

        public virtual void Dispose()
        {
            Disposer.Clear();
            Disposer.Dispose();
        }

        public virtual void OnApplicationQuit()
        {
			EcsApplication.IsQuitting = true;
        }
    }
}

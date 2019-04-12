using AlphaECS;
using AlphaECS.Unity;
using UniRx;
using System;
using UnityEngine;
using System.Collections.Generic;

namespace AlphaECS.Unity
{
    [Serializable]
	//public class ComponentBase : ScriptableObject, IComponent, IDisposable, IDisposableContainer
	public class ComponentBase : IComponent, IDisposable, IDisposableContainer
	{
		private CompositeDisposable _disposer = new CompositeDisposable();
		public CompositeDisposable Disposer
		{
			get { return _disposer; }
			set { _disposer = value; }
		}

		public void Dispose()
		{
			Disposer.Dispose();
		}

		//public ComponentBase() { }
	}
}

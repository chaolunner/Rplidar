using AlphaECS;
using AlphaECS.Unity;
using UniRx;
using System;
using UnityEngine;
using System.Collections.Generic;

namespace AlphaECS.Unity
{
    public class ViewComponent : ComponentBase
	{
		public ReactiveCollection<Transform> Transforms = new ReactiveCollection<Transform>();
		public ViewComponent() { }
	}
}

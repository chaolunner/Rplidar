using System;
using UnityEngine;
using AlphaECS;

namespace UniRx
{
    [Serializable]
    public class GameObjectReactiveProperty : ReactiveProperty<GameObject>
    {
        public GameObjectReactiveProperty() { }
        public GameObjectReactiveProperty(GameObject initialValue) : base(initialValue) { }
    }

    [Serializable]
    public class TransformReactiveProperty : ReactiveProperty<Transform>
    {
        public TransformReactiveProperty() { }
        public TransformReactiveProperty(Transform initialValue) : base(initialValue) { }
    }

    [Serializable]
    public class EntityReactiveProperty : ReactiveProperty<IEntity>
    {
        public EntityReactiveProperty() { }
        public EntityReactiveProperty(IEntity initialValue) : base(initialValue) { }
    }
}
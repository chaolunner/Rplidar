using System;
using System.Collections.Generic;
using UniRx;

namespace AlphaECS
{
    public static class IGroupExtensions
    {
//        public static IGroup WithComponent<T>(this IGroup group) where T : class, IComponent
//        {
//            var componentTypes = new List<Type>(group.TargettedComponents);
//            componentTypes.Add(typeof(T));
//            return new Group(componentTypes.ToArray());
//        }

		public static IObservable<IEntity> OnAdd(this IGroup group)
		{ return group.Entities.ObserveAdd ().Select (x => x.Value).StartWith (group.Entities); }

		public static IObservable<IEntity> OnRemove(this IGroup group)
		{ return group.Entities.ObserveRemove ().Select (x => x.Value); }
    }
}
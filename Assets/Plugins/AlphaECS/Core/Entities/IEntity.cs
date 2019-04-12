using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AlphaECS
{
	public interface IEntity : IDisposable
    {
        string Id { get; }
		IEnumerable<object> Components { get; }

		object AddComponent(object component);
		T AddComponent<T> () where T : class, new();
        object[] AddComponents(params object[] components);
		void RemoveComponent(object component);
        void RemoveComponent<T>() where T : class;
        void RemoveAllComponents();
        T GetComponent<T>() where T : class;
		object GetComponent (Type type);

        bool HasComponent<T>() where T : class;
		bool HasComponent(Type componentType);
        bool HasComponents(params Type[] components);
		bool HasComponents(HashSet<Type> components);
    }
}

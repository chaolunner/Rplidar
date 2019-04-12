using System;
using UnityEngine;
using System.Linq;
using Zenject;

namespace AlphaECS.Unity
{
	public static class GameObjectExtensions
	{
		public static void BindToEntity(this GameObject gameObject, IEntity entity, IPool pool)
		{
			if(gameObject.GetComponent<EntityBehaviour>())
			{ throw new Exception("GameObject already has an EntityBehaviour monobehaviour applied"); }

//            if(entity.HasComponents(typeof(EntityBehaviour))
//            { throw new Exception("Entity already has a EntityBehaviour monobehaviour applied"); }

			var entityBehaviour = gameObject.AddComponent<EntityBehaviour>();
			entityBehaviour.Entity = entity;
			entityBehaviour.Pool = pool;
		}

		/*
		 * OnDestroy() only gets called for gameObjects that have previously been active
		 * however Setup() is called on all EntityBehaviours under a SceneContext
		 * so some entities could be setup correctly but then not properly disposed when destroyed
		 * i think it's correct for Setup() to work this way (scene as configuration)
		 * so for consistency we should ensure cleanup happens by enabling and then resetting the active state
		*/
		public static void ForceEnable(this GameObject gameObject)
		{
			if (gameObject.activeInHierarchy) { return; }

			var isActive = gameObject.activeSelf;
			var parent = gameObject.transform.parent;
            var index = gameObject.transform.GetSiblingIndex();

			gameObject.transform.SetParent (ProjectContext.Instance.Container.DefaultParent, true);
			gameObject.SetActive (true);
			gameObject.SetActive (isActive);
			gameObject.transform.SetParent (parent, true);
            gameObject.transform.SetSiblingIndex(index);
		}

//		//if a parent is disabled and the child enabled, the above won't work
//		public static void ForceEnable(this GameObject[] gameObjects)
//		{
//			var isActive = gameObjects.Select (go => go.activeSelf).ToArray();
//
//			for (var i = 0; i < gameObjects.Length; i++)
//			{
//				gameObjects[i].SetActive (true);
//			}
//
//			for (var i = 0; i < gameObjects.Length; i++)
//			{
//				gameObjects[i].SetActive (isActive[i]);
//			}
//		}
	}
}

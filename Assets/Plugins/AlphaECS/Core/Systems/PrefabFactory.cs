using UnityEngine;
using Zenject;
using AlphaECS.Unity;
using System;

namespace AlphaECS
{
    public class PrefabFactory
    {
        [Inject]
        protected DiContainer Container = null;
		[Inject]
		protected IEventSystem EventSystem = null;

//		public GameObject Instantiate(GameObject prefab)
//		{
//			return Instantiate(prefab, null);
//		}
//
//        public GameObject Instantiate(GameObject prefab, Transform parent)
//        {
//			return Instantiate(prefab, parent, true);
//        }
//
//		public GameObject Instantiate(GameObject prefab, bool fastInject)
//		{
//			return Instantiate(prefab, fastInject, null);
//		}

        //TODO -> add in proper overloads for setting passing in position, rotation, etc
        public GameObject Instantiate(GameObject prefab, Transform parent = null, bool fastInject = true)
        {
            GameObject gameObject = null;
            var wasActive = prefab.activeSelf;
            if (fastInject)
            {
                gameObject = InstantiatePrefab(prefab, parent);
                Inject(gameObject);
                if (wasActive)
                {
                    gameObject.SetActive(true);
                }
            }
            else
            {
                gameObject = parent == null ? Container.InstantiatePrefab(prefab) : Container.InstantiatePrefab(prefab, parent);
                gameObject.name = prefab.name;
            }

            if (!gameObject.activeInHierarchy)
            { gameObject.ForceEnable(); }

            return gameObject;
        }

        //public GameObject Instantiate(IEntity entity, GameObject prefab, Transform parent)
        //{
		//	return Instantiate(entity, prefab, parent, true);
        //}

        //public GameObject Instantiate(IEntity entity, GameObject prefab, Transform parent = null, bool fastInject = true)
        //{
        //    return Instantiate(entity, prefab, parent, true);
        //}

		public GameObject Instantiate(IEntity entity, GameObject prefab, Transform parent = null, bool fastInject = true, string data = "")
		{
            var wasActive = prefab.activeSelf;
			var gameObject = InstantiatePrefab(prefab, parent);

			if (!gameObject.GetComponent<EntityBehaviour>())
			{
				throw new Exception("GameObject has no EntityBehaviour monobehaviour to link to!");
			}

			var entityBehaviour = gameObject.GetComponent<EntityBehaviour>();
			entityBehaviour.Entity = entity;
            //TODO!!!
			//entityBehaviour.RemoveEntityOnDestroy = false;

            if(!string.IsNullOrEmpty(data))
            {
                entity.Deserialize(data);
            }

			if (fastInject)
			{
				Inject (gameObject);
                if (wasActive)
                {
                    gameObject.SetActive(true);
                }
            }
			else
			{
				Container.InjectGameObject(gameObject);
			}

            if (!gameObject.activeInHierarchy)
            { gameObject.ForceEnable(); }

            return gameObject;
		}

        private GameObject InstantiatePrefab(GameObject prefab, Transform parent)
        {
            var wasActive = prefab.activeSelf;
            if (wasActive)
            {
#if UNITY_EDITOR //HACK -> if we're in the editor we'll create an instance of the prefab before calling .SetActive(false) so that it isn't marked as dirty in source control
                Container.DefaultParent.gameObject.SetActive(false);
                var name = prefab.name;
                prefab = GameObject.Instantiate(prefab, Container.DefaultParent);
                prefab.name = name;
#endif
                prefab.SetActive(false);
            }

            var instance = parent == null ? GameObject.Instantiate(prefab) : GameObject.Instantiate(prefab, parent);
            instance.name = prefab.name;

            if (wasActive)
            {
#if UNITY_EDITOR //HACK -> if we're in the editor we've create an instance of the prefab before disabling it (see above comment), so we'll clean it up
                GameObject.Destroy(prefab);
                Container.DefaultParent.gameObject.SetActive(true);
#else
				prefab.SetActive(true);
#endif
            }

            return instance;
        }

		private void Inject(GameObject gameObject)
		{
			var components = gameObject.GetComponentsInChildren<ComponentBehaviour> (true);
			foreach (var component in components)
			{ component.Initialize (EventSystem); }
		}

        public Transform DefaultParent
        {
            get
            {
                return Container.DefaultParent;
            }
        }
    }
}

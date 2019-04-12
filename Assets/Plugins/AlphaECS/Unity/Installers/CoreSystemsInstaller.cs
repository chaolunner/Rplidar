using UniRx;
using Zenject;
using AlphaECS;
using System;
using System.Collections.Generic;
using UnityEngine;
using SubjectNerd.Utilities;
using AlphaECS.Unity;

namespace AlphaECS
{
    public class CoreSystemsInstaller : MonoInstaller
    {
        [SerializeField] [Reorderable] private List<GameObject> systemPrefabs = new List<GameObject>();

        private List<GameObject> kernelObjects = new List<GameObject>();

        public override void InstallBindings()
        {
            foreach (var resource in systemPrefabs)
            {
                var prefab = resource; // capture this locally so we can reassign later
                var name = prefab.name;

                var wasActive = prefab.activeSelf;
                if (wasActive)
                {
#if UNITY_EDITOR
                    Container.DefaultParent.gameObject.SetActive(false);
                    prefab = GameObject.Instantiate(prefab, Container.DefaultParent);
#endif

                    prefab.SetActive(false);
                }

                var go = (GameObject)Instantiate(prefab);
                go.name = name;

                DontDestroyOnLoad(go);
                kernelObjects.Add(go);
                var systems = go.GetComponentsInChildren<SystemBehaviour>(true);
                foreach (var system in systems)
                {
                    Container.Bind(system.GetType()).FromInstance(system).AsSingle();
                }

                if (wasActive)
                {
#if UNITY_EDITOR
                    GameObject.Destroy(prefab);
                    Container.DefaultParent.gameObject.SetActive(true);
#else
                    prefab.SetActive(true);
#endif
                }
            }

            /* zenject will throw a warning here
            * we can safely ignore this as we're using our Initialize() method on these kernel systems as a di constructor only
            * and the dependencies which we're injecting are in the framework scope and have already been bound to the container with AlphaECSInstaller
            */
            foreach (var go in kernelObjects)
            {
                Container.InjectGameObject(go);
                go.SetActive(true);
            }
        }
    }
}
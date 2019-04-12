using UnityEngine;
using Zenject;
using AlphaECS;
using System.Linq;
using System.Collections.Generic;
using AlphaECS.Unity;
using UnityEngine.SceneManagement;
using UniRx;

public class ProjectInstaller : MonoInstaller
{
    private List<GameObject> kernelObjects = new List<GameObject> ();

    public override void InstallBindings()
    {
        var resources = Resources.LoadAll("Kernel");
        foreach (var resource in resources)
        {
            var prefab = resource as GameObject;
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
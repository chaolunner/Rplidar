using UnityEngine;
using Zenject;
using AlphaECS;
using System.Linq;
using System.Collections.Generic;
using AlphaECS.Unity;
using System.Collections;
//using SubjectNerd.Utilities;

public class SceneInstaller : MonoInstaller
{
	//[Reorderable]
	public SystemBehaviour[] Systems;

	//[Reorderable]
	public EntityBehaviour[] Entities;

    public override void InstallBindings()
    {
        if (Systems == null || Systems.Length <= 0)
        { Systems = GetComponentsInChildren<SystemBehaviour>(true); }

        foreach (var system in Systems)
        {
            Container.Bind(system.GetType()).FromInstance(system).AsSingle();
        }
    }

    public override void Start ()
	{
		base.Start ();

		if (Entities == null || Entities.Length <= 0)
		{ Entities = GetComponentsInChildren<EntityBehaviour> (true); }

//		var gameObjects = entityBehaviours.Select (eb => eb.gameObject).ToArray();
//		gameObjects.ForceEnable ();
////		gameObjects.ForceEnable (); //HACK for some reason we have to call this twice - not sure if it's a framework bug or a project specific edge case...

		foreach (var eb in Entities)
		{
			if (eb == null)
			{
#if UNITY_EDITOR
				Debug.LogWarning(this.gameObject.scene.name + " IS MISSING AN ENTITY ON SETUP!");
#endif
				continue;
			}
			if (!eb.gameObject.activeInHierarchy)
			{
				//HACK for some reason we have to call this twice - not sure if it's a framework bug or a project specific edge case...
				//i recall seeing similar things in other frameworks in the past, i guess it has something to do with the scene not technically being fully bootstrapped...
				//...or some case where a child object may not be considered fully enabled until AFTER parent has been enabled (or vice versa)...
				//...for example, enabling parent -> child in the same step, or child -> parent
				eb.gameObject.ForceEnable ();
				eb.gameObject.ForceEnable ();
			}
		}
	}

	private void Reset()
	{
		#if UNITY_EDITOR
		Systems = null;
		Systems = GetComponentsInChildren<SystemBehaviour> (true);
		Entities = null;
		Entities = GetComponentsInChildren<EntityBehaviour> (true);

		UnityEditor.EditorUtility.SetDirty(this);
		#endif
	}
}
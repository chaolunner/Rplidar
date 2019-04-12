using System;
using UnityEngine;

[Serializable]
public class SceneReference
{
    [SerializeField][HideInInspector]
    public string SceneName;
    [SerializeField][HideInInspector]
    public string ScenePath;

    public static implicit operator string(SceneReference sceneReference)
    {
        return sceneReference.SceneName;
    }

    public static implicit operator SceneReference(string SceneName)
    {
        return new SceneReference { SceneName = SceneName };
    }
}
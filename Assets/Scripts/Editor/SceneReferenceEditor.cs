using UnityEngine;
using UnityEditor;
using System;

// TODO Harden this to be resilient against directory changes, file renaming, etc.
[CustomPropertyDrawer(typeof(SceneReference))]
public class SceneReferenceEditor : PropertyDrawer
{
    private const string SCENE_FILE_EXTENSION = ".unity";
    string SCENE_DIRECTORY = AppDataSystem.Load<ProjectPathConfiguration>("ProjectPathConfiguration").ScenesPath + "/";

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var sceneReference = AssetDatabase.LoadAssetAtPath<SceneAsset>(SCENE_DIRECTORY + property.FindPropertyRelative("SceneName").stringValue + SCENE_FILE_EXTENSION);
        var sceneAsset = EditorGUI.ObjectField(position, label, sceneReference, typeof(SceneAsset), false);

        if (sceneAsset == null)
        {
            property.FindPropertyRelative("SceneName").stringValue = "";
            property.FindPropertyRelative("ScenePath").stringValue = "";
        }
        else
        {
            property.FindPropertyRelative("SceneName").stringValue = sceneAsset.name;
            property.FindPropertyRelative("ScenePath").stringValue = SCENE_DIRECTORY + property.FindPropertyRelative("SceneName").stringValue + SCENE_FILE_EXTENSION;
        }
    }
}

namespace EditorUtilities
{
    public static class ExtensionMethods
    {
        public static BuildTarget ToBuildTarget(this BuildTargetType buildTargetType)
        {
            BuildTarget buildTarget;
            Enum.TryParse(buildTargetType.ToString(), false, out buildTarget);
            return buildTarget;
        }
    }

}

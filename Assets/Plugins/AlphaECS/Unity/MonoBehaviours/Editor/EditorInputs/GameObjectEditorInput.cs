using UnityEditor;
using UnityEngine;

namespace AlphaECS.Unity.Editor
{
    public class GameObjectEditorInput : SimpleEditorInput<GameObject>
    {
        protected override GameObject CreateTypeUI(string label, GameObject value)
        { return (GameObject)EditorGUILayout.ObjectField(label, value, typeof(GameObject), true); }
    }
}

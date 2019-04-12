using UnityEditor;
using UnityEngine;

namespace AlphaECS.Unity.Editor
{
    public class BoundsEditorInput : SimpleEditorInput<Bounds>
    {
        protected override Bounds CreateTypeUI(string label, Bounds value)
        { return EditorGUILayout.BoundsField(label, value); }
    }
}
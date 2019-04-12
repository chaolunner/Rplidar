using UnityEditor;
using UnityEngine;

namespace AlphaECS.Unity.Editor
{
    public class ColorEditorInput : SimpleEditorInput<Color>
    {
        protected override Color CreateTypeUI(string label, Color value)
        { return EditorGUILayout.ColorField(label, value); }
    }
}
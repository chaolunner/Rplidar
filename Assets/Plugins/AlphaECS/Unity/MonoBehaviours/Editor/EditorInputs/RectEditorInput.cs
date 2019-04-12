using UnityEditor;
using UnityEngine;

namespace AlphaECS.Unity.Editor
{
    public class RectEditorInput : SimpleEditorInput<Rect>
    {
        protected override Rect CreateTypeUI(string label, Rect value)
        { return EditorGUILayout.RectField(label, value); }
    }
}
using UnityEditor;

namespace AlphaECS.Unity.Editor
{
    public class FloatEditorInput : SimpleEditorInput<float>
    {
        protected override float CreateTypeUI(string label, float value)
        { return EditorGUILayout.FloatField(label, value); }
    }
}
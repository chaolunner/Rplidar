using UniRx;
using UnityEditor;

namespace AlphaECS.Unity.Editor
{
    public class ReactiveIntEditorInput : SimpleEditorInput<IntReactiveProperty>
    {
        protected override IntReactiveProperty CreateTypeUI(string label, IntReactiveProperty value)
        {
            value.Value = EditorGUILayout.IntField(label, value.Value);
            return null;
        }
    }
}
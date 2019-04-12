using UniRx;
using UnityEditor;

namespace AlphaECS.Unity.Editor
{
    public class ReactiveRectEditorInput : SimpleEditorInput<RectReactiveProperty>
    {
        protected override RectReactiveProperty CreateTypeUI(string label, RectReactiveProperty value)
        {
            value.Value = EditorGUILayout.RectField(label, value.Value);
            return null;
        }
    }
}
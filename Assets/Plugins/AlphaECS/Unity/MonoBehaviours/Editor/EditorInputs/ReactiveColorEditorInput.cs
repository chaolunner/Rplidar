using UniRx;
using UnityEditor;

namespace AlphaECS.Unity.Editor
{
    public class ReactiveColorEditorInput : SimpleEditorInput<ColorReactiveProperty>
    {
        protected override ColorReactiveProperty CreateTypeUI(string label, ColorReactiveProperty value)
        {
            value.Value = EditorGUILayout.ColorField(label, value.Value);
            return null;
        }
    }
}
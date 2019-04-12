using UniRx;
using UnityEditor;

namespace AlphaECS.Unity.Editor
{
    public class ReactiveBoundsEditorInput : SimpleEditorInput<BoundsReactiveProperty>
    {
        protected override BoundsReactiveProperty CreateTypeUI(string label, BoundsReactiveProperty value)
        {
            value.Value = EditorGUILayout.BoundsField(label, value.Value);
            return null;
        }
    }
}
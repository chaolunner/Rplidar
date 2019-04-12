using UniRx;

[UnityEditor.CustomPropertyDrawer(typeof(TransformReactiveProperty))]
[UnityEditor.CustomPropertyDrawer(typeof(GameObjectReactiveProperty))]
public partial class ReactivePropertiesDrawer : InspectorDisplayDrawer
{
}
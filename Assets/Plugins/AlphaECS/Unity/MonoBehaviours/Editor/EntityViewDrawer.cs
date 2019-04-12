// The property drawer class should be placed in an editor script, inside a folder called Editor.

// Tell the RangeDrawer that it is a drawer for properties with the RangeAttribute.
using UnityEngine;
using UnityEditor;
using AlphaECS;
using AlphaECS.Unity;

[CustomPropertyDrawer(typeof(EntityViewAttribute))]
public class EntityViewDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		label = EditorGUI.BeginProperty(position, label, property);
		position = EditorGUI.PrefixLabel(position, label);

//		var entity = property as Entity;
//		var view = entity.GetComponent<ViewComponent> ().Transforms [0];

		var components = property.FindPropertyRelative ("Components");
//		var arraySizeProperty = components.isArray

//		Debug.Log(property.name);
//		for (int i = 0; i < arraySizeProperty.intValue; i++)
//		{
//			Debug.Log (property.GetArrayElementAtIndex (i).name);
////			EditorGUILayout.PropertyField(property.GetArrayElementAtIndex(i));
//		}

//		if (view != null)
//		{
//			EditorGUI.ObjectField (position, view);
//		}

		EditorGUI.EndProperty();
	}
}
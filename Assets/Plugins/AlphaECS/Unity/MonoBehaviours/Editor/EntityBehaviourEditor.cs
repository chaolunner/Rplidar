using System;
using System.Collections.Generic;
using System.Linq;
using AlphaECS;
using SimpleJSON;
using AlphaECS.Unity;
using UniRx;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;


namespace AlphaECS
{
	[CustomEditor(typeof(EntityBehaviour), true)]
	public class EntityBehaviourEditor : Editor
	{
		private EntityBehaviour view;

		private ReorderableList reorderableComponents;

		private ReorderableList reorderableBlueprints;

		private bool showComponents = true;
		private bool showBlueprints = false;

		private readonly IEnumerable<Type> componentBaseTypes = AppDomain.CurrentDomain.GetAssemblies()
			.SelectMany(s => s.GetTypes())
			.Where(p => typeof(ComponentBase).IsAssignableFrom(p) && p.IsClass);

        //private readonly IEnumerable<Type> blueprintBaseTypes = AppDomain.CurrentDomain.GetAssemblies()
            //.SelectMany(s => s.GetTypes())
            //.Where(p => typeof(BlueprintBase).IsAssignableFrom(p) && p.IsClass);

		int headerProperties = 2;
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float lineSpacing = EditorGUIUtility.standardVerticalSpacing;
        float elementPadding = EditorGUIUtility.standardVerticalSpacing;

		public float[] componentHeights = new float[100];
		public float[] blueprintHeights = new float[100];

        private int componentToRemove = -1;

		private class ObjectInfo
		{
			public Type type;
		}

		void OnEnable()
		{
			if (view == null)
			{ view = (EntityBehaviour)target; }

//			reorderableComponents = new ReorderableList(serializedObject, serializedObject.FindProperty("ComponentTypes"), true, true, true, true);
			reorderableComponents = new ReorderableList(serializedObject, serializedObject.FindProperty("Components"), true, true, true, true);

			reorderableComponents.drawHeaderCallback = (Rect rect) =>
			{ EditorGUI.LabelField(rect, "Components", EditorStyles.boldLabel); };

			reorderableComponents.drawElementCallback = (rect, index, isActive, isFocused) =>
			{
//                var component = (object)Activator.CreateInstance(view.ComponentTypes[index].GetTypeWithAssembly());
//                JsonUtility.FromJsonOverwrite(view.ComponentData[index], component);
//                OnDrawElement(rect, component, index, componentHeights);
//                view.ComponentData[index] = JsonUtility.ToJson(component);
//				//OnDrawElement(reorderableComponents, view.Components[index].GetType().ToString(), rect, index, isActive, isFocused, componentHeights);

				var label = view.Components[index] != null ? view.Components[index].GetType().ToString() : "None";
				OnDrawElement(reorderableComponents, label, rect, index, isActive, isFocused, componentHeights, false);
			};

			reorderableComponents.elementHeightCallback = (index) =>
			{
				return componentHeights[index];
//              return OnElementHeight(reorderableComponents, index);
			};

			reorderableComponents.onAddDropdownCallback = (Rect rect, ReorderableList list) =>
			{
//				OnAddDropdown(rect, list, AddComponent, componentBaseTypes.ToArray());
				view.Components = view.GetComponents<Component> ().Where (c => c.GetType () != typeof(Transform) && c.GetType () != typeof(EntityBehaviour)).ToArray ();
				EditorUtility.SetDirty(target);
			};

			reorderableComponents.onRemoveCallback = (list) =>
			{
//				RemoveComponent(list);
				view.Components = null;
			};



			reorderableBlueprints = new ReorderableList(serializedObject, serializedObject.FindProperty("Blueprints"), true, true, true, true);

			reorderableBlueprints.drawHeaderCallback = (Rect rect) =>
			{ EditorGUI.LabelField(rect, "Blueprints", EditorStyles.boldLabel); };

			reorderableBlueprints.drawElementCallback = (rect, index, isActive, isFocused) =>
			{
				var label = view.Blueprints[index] != null ? view.Blueprints[index].GetType().ToString() : "None";
				OnDrawElement(reorderableBlueprints, label, rect, index, isActive, isFocused, blueprintHeights);
			};

			reorderableBlueprints.elementHeightCallback = (index) =>
			{
				return blueprintHeights[index];
//              return OnElementHeight(reorderableBlueprints, index);
			};

			reorderableBlueprints.onAddDropdownCallback = (Rect rect, ReorderableList list) =>
			{ OnAddDropdown(list, "Blueprints"); };

			reorderableBlueprints.onRemoveCallback = (list) =>
			{ RemoveBlueprint(list); };
		}

		public override void OnInspectorGUI()
		{
			if (view == null)
			{ view = (EntityBehaviour)target; }

			base.OnInspectorGUI();

			if (view == null) { return; }

			DrawHeaderSection();

            componentToRemove = -1;

//			if (Application.isPlaying)
//			{
//				if (showComponents)
//				{
//					if (this.WithIconButton("▾"))
//					{ showComponents = false; }
//				}
//				else
//				{
//					if (this.WithIconButton("▸"))
//					{ showComponents = true; }
//				}
//
//				if (showComponents)
//				{
//					for (var i = 0; i < view.Entity.Components.Count(); i++)
//					{
//						var rect = EditorGUILayout.BeginVertical();
//						OnDrawElement(rect, view.Entity.Components.ElementAt(i), i, componentHeights);
//						EditorGUILayout.EndVertical();
//
//                        GUILayoutUtility.GetRect(0f, componentHeights[i] + lineHeight);
//					}
//				}
//
//                if(componentToRemove > -1)
//                {
//                    var component = view.Entity.Components.ElementAt(componentToRemove);
//                    view.Entity.RemoveComponent(component);
//
//                    if (component.GetType().IsSubclassOf(typeof(Component)))
//                    {
//                        Destroy((UnityEngine.Object)component);
//                    }
//				}
//				return;
//			}

			if (showComponents)
			{
				if (this.WithIconButton("▾"))
				{ showComponents = false; }
			}
			else
			{
				if (this.WithIconButton("▸"))
				{
//					view.Components = view.GetComponents<Component> ().Where (c => c.GetType () != typeof(Transform) && c.GetType () != typeof(EntityBehaviour)).ToArray ();
//					EditorUtility.SetDirty(target);
					showComponents = true;
				}
			}

			if (showComponents)
			{
//				if (GUILayout.Button("Preload Components"))
//				{
//					view.Components = view.GetComponents<Component> ().Where (c => c.GetType () != typeof(Transform) && c.GetType () != typeof(EntityBehaviour)).ToArray ();
//					EditorUtility.SetDirty(target);
//				}

//				serializedObject.Update();
//				Undo.RecordObject(view, "Added Data");
				reorderableComponents.DoLayoutList();
//				serializedObject.ApplyModifiedProperties();
			}

			if (componentToRemove > -1)
			{
//                view.ComponentTypes.RemoveAt(componentToRemove);
//                view.ComponentData.RemoveAt(componentToRemove);
//				view.Components.RemoveAt(componentToRemove);

				//if (component.GetType().IsSubclassOf(typeof(Component)))
				//{
				//	Destroy((UnityEngine.Object)component);
				//}
			}

			if (showBlueprints)
			{
				if (this.WithIconButton("▾"))
				{ showBlueprints = false; }
			}
			else
			{
				if (this.WithIconButton("▸"))
				{ showBlueprints = true; }
			}

			if (showBlueprints)
			{
				serializedObject.Update();
				Undo.RecordObject(view, "Added Data");
				reorderableBlueprints.DoLayoutList();
				serializedObject.ApplyModifiedProperties();
			}

			if (showComponents || showBlueprints)
			{
				PersistChanges();
			}
		}

        /// <summary>
        /// Draws an element of type serialized property in a reorderable list
        /// </summary>
		private void OnDrawElement(ReorderableList list, string labelName, Rect rect, int index, bool isActive, bool isFocused, float[] heightsArray, bool showChildren = true)
		{
			var element = list.serializedProperty.GetArrayElementAtIndex(index);

			EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), labelName, EditorStyles.boldLabel);
			EditorGUI.ObjectField(new Rect(rect.x, rect.y + lineHeight, rect.width, lineHeight), element);

			if (element.objectReferenceValue == null || showChildren == false)
			{
                heightsArray[index] = (headerProperties * lineHeight) + (headerProperties * lineSpacing);
				return;
			}


			var so = new SerializedObject(element.objectReferenceValue);
			so.Update();

			var iterator = so.GetIterator();
			iterator.NextVisible(true); // skip the script reference

			var i = headerProperties;
//			var showChildren = true;
			while (iterator.NextVisible(showChildren))
			{
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + (i * lineHeight) + (i * lineSpacing), rect.width, lineHeight), iterator);
				i++;
				if (iterator.isArray)
				{
					showChildren = iterator.isExpanded;
				}
			}

			so.ApplyModifiedProperties();
            heightsArray[index] = (i * lineHeight) + (i * lineSpacing);
		}

		/// <summary>
		/// Draws an element of type object in a list
		/// </summary>
		private void OnDrawElement(Rect rect, object component, int index, float[] heightsArray)
		{
			serializedObject.Update();

			var componentType = component.GetType();
			var typeName = componentType == null ? "" : componentType.Name;
			var typeNamespace = componentType == null ? "" : componentType.Namespace;
			headerProperties = 0;

			if (string.IsNullOrEmpty(typeName))
			{
				typeName = "Unknown Type";
			}
			EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, lineHeight), typeName, EditorStyles.boldLabel);
			headerProperties += 1;

			//EditorGUILayout.BeginHorizontal();
			if (GUI.Button(new Rect(rect.width, rect.y, lineHeight, lineHeight), "-"))
			{
				componentToRemove = index;
			}
			//EditorGUILayout.EndHorizontal();

			if (!string.IsNullOrEmpty(typeNamespace))
			{
				EditorGUI.LabelField(new Rect(rect.x, rect.y + lineHeight, rect.width, lineHeight), typeNamespace);
				headerProperties += 1;
			}

			//EditorGUILayout.Space();

			if (componentType.IsSubclassOf(typeof(UnityEngine.Component)))
			{
				heightsArray[index] = (headerProperties * lineHeight) + (headerProperties * lineSpacing);
                serializedObject.ApplyModifiedProperties();
				return;
			}

			var i = headerProperties;

			//draw component fields
			foreach (var field in component.GetType().GetFields())
			{
                var type = field.FieldType;
                var value = field.GetValue(component);
				var isTypeSupported = this.TryDrawValue(rect, type, ref value, field.Name, i);

				if (isTypeSupported == true)
				{
					field.SetValue(component, value);
					i++;
				}
			}

			//draw component properties
			foreach (var property in component.GetType().GetProperties())
			{
                var type = property.PropertyType;
                var value = property.GetValue(component, null);
				var isTypeSupported = this.TryDrawValue(rect, type, ref value, property.Name, i);

				if (isTypeSupported == true)
				{
					property.SetValue(component, value, null);
					i++;
				}
			}

			serializedObject.ApplyModifiedProperties();
			heightsArray[index] = (i * lineHeight) + (i * lineSpacing);
		}

		private void OnAddDropdown(Rect rect, ReorderableList list, Action<object> action, Type[] types)
		{
			var dropdownMenu = new GenericMenu();

			for (var i = 0; i < types.Length; i++)
			{
				dropdownMenu.AddItem(new GUIContent(types[i].ToString()), false, action.Invoke, new ObjectInfo() { type = types.ElementAt(i) });
			}

			dropdownMenu.ShowAsContext();
		}

		private void OnAddDropdown(ReorderableList list, string elementName)
		{
			var element = list.serializedProperty.serializedObject.FindProperty(elementName);
			element.arraySize += 1;
		}

		private void DrawHeaderSection()
		{
			this.UseVerticalBoxLayout(() =>
			{
				if (Application.isPlaying)
				{
					this.WithLabelField("Id: ", view.Entity.Id.ToString());
					this.WithLabelField("Pool: ", view.Pool.Name);
				}
				else
				{
					view.Id = this.WithTextField("Id:", view.Id);
					view.PoolName = this.WithTextField("Pool: ", view.PoolName);
				}

				EditorGUILayout.BeginHorizontal();
				view.RemoveEntityOnDestroy = EditorGUILayout.Toggle(view.RemoveEntityOnDestroy);
				EditorGUILayout.LabelField("Remove Entity On Destroy");
				EditorGUILayout.EndHorizontal();

				if (Application.isPlaying)
				{
					if (GUILayout.Button("Destroy Entity"))
					{
						view.Pool.RemoveEntity(view.Entity);
						Destroy(view.gameObject);
					}
				}
			});
		}

		private void AddComponent(object info)
		{
//			var componentInfo = (ObjectInfo)info;
//            var component = (ComponentBase)Activator.CreateInstance(componentInfo.type);
//            var type = component.GetType().ToString();
//            var json = JsonUtility.ToJson(component);
//
//            view.ComponentTypes.Add(type);
//            view.ComponentData.Add(json);
		}

		private void RemoveComponent(ReorderableList list)
		{
//            view.ComponentTypes.RemoveAt(list.index);
//            view.ComponentData.RemoveAt(list.index);
		}

		private void AddBlueprint(object info)
		{
			var blueprintInfo = (ObjectInfo)info;
			var blueprint = (BlueprintBase)ScriptableObject.CreateInstance(blueprintInfo.type);
			blueprint.name = blueprintInfo.type.Name;
			view.Blueprints.Add(blueprint);
		}

		private void RemoveBlueprint(ReorderableList list)
		{
			view.Blueprints.RemoveAt(list.index);
		}

		private void PersistChanges()
		{
			if (GUI.changed && !Application.isPlaying)
			{
				this.SaveActiveSceneChanges();
            	AssetDatabase.SaveAssets();
			}
		}
	}
}
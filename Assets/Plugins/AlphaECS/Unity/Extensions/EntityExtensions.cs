using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using AlphaECS.Unity;
using System.Linq;
using System;
using UniRx;

namespace AlphaECS
{
    public static class EntityExtensions
    {
        private static FastString FastJson
        {
            get
            {
                if (fastJson == null)
                {
                    fastJson = new FastString(256);
                }
                return fastJson;
            }
        }
        private static FastString fastJson;

        private static string json;
        private static Type type;
        private static object component;
        private static bool shouldIgnore;
        private static string componentAsJson;

        private static JSONNode node;
        private static string ComponentsKey = "Components";
        private static string TypesKey = "Types";

        private static string idHeader = "{ \"Id\": ";
        private static string typesHeader = "\"Types\": [";
        private static string componentsHeader = "\"Components\": [";
        private static string componentsFooter = "]}";

        private static string typesString;
        private static string componentsString;

        private static int componentIndex = 0;

        private static string[] typesStrings;
        private static string[] componentsStrings;

        private static string lineSeparator = ", \n";
        //private static string lineBreak = "\n";
        private static string[] lineSeparatorChar = new[] { lineSeparator };
        //private static string[] lineBreakChar = new[] { lineBreak };

        private static FastString Types
        {
            get
            {
                if (types == null)
                {
                    types = new FastString(256);
                }
                return types;
            }
        }
        private static FastString types;

        public static string Serialize(this IEntity entity, Type[] includedTypes = null, Type[] ignoredTypes = null)
        {
            FastJson.Clear();
            Types.Clear();

            FastJson.Append(idHeader).Append("\"").Append(entity.Id).Append("\"").Append(lineSeparator);

            Types.Append(typesHeader);

            Types.Append(string.Join(",", entity.Components.Where(c =>
            {
                if (includedTypes != null && includedTypes.Contains(c.GetType()))
                {
                    return true;
                }
                else
                {
                    if (!c.GetType().ShouldIgnore(ignoredTypes))
                    { return true; }
                }
                return false;
            }).Select(c => "\"" + c.GetType().ToString() + "\"").ToArray()) + "]");
            FastJson.Append(types).Append(lineSeparator);

            FastJson.Append(componentsHeader);
            for (componentIndex = 0; componentIndex < entity.Components.Count(); componentIndex++)
            {
                component = entity.Components.ElementAt(componentIndex);
                if (component.GetType().ShouldIgnore(ignoredTypes))
                {
                    continue;
                }

                componentAsJson = JsonUtility.ToJson(component);
                FastJson.Append(componentAsJson);
                if(componentIndex < entity.Components.Count() - 1)
                {
                    FastJson.Append(lineSeparator);
                }
            }
            FastJson.Append("]}");

            json = FastJson.ToString();
            return json;
        }

        public static IEntity Deserialize(this IEntity entity, string json, Type[] includedTypes = null, Type[] ignoredTypes = null, bool slowAndSafe = true)
        {
            if (slowAndSafe)
            {
                node = JSON.Parse(json);
                return entity.Deserialize(node, includedTypes, ignoredTypes);
            }
            else
            {
                var typesIndex = json.IndexOf(typesHeader);
                var componentsIndex = json.IndexOf(componentsHeader);

                var typesLength = componentsIndex - (typesIndex + typesHeader.Length) - 5; // -5 trims the trailing ], and newline characters

                typesString = json.Substring(typesIndex + typesHeader.Length, typesLength);
                componentsString = json.Substring(componentsIndex + componentsHeader.Length - 1);

                typesStrings = typesString.Split(',');
                //TODO componenetsStrings still includes garbage }] characters at the end here
                //should remove but can leave for now
                componentsStrings = componentsString.Split(lineSeparatorChar, StringSplitOptions.None);

                for (var i = 1; i < typesStrings.Length; i++)
                {
                    typesStrings[i] = typesStrings[i].Replace("\"", "").Replace(" ", "").Replace(", ", "");
                    componentsStrings[i] = componentsStrings[i].Replace(", ", "").Replace("\\n", "").Replace(" }]", "");

                    if(i == typesStrings.Length - 1)
                    {
                        componentsStrings[i] = componentsStrings[i].Substring(0, componentsStrings[i].Length - 3);
                    }

                    component = entity.Components.Where(c => c.GetType().ToString().Equals(typesStrings[i])).FirstOrDefault();
                    JsonUtility.FromJsonOverwrite(componentsStrings[i], component);
                }
                return entity;
            }
        }

        //slow and safe
        public static IEntity Deserialize(this IEntity entity, JSONNode node, Type[] includedTypes = null, Type[] ignoredTypes = null)
        {
            if(node == null || node[TypesKey] == null || node[TypesKey].Count <= 0) { return entity; }

            for (var i = 0; i < node[TypesKey].Count; i++)
            {
                type = node[TypesKey][i].ToString().Replace("\"", "").GetTypeWithAssembly();
                if (type == null)
                {
#if UNITY_EDITOR
                    Debug.LogWarning("Unable to resolve type " + node[TypesKey][i].ToString().Replace("\"", "") + "!");
#endif
                    continue;
                }

                if (includedTypes != null && !includedTypes.Contains(type))
                { continue; }

                if (ignoredTypes != null && type.ShouldIgnore(ignoredTypes))
                { continue; }

//              if (!entity.HasComponent (type))
//              {
//                  Debug.LogWarning ("Type " + node ["Types"] [i].ToString ().Replace ("\"", "") + " not found on entity!");
//                  continue;
//              }

                component = null;

                if (typeof(Component).IsAssignableFrom(type)) //~0-5
                {
                    if (!entity.HasComponent<ViewComponent>())
                    {
                        entity.AddComponent(new ViewComponent());
#if UNITY_EDITOR
                        Debug.LogWarning("No view found for component " + type.ToString() + "!");
#endif
                    }

                    var viewComponent = entity.GetComponent<ViewComponent>();
                    var index = i;

                    foreach (var t in viewComponent.Transforms)
                    {
                        if (t.gameObject.GetComponent(type) != null)
                        {
                            component = t.gameObject.GetComponent(type);
                            JsonUtility.FromJsonOverwrite(node[ComponentsKey][index].ToString(), component);
                        }
                    }
                }
                else
                {
                    if (!entity.HasComponent(type))
                    {
                        component = (object)Activator.CreateInstance(type);
                    }
                    else
                    {
                        component = entity.GetComponent(type);
                    }
                    JsonUtility.FromJsonOverwrite(node[ComponentsKey][i].ToString(), component);
                }
            }
            return entity;
        }

        public static bool ShouldIgnore(this Type type, Type[] ignoredTypes)
        {
            shouldIgnore = false;

            //use MonoBehaviour as JsonUtility.ToJson does not support engine types
            if ((!typeof(MonoBehaviour).IsAssignableFrom(type) && !typeof(IComponent).IsAssignableFrom(type)) ||
                type.IsDefined(typeof(NonSerializableDataAttribute), false))
            {
                shouldIgnore = true;
            }

            if(ignoredTypes != null)
            {
                foreach (var t in ignoredTypes)
                {
                    if (t.IsAssignableFrom(type))
                    {
                        shouldIgnore = true;
                        break;
                    }
                }  
            }

            return shouldIgnore;
        }
    }
}
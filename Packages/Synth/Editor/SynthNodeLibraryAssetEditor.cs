using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

namespace Fab.Synth.Editor
{
	[CustomEditor(typeof(SynthNodeLibraryAsset))]
	public class SynthNodeLibraryAssetEditor : UnityEditor.Editor
	{
		private static readonly string ObjectSelectorClosedCmdName = "ObjectSelectorClosed";
		private int currentPickerWindow = -2;

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			serializedObject.Update();

			if (GUILayout.Button("Add Node"))
			{
				currentPickerWindow = GUIUtility.GetControlID(FocusType.Passive);
				EditorGUIUtility.ShowObjectPicker<Shader>(null, false, null, currentPickerWindow);
			}

			if (Event.current != null && Event.current.commandName == ObjectSelectorClosedCmdName && EditorGUIUtility.GetObjectPickerControlID() == currentPickerWindow)
			{
				currentPickerWindow = -2;
				if (EditorGUIUtility.GetObjectPickerObject() is Shader shader)
				{
					AddNode(shader);

				}
			}

			serializedObject.ApplyModifiedProperties();
		}

		private void AddNode(Shader shader)
		{
			string[] tokens = shader.name.Split('/');
			if (tokens.Length < 3)
			{
				Debug.LogError($"Cannot create node from shader \"{shader.name}\"");
				return;
			}


			switch (tokens[tokens.Length - 2])
			{
				case "Generate":
					AddNodeItem("generatorNodes", tokens[tokens.Length - 1], shader);
					break;
				case "Mutate":
					AddNodeItem("modulateNodes", tokens[tokens.Length - 1], shader);
					break;
				case "Blend":
					AddNodeItem("blendNodes", tokens[tokens.Length - 1], shader);
					break;
				default:
					Debug.LogError($"Cannot create node from shader \"{shader.name}\". " +
						"Shader name must be of the type .../<NodeType>/<NodeName> where the node type is either Generate, Modulate or Blend");
					return;
			}
		}

		private void AddNodeItem(string collectionPropName, string name, Shader shader)
		{
			Undo.RecordObject(target, "Add Node");
			var nodesProp = serializedObject.FindProperty("library." + collectionPropName);
			nodesProp.InsertArrayElementAtIndex(nodesProp.arraySize);
			var nodeProp = nodesProp.GetArrayElementAtIndex(nodesProp.arraySize - 1);
			nodeProp.FindPropertyRelative("name").stringValue = name;
			nodeProp.FindPropertyRelative("shader").objectReferenceValue = shader;

			//populate properties
			var propertiesProp = nodeProp.FindPropertyRelative("properties");
			propertiesProp.ClearArray();
			int propCount = ShaderUtil.GetPropertyCount(shader);

			List<string> keywords = new List<string>();
			int addedProps = 0;
			for (int i = 0; i < propCount; i++)
			{
				keywords.Clear();
				string propName = ShaderUtil.GetPropertyName(shader, i);
				int typeValue = -1;
				switch (ShaderUtil.GetPropertyType(shader, i))
				{
					case ShaderUtil.ShaderPropertyType.Color:
						typeValue = (int)SynthNodeDescriptor.PropertyDescriptor.PropertyType.Color;
						break;
					case ShaderUtil.ShaderPropertyType.Vector:
						typeValue = (int)SynthNodeDescriptor.PropertyDescriptor.PropertyType.Vector;
						break;
					case ShaderUtil.ShaderPropertyType.Float:
					case ShaderUtil.ShaderPropertyType.Range:
						foreach (LocalKeyword keyword in shader.keywordSpace.keywords)
						{
							if (keyword.type == ShaderKeywordType.UserDefined && keyword.name.StartsWith(propName))
								keywords.Add(keyword.name);
						}

						if (keywords.Count > 0)
						{
							typeValue = (int)SynthNodeDescriptor.PropertyDescriptor.PropertyType.Enum;
						}
						else
							typeValue = (int)SynthNodeDescriptor.PropertyDescriptor.PropertyType.Float;
						break;
					default:
						break;
				}

				if (typeValue != -1)
				{
					propertiesProp.InsertArrayElementAtIndex(addedProps);
					var propProp = propertiesProp.GetArrayElementAtIndex(addedProps);
					propProp.FindPropertyRelative("name").stringValue = ShaderUtil.GetPropertyDescription(shader, i);
					propProp.FindPropertyRelative("propName").stringValue = propName;

					var typeProp = propProp.FindPropertyRelative("type");
					typeProp.enumValueIndex = typeValue;

					if(typeValue == (int)SynthNodeDescriptor.PropertyDescriptor.PropertyType.Enum)
					{
						var keywordsProp = propProp.FindPropertyRelative("keywords");
						keywordsProp.ClearArray();
						for (int j = 0; j < keywords.Count; j++)
						{
							keywordsProp.InsertArrayElementAtIndex(j);
							keywordsProp.GetArrayElementAtIndex(j).stringValue = keywords[j];
						}
					}

					addedProps++;
				}
			}
		}
	}
}

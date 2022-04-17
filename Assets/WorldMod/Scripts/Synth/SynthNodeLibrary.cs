using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Fab.WorldMod.Synth
{
	[Serializable]
	public class SynthNodeDescriptor
	{
		[SerializeField]
		private string name;
		[SerializeField]
		private Shader shader;

		[SerializeField]
		private PropertyDescriptor[] properties;

		public string Name => name;
		public Shader Shader => shader;

		public PropertyDescriptor GetProperty(string name)
		{
			for (int i = 0; i < properties.Length; i++)
			{
				if(properties[i].Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
					return properties[i];
			}

			throw new ArgumentException($"No property with the given name \"{name}\" was found");
		}

		//public LocalKeyword GetKeyword(string propertyName, string keyword)
		//{
		//	PropertyDescriptor descriptor = null;
		//	foreach (var prop in properties)
		//	{
		//		if (prop.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase))
		//		{
		//			descriptor = prop;
		//			break;
		//		}
		//	}

		//	if (descriptor == null)
		//		throw new ArgumentException($"No property with the given name \"{name}\" was found");

		//	int index = Array.IndexOf(descriptor.Keywords, keyword);
		//	if (index == -1)
		//		throw new ArgumentException($"No keyword with the given name \"{keyword}\" was found");

		//	return new LocalKeyword(shader, propertyName + '_' + keyword);
		//}


		[Serializable]
		public class PropertyDescriptor
		{
			public enum PropertyType
			{
				Float,
				Color,
				Vector,
				Enum
			}

			[SerializeField]
			private string name;
			[SerializeField]
			[HideInInspector]
			private int id;
			[SerializeField]
			private PropertyType type;
			[SerializeField]
			private string[] keywords;

			public IEnumerable<string> Keywords => keywords;
			public string Name => name;
			public int Id => id;
			public PropertyType Type => type;
		}
	}


	[Serializable]
	public class SynthNodeLibrary
	{

		[SerializeField]
		private List<SynthNodeDescriptor> generatorNodes;
		[SerializeField]
		private List<SynthNodeDescriptor> mutationNodes;
		[SerializeField]
		private List<SynthNodeDescriptor> blendNodes;

		public IEnumerable<SynthNodeDescriptor> GeneratorNodes { get => generatorNodes; }
		public IEnumerable<SynthNodeDescriptor> MutationNodes { get => mutationNodes; }
		public IEnumerable<SynthNodeDescriptor> BlendNodes { get => blendNodes; }
	}
}

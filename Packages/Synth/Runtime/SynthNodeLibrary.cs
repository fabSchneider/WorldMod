using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fab.Synth
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

		[Serializable]
		public class PropertyDescriptor
		{
			public enum PropertyType
			{
				Float,
				Color,
				Vector,
				Enum,
				Texture
			}

			[SerializeField]
			private string name;
			[SerializeField]
			private string propName;

			[SerializeField]
			private PropertyType type;
			[SerializeField]
			private string[] keywords;

			public IEnumerable<string> Keywords => keywords;
			public string Name => name;
			public string PropName => propName;
			public PropertyType Type => type;
		}
	}


	[Serializable]
	public class SynthNodeLibrary
	{

		[SerializeField]
		private List<SynthNodeDescriptor> generatorNodes;
		[SerializeField]
		[UnityEngine.Serialization.FormerlySerializedAs("mutationNodes")]
		private List<SynthNodeDescriptor> modulateNodes;
		[SerializeField]
		private List<SynthNodeDescriptor> blendNodes;

		public IEnumerable<SynthNodeDescriptor> GeneratorNodes { get => generatorNodes; }
		public IEnumerable<SynthNodeDescriptor> MutationNodes { get => modulateNodes; }
		public IEnumerable<SynthNodeDescriptor> BlendNodes { get => blendNodes; }
	}
}

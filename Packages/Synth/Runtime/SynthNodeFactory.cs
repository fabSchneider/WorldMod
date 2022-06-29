using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fab.Synth
{
	public class SynthNodeFactory
	{
		[SerializeField]
		private Dictionary<string, SynthNodeDescriptor> generatorNodesByName;
		[SerializeField]
		private Dictionary<string, SynthNodeDescriptor> mutationNodesByName;
		[SerializeField]
		private Dictionary<string, SynthNodeDescriptor> blendNodesByName;

		public SynthNodeFactory(SynthNodeLibrary library)
		{
			generatorNodesByName = new Dictionary<string,SynthNodeDescriptor>(StringComparer.InvariantCultureIgnoreCase);
			foreach (var node in library.GeneratorNodes)
				generatorNodesByName.Add(node.Name, node);

			mutationNodesByName = new Dictionary<string, SynthNodeDescriptor>(StringComparer.InvariantCultureIgnoreCase);
			foreach (var node in library.MutationNodes)
				mutationNodesByName.Add(node.Name, node);

			blendNodesByName = new Dictionary<string, SynthNodeDescriptor>(StringComparer.InvariantCultureIgnoreCase);
			foreach (var node in library.BlendNodes)
				blendNodesByName.Add(node.Name, node);
		}

		public SynthNodeDescriptor GetNodeDescriptor(Type nodeType, string name)
		{
			if (nodeType == typeof(GenNode))
			{
				if (generatorNodesByName.TryGetValue(name, out SynthNodeDescriptor descriptor))
					return descriptor;
			}
			else if (nodeType == typeof(ModulateNode))
			{
				if (mutationNodesByName.TryGetValue(name, out SynthNodeDescriptor descriptor))
					return descriptor;
			}
			else if (nodeType == typeof(BlendNode))
			{
				if (blendNodesByName.TryGetValue(name, out SynthNodeDescriptor descriptor))
					return descriptor;
			}

			throw new ArgumentException($"No descriptor for the {nodeType.Name} with the name \"{name}\" found");
		}

		public SynthNode CreateNode(Type nodeType, string name)
		{
			SynthNode node = null;
			if (nodeType == typeof(GenNode))
			{
				if(generatorNodesByName.TryGetValue(name, out SynthNodeDescriptor descriptor))
					node = new GenNode(descriptor.Shader);
			}
			else if (nodeType == typeof(ModulateNode))
			{
				if (mutationNodesByName.TryGetValue(name, out SynthNodeDescriptor descriptor))
					node = new ModulateNode(descriptor.Shader);
			}
			else if (nodeType == typeof(BlendNode))
			{
				if (blendNodesByName.TryGetValue(name, out SynthNodeDescriptor descriptor))
					node = new BlendNode(descriptor.Shader);
			}

			if (node == null)
				throw new ArgumentException($"{nodeType.Name} with the name \"{name}\" does not exist");

			return node;
		}
	}
}

using System;
using Fab.Geo.Lua.Interop;
using Fab.Lua.Core;
using Fab.WorldMod.Synth;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.Rendering;

namespace WorldMod.Lua
{
	public class LuaNodeProperty
	{
		private DynValue value;

		private float floatValue;
		private Color colorValue;
		private Vector3 vectorValue;
		private string enumValue;

		public float FloatValue  => floatValue; 
		public Color ColorValue  => colorValue; 
		public Vector3 VectorValue => vectorValue; 
		public string EnumValue => enumValue; 

		private string name;
		public string Name => name;

		public LuaNodeProperty(string name, DynValue value)
		{
			this.name = name;
			this.value = value;

		}

		public void SetType(SynthNodeDescriptor.PropertyDescriptor.PropertyType type)
		{
			switch (type)
			{
				case SynthNodeDescriptor.PropertyDescriptor.PropertyType.Float:
					floatValue = (float)value.CastToNumber();
					return;
				case SynthNodeDescriptor.PropertyDescriptor.PropertyType.Color:
					colorValue = value.ToObject<Color>();
					return;
				case SynthNodeDescriptor.PropertyDescriptor.PropertyType.Vector:
					vectorValue = value.ToObject<Vector3>();
					return;
				case SynthNodeDescriptor.PropertyDescriptor.PropertyType.Enum:
					enumValue = value.String;
					return;
			}
		}
	}

	//[LuaName("node_prop")]
	//public class NodePropertyModule : LuaObject, ILuaObjectInitialize
	//{
	//	NodePropertyProxy @float(string name, float value)
	//	{
	//		var proxy = new NodePropertyProxy();
	//		proxy.SetTarget(new LuaNodeProperty(name, value));
	//		return proxy;
	//	}

	//	NodePropertyProxy color(string name, Color value)
	//	{
	//		var proxy = new NodePropertyProxy();
	//		proxy.SetTarget(new LuaNodeProperty(name, value));
	//		return proxy;
	//	}

	//	NodePropertyProxy vector(string name, Vector3 value)
	//	{
	//		var proxy = new NodePropertyProxy();
	//		proxy.SetTarget(new LuaNodeProperty(name, value));
	//		return proxy;
	//	}

	//	NodePropertyProxy @enum(string name, int value)
	//	{
	//		var proxy = new NodePropertyProxy();
	//		proxy.SetTarget(new LuaNodeProperty(name, value));
	//		return proxy;
	//	}

	//	public void Initialize()
	//	{
			
	//	}
	//}


	public class NodePropertyProxy : LuaProxy<LuaNodeProperty>
	{

	}


	public class LayerControl
	{
		public string name;


		public event Action<float> onValueChange;

		public void SetValue(float value)
		{
			onValueChange?.Invoke(value);
		}
	}

	[LuaName("slider")]
	[LuaHelpInfo("A slider control")]
	public class LayerControlProxy : LuaProxy<LayerControl>
	{
		private Closure onValueChange;


		[LuaHelpInfo("Add a function to be executed when the value of this slider changes")]
		public void on_change(Closure callback)
		{
			ThrowIfNil();

			onValueChange = callback;

			Target.onValueChange -= OnValueChange;
			if (onValueChange != null)
				Target.onValueChange += OnValueChange;
		}

		public void set(float value)
		{
			Target.onValueChange -= OnValueChange;
			target.SetValue(value);
			Target.onValueChange += OnValueChange;
		}

		private void OnValueChange(float val)
		{
			onValueChange?.Call(val);
		}

	}


	[LuaHelpInfo("A synth layer")]
	[LuaName("synth_layer")]
	public class SynthLayerProxy : LuaProxy<SynthLayer>
	{
		SynthNodeFactory nodeFactory;

		public SynthLayerProxy(SynthNodeFactory nodeFactory)
		{
			this.nodeFactory = nodeFactory;
		}


		[LuaHelpInfo("Sets a texture")]
		public SynthLayerProxy texture(ImageProxy texture)
		{
			if (texture == null || texture.Target == null)
				throw new ArgumentException("Texture is nil");

			target.InputTexture = texture.Target;
			return this;
		}

		[LuaHelpInfo("Interpolates dark to light of the input from color a to color b")]
		public SynthLayerProxy lerp(Color a, Color b)
		{
			MutateNode node = (MutateNode)nodeFactory.CreateNode(typeof(MutateNode), "lerp");

			node.Material.SetColor("_ColorA", a);
			node.Material.SetColor("_ColorB", b);
			target.AddMutateNode(node);
			return this;
		}

		[LuaHelpInfo("Masks one channel of the input")]
		public SynthLayerProxy mask(string channel)
		{
			MutateNode node = (MutateNode)nodeFactory.CreateNode(typeof(MutateNode), "mask");

			//var descriptor = nodeFactory.GetNodeDescriptor(typeof(MutateNode), "mask");
		
			foreach(LocalKeyword enabled in node.Material.enabledKeywords)
			{
				if (enabled.name.StartsWith("_MASK"))
					node.Material.DisableKeyword(in enabled);
			}
		
			//LocalKeyword keyword = descriptor.GetKeyword("_MASK", channel);
			//node.Material.EnableKeyword("_MASK_" + channel);
			SetKeywordOnMaterial(node.Material, "_MASK", channel);
			target.AddMutateNode(node);
			return this;
		}

		private void SetKeywordOnMaterial(Material material, string propName, string keyword)
		{
			foreach (LocalKeyword enabled in material.enabledKeywords)
			{
				if (enabled.name.StartsWith(propName))
					material.DisableKeyword(in enabled);
			}

			material.EnableKeyword(propName + "_" + keyword);
		}

		public SynthLayerProxy generate(string name, params LuaNodeProperty[] props)
		{
			GenNode node = (GenNode)CreateNode(typeof(GenNode), name, props);
			target.GenerateNode = node;
			return this;
		}

		public SynthLayerProxy mutate(string name, params LuaNodeProperty[] props)
		{
			MutateNode node = (MutateNode)CreateNode(typeof(MutateNode), name, props);
			target.AddMutateNode(node);
			return this;
		}

		public SynthLayerProxy blend(string name, params LuaNodeProperty[] props)
		{
			BlendNode node = (BlendNode)CreateNode(typeof(BlendNode), name, props);
			target.BlendNode = node;
			return this;
		}

		private SynthNode CreateNode(Type nodeType, string name, LuaNodeProperty[] props)
		{
			SynthNode node = nodeFactory.CreateNode(nodeType, name);
			SynthNodeDescriptor descriptor = nodeFactory.GetNodeDescriptor(nodeType, name);

			foreach (var prop in props)
			{
				var propDescriptor = descriptor.GetProperty(prop.Name);

				prop.SetType(propDescriptor.Type);
				switch (propDescriptor.Type)
				{
					case SynthNodeDescriptor.PropertyDescriptor.PropertyType.Float:
						node.Material.SetFloat(propDescriptor.Id, prop.FloatValue);
						break;
					case SynthNodeDescriptor.PropertyDescriptor.PropertyType.Color:
						node.Material.SetColor(propDescriptor.Id, prop.ColorValue);
						break;
					case SynthNodeDescriptor.PropertyDescriptor.PropertyType.Vector:
						node.Material.SetVector(propDescriptor.Id, prop.VectorValue);
						break;
					case SynthNodeDescriptor.PropertyDescriptor.PropertyType.Enum:
						break;
					default:
						break;
				}
			}

			return node;

		}

		[LuaHelpInfo("Sets the layer blend mode")]
		public SynthLayerProxy blend(string name)
		{
			target.BlendNode = (BlendNode)nodeFactory.CreateNode(typeof(BlendNode), name);
			return this;
		}
	}
}

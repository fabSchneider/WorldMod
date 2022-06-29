using System;
using Fab.WorldMod.Lua.Interop;
using Fab.Lua.Core;
using Fab.Synth;
using MoonSharp.Interpreter;
using UnityEngine;

namespace Fab.WorldMod.Lua
{
	[LuaHelpInfo("A synth layer")]
	[LuaName("synth_layer")]
	public class SynthLayerProxy : LuaProxy<SynthLayer>
	{
		SynthNodeFactory nodeFactory;

		public SynthLayerProxy(SynthNodeFactory nodeFactory)
		{
			this.nodeFactory = nodeFactory;
		}


		[LuaHelpInfo("Sets a texture to be used as the input of the layer")]
		public SynthLayerProxy texture(ImageProxy texture)
		{
			if (texture == null || texture.Target == null)
				throw new ArgumentException("Texture is nil");

			target.InputTexture = texture.Target;
			return this;
		}

		[LuaHelpInfo("Sets the layers generator node")]
		public SynthLayerProxy generate(string name, params Table[] properties)
		{
			GenNode node = (GenNode)CreateNode(typeof(GenNode), name, properties);
			target.GenerateNode = node;
			return this;
		}

		[LuaHelpInfo("Adds a modulate node to the layer")]
		public SynthLayerProxy modulate(string name, params Table[] properties)
		{
			ModulateNode node = (ModulateNode)CreateNode(typeof(ModulateNode), name, properties);
			target.AddMutateNode(node);
			return this;
		}

		[LuaHelpInfo("Sets the layer blend mode")]
		public SynthLayerProxy blend(string name, params Table[] properties)
		{
			BlendNode node = (BlendNode)CreateNode(typeof(BlendNode), name, properties);
			target.BlendNode = node;
			return this;
		}


		private SynthNode CreateNode(Type nodeType, string name, Table[] properties)
		{
			SynthNode node = nodeFactory.CreateNode(nodeType, name);
			SynthNodeDescriptor descriptor = nodeFactory.GetNodeDescriptor(nodeType, name);

			// Set properties and register controls
			foreach (var prop in properties)
			{
				string propName = prop.Get(1).String;
				DynValue val = prop.Get(2);

				var propDescriptor = descriptor.GetProperty(propName);

				if (val.UserData != null && val.UserData.Object is IValueControlProxy controlProxy)
				{ 
					ValueControl control = controlProxy.Target;
					SetNodePropertyControl(node, propDescriptor, control);
				}
				else
					SetNodePropertyValue(node, propDescriptor, val);
			}
			return node;
		}

		private void SetNodePropertyControl(SynthNode node, SynthNodeDescriptor.PropertyDescriptor propertyDescriptor, ValueControl control)
		{
			switch (propertyDescriptor.Type)
			{
				case SynthNodeDescriptor.PropertyDescriptor.PropertyType.Float:
					var floatControl = control as ValueControl<float>;
					node.SetFloat(propertyDescriptor, floatControl.Value);
					floatControl.RegisterChangeCallback(val => node.SetFloat(propertyDescriptor, val));
					return;
				case SynthNodeDescriptor.PropertyDescriptor.PropertyType.Color:
					var colorControl = control as ValueControl<Color>;
					node.SetColor(propertyDescriptor, colorControl.Value);
					colorControl.RegisterChangeCallback(val => node.SetColor(propertyDescriptor, val));
					return;
				case SynthNodeDescriptor.PropertyDescriptor.PropertyType.Vector:
					var vectorControl = control as ValueControl<Vector2>;
					node.SetVector(propertyDescriptor, vectorControl.Value);
					vectorControl.RegisterChangeCallback(val => node.SetVector(propertyDescriptor, val));
					return;
				case SynthNodeDescriptor.PropertyDescriptor.PropertyType.Enum:
					if (control is ValueControl<string> stringControl)
					{
						node.SetEnum(propertyDescriptor, stringControl.Value);
						stringControl.RegisterChangeCallback(val => node.SetEnum(propertyDescriptor, val));
					}
					else if (control is ChoiceControl choiceControl)
					{
						node.SetEnum(propertyDescriptor, choiceControl.CurrentChoice);
						choiceControl.RegisterChangeCallback(val => node.SetEnum(propertyDescriptor, choiceControl.CurrentChoice));
					}
					return;
				case SynthNodeDescriptor.PropertyDescriptor.PropertyType.Texture:
					return;
				default:
					return;
			}
		}

		private void SetNodePropertyValue(SynthNode node, SynthNodeDescriptor.PropertyDescriptor propertyDescriptor, DynValue value)
		{
			switch (propertyDescriptor.Type)
			{
				case SynthNodeDescriptor.PropertyDescriptor.PropertyType.Float:
					node.SetFloat(propertyDescriptor, (float)value.Number);
					return;
				case SynthNodeDescriptor.PropertyDescriptor.PropertyType.Color:
					node.SetColor(propertyDescriptor, value.ToObject<Color>());
					return;
				case SynthNodeDescriptor.PropertyDescriptor.PropertyType.Vector:
					node.SetVector(propertyDescriptor, value.ToObject<Vector3>());
					return;
				case SynthNodeDescriptor.PropertyDescriptor.PropertyType.Enum:
					node.SetEnum(propertyDescriptor, value.String);
					return;
				case SynthNodeDescriptor.PropertyDescriptor.PropertyType.Texture:
					node.SetTexture(propertyDescriptor, ((ImageProxy)value.UserData.Object).Target);
					return;
				default:
					return;
			}
		}

	}
}

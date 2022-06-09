using System;
using UnityEngine;

namespace Fab.Synth
{
	/// <summary>
	/// Holds information about a synth channel
	/// </summary>
	[Serializable]
	public class SynthChannelData : ISerializationCallbackReceiver
	{
		[SerializeField]
		private string name;
		[SerializeField]
		private string targetPropertyName;
		[SerializeField]
		private CustomRenderTexture outputTexture;

		private int targetPropertyId;
		public string Name => name;
		public int TexturePropertyId => targetPropertyId;
		public int Id => targetPropertyId;
		public CustomRenderTexture OutputTexture => outputTexture;

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			targetPropertyId = Shader.PropertyToID(targetPropertyName);
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
		}
	}
}

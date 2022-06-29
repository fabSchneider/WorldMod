using UnityEngine;
using Fab.Synth;

namespace Fab.Synth.Samples
{
    public class SimpleSynth : MonoBehaviour
    {
        [SerializeField]
        private SynthComponent synth;

        private SynthLayer layerA;
        private SynthLayer layerB;
        private GenNode fillNodeA;
        private GenNode fillNodeB;
        private BlendNode blendNode;

        void Start()
        {
            layerA = new SynthLayer("Base");
            layerB = new SynthLayer("Base");

            fillNodeA = (GenNode)synth.NodeFactory.CreateNode(typeof(GenNode), "Fill");
            fillNodeB = (GenNode)synth.NodeFactory.CreateNode(typeof(GenNode), "Fill");

            blendNode = (BlendNode)synth.NodeFactory.CreateNode(typeof(BlendNode), "Add");

            layerA.GenerateNode = fillNodeA;
            layerB.GenerateNode = fillNodeB;
            layerB.BlendNode = blendNode;

            synth.AddLayer(layerA);
            synth.AddLayer(layerB);
        }

        void Update()
        {
            fillNodeA.SetColor("_Color", Color.Lerp(Color.red, Color.blue, Time.time % 1f));
            fillNodeB.SetColor("_Color", Color.Lerp(Color.green, Color.blue, (Time.time % 1f) * 0.2f));
            synth.SetChannelDirty("Base");
        }
    }
}

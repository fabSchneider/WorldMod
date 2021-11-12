using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace FabGeo.Samples
{
    [RequireComponent(typeof(MeshFilter))]
    public class SphereSample : MonoBehaviour
    {
        public int resolution = 4;
        public float equalize = 1f;

        private MeshFilter mf;
        void Start()
        {
            var mesh = new Mesh();
            mesh.name = "Sphere";
            mf = GetComponent<MeshFilter>();
            mf.mesh = mesh;
        }

        private void Update()
        {
            if (mf)
            {
                var meshDataArray = MeshUtils.CreateCube(1f, resolution);
                var meshData = meshDataArray[0];

                var positions = meshData.GetVertexData<float3>();
                var normals = meshData.GetVertexData<float3>(1);

                for (int i = 0; i < positions.Length; i++)
                {
                    float3 p = math.lerp(math.normalize(positions[i]), Geo.PointOnCubeToPointOnSphere(positions[i]), equalize);
                    positions[i] = p;
                    normals[i] = math.normalize(positions[i]);
                }

                Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mf.mesh);
            }
        }
    }
}
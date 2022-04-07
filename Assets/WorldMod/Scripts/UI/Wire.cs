
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fab.WorldMod.UI
{
	public static class BezierUtil
	{
		/// <summary>
		/// Returns a point along a quadratic bezier.
		/// </summary>
		public static Vector2 Bezier2(Vector2 a, Vector2 b, Vector2 c, float t)
		{
			Vector2 ab = a + t * (b - a);
			Vector2 ba = b + t * (c - b);
			return ab + t * (ba - ab);
		}

		/// <summary>
		/// Returns a point along a cubic bezier.
		/// </summary>
		public static Vector2 Bezier3(in Vector2 a, in Vector2 b, in Vector2 c, in Vector2 d, float t)
		{
			float t1 = 1 - t;
			float t12 = t1 * t1;
			float t13 = t1 * t1 * t1;
			return t13 * a
				   + 3 * t12 * t * b
				   + 3 * t1 * t * t * c
				   + t * t * t * d;
		}

		/// <summary>
		/// Returns the tangent along a cubic bezier.
		/// </summary>
		/// <param name="bezier">The bezier matrix. The first column represents the start, 
		/// the second the first tangent point, the third the second tangent point and the fourth the end of the curve</param>
		/// <param name="t">The parameter along the curve</param>
		/// <returns></returns>
		public static Vector2 Bezier3Tangent(in Vector2 a, in Vector2 b, in Vector2 c, in Vector2 d, float t)
		{
			float tInv = 1f - t;
			float tInvSQ = tInv * tInv;
			return -3f * tInvSQ * a
				+ (3f * tInvSQ - 6f * tInv * t) * b
				   + (-3f * t * t + 6f * t * tInv) * c
				   + 3f * t * t * d;
		}
	}

	public class Wire : VisualElement
	{

		private Vector2 a, b, c, d;
		private float thickness = 1f;
		private int resolution = 16;
		private bool useAdaptiveResolution;
		private Color tint;
		private Gradient gradient;

		private Texture texture;

		public Texture Texture
		{
			get => texture;
			set
			{
				texture = value;
				MarkDirtyRepaint();
			}
		}

		public Vector2 Start
		{
			get => a;
			set
			{
				if (a != value)
					SetEndpoints(value, d);
			}
		}

		public Vector2 End
		{
			get => d;
			set
			{
				if (d != value)
					SetEndpoints(a, value);
			}
		}

		public float Thickness
		{
			get => thickness;
			set
			{
				if (thickness != value)
				{
					thickness = value;
					MarkDirtyRepaint();
				}
			}
		}

		public Color Tint
		{
			get => tint;
			set
			{
				tint = value;
				MarkDirtyRepaint();
			}
		}

		public Gradient Gradient
		{
			get => gradient;
			set
			{
				gradient = value;
				MarkDirtyRepaint();
			}
		}

		public bool UseAdaptiveResolution
		{
			get => useAdaptiveResolution;
			set => useAdaptiveResolution = value;
		}

		public int Resolution
		{
			get => resolution;
			set
			{
				int res = math.clamp(value, 2, 512);
				if (res != resolution)
				{
					resolution = res;
					MarkDirtyRepaint();
				}
			}

		}

		public void SetEndpoints(Vector2 start, Vector2 end)
		{
			a = start;
			d = end;
			b = new Vector2(a.x + (d.x - a.x) / 2f, a.y);
			c = new Vector2(b.x, d.y);
			MarkDirtyRepaint();
		}

		public Wire()
		{
			generateVisualContent = GenerateVisualContent;
			AddToClassList("wire");
		}

		private void GenerateVisualContent(MeshGenerationContext context)
		{
			int res = useAdaptiveResolution ? (int)(math.distance(a, d) / resolution) + 4 : resolution;

			MeshWriteData mesh = context.Allocate((res + 1) * 2, res * 6, texture);

			Vector3 midPoint;
			Vector3 tangent;
			Vector3 biTangent;
			Color currTint;

			for (int i = 0; i < res; i++)
			{

				float t = (float)i / res;

				midPoint = BezierUtil.Bezier3(in a, in b, in c, in d, t);
				tangent = BezierUtil.Bezier3Tangent(in a, in b, in c, in d, t).normalized;
				biTangent = new Vector2(-tangent.y, tangent.x);
				int vID = i * 2;

				currTint = gradient == null ? tint :gradient.Evaluate(t) * tint;
				mesh.SetNextVertex(new Vertex()
				{
					position = midPoint - biTangent * thickness,
					tint = currTint,
					uv = new Vector2(t, 0f)
				});
				mesh.SetNextVertex(new Vertex()
				{
					position = midPoint + biTangent * thickness,
					tint = currTint,
					uv = new Vector2(t, 1f)
				});

				mesh.SetNextIndex((ushort)vID);
				mesh.SetNextIndex((ushort)(vID + 2));
				mesh.SetNextIndex((ushort)(vID + 1));
				mesh.SetNextIndex((ushort)(vID + 1));
				mesh.SetNextIndex((ushort)(vID + 2));
				mesh.SetNextIndex((ushort)(vID + 3));
			}

			midPoint = BezierUtil.Bezier3(in a, in b, in c, in d, 1f);
			tangent = BezierUtil.Bezier3Tangent(in a, in b, in c, in d, 1f).normalized;
			biTangent = new Vector2(-tangent.y, tangent.x);

			currTint = gradient == null ? tint : gradient.Evaluate(1f) * tint;

			mesh.SetNextVertex(new Vertex()
			{
				position = midPoint - biTangent * thickness,
				tint = currTint,
				uv = new Vector2(1f, 0f)
			});
			mesh.SetNextVertex(new Vertex()
			{
				position = midPoint + biTangent * thickness,
				tint = currTint,
				uv = new Vector2(1f, 1f)
			});
		}
	}
}

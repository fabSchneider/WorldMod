Shader "Layers/ChannelLerp"
{
    Properties
    {
        [NoScaleOffset]_MainTex ("InputTex", 2D) = "white" {}
        _Lerp ("Lerp", Float) = 0
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        ENDHLSL

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            struct VertexInput
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct VertexOutput
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            VertexOutput vert (VertexInput v)
            {
                VertexOutput o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                return o;
            }

            CBUFFER_START(UnityPerMaterial)
            float _Lerp;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 frag (VertexOutput i) : SV_Target
            {
                float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                float rg = lerp(col.x, col.y, min(1, _Lerp * 3));
                float gb = lerp(rg, col.z, max(0, (_Lerp - 1.0/3.0) * 1.5));
                float ba = lerp(gb, col.w, max(0, (_Lerp - 2.0/3.0) * 3));
                return ba;
            }

            ENDHLSL
        }
    }
}

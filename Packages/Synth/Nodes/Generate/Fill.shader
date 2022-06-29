Shader "Synth/Generate/Fill"
{
    Properties
    {
        _Color ("Color", Color) = (1,0,0,1)
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
            float4 _Color;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 frag (VertexOutput i) : SV_Target
            {
                return _Color;
            }

            ENDHLSL
        }
    }
}

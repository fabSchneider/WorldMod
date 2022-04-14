Shader "Layers/Blend/Lerp"
{
    Properties
    {
        [NoScaleOffset]_BaseTex ("BaseTex", 2D) = "black" {}
        [NoScaleOffset]_MainTex ("InputTex", 2D) = "black" {}
        _Opacity ("Opacity", Float) = 1.0
        
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
            float _Opacity;
            CBUFFER_END

            TEXTURE2D(_BaseTex);
            SAMPLER(sampler_BaseTex);
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 frag (VertexOutput i) : SV_Target
            {
                float4 baseCol = SAMPLE_TEXTURE2D(_BaseTex, sampler_BaseTex, i.uv);
                float4 blendCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                float t = max(baseCol.a, blendCol.a) * _Opacity;
                float3 blend =  lerp(baseCol.xyz, blendCol.xyz, _Opacity);
                return float4(blend, t); 
            }

            ENDHLSL
        }
    }
}

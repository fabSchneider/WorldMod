Shader "Layers/Mutate/Mask"
{
    Properties
    {
        [NoScaleOffset]_MainTex ("InputTex", 2D) = "white" {}
        [KeywordEnum(R, G, B, A)]_MASK("Mask", Float) = 0
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
            #pragma multi_compile _MASK_R _MASK_G _MASK_B _MASK_A

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

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 frag (VertexOutput i) : SV_Target
            {
                float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                #if _MASK_R
                return col.x;
                #elif _MASK_G
                return col.y;
                #elif _MASK_B
                return col.z;
                #elif _MASK_A
                return col.w;
                #else
                return col;
                #endif
            }

            ENDHLSL
        }
    }
}

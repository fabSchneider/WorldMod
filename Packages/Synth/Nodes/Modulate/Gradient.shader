Shader "Synth/Modulate/Gradient"
{
    Properties
    {
        [NoScaleOffset]_MainTex ("InputTex", 2D) = "white" {}
        [NoScaleOffset]_GradientTex ("GradientTex", 2D) = "white" {}
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
            TEXTURE2D(_GradientTex);
            SAMPLER(sampler_GradientTex);

            float4 frag (VertexOutput i) : SV_Target
            {
                float4 col =  SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                return SAMPLE_TEXTURE2D(_GradientTex, sampler_GradientTex, float2(clamp(col.x, 0.01, 0.99), 0.0));
            }

            ENDHLSL
        }
    }
}

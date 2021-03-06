Shader "Synth/Modulate/Clip"
{
    Properties
    {
        [NoScaleOffset]_MainTex ("InputTex", 2D) = "white" {}
        _Threshold ("Threshold", Float) = 0.5
        _Invert ("Invert", Float) = 0

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
            float _Threshold;
            float _Invert;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 frag (VertexOutput i) : SV_Target
            {
                float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                float a = lerp(col.a, 1.0 - col.a, _Invert);
                a = step(_Threshold, a);
                return float4(col.xyz, a);
            }

            ENDHLSL
        }
    }
}

Shader "Custom/SkyboxBlend"
{
    Properties
    {
        _Tint     ("Tint Color", Color) = (1,1,1,1)
        _Exposure ("Exposure", Range(0,8)) = 1.0
        _Rotation ("Rotation", Range(0,360)) = 0
        [NoScaleOffset] _Tex1 ("Skybox 1 (Cubemap)", Cube) = "grey" {}
        [NoScaleOffset] _Tex2 ("Skybox 2 (Cubemap)", Cube) = "grey" {}
        _Blend    ("Blend", Range(0,1)) = 0
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURECUBE(_Tex1); SAMPLER(sampler_Tex1);
            TEXTURECUBE(_Tex2); SAMPLER(sampler_Tex2);

            CBUFFER_START(UnityPerMaterial)
                half4 _Tint;
                half  _Exposure;
                float _Rotation;
                float _Blend;
            CBUFFER_END

            struct Attributes { float4 posOS : POSITION; };

            struct Varyings
            {
                float4 posCS : SV_POSITION;
                float3 dir   : TEXCOORD0;
            };

            float3 RotateAroundY(float3 v, float deg)
            {
                float rad = deg * PI / 180.0;
                float s, c;
                sincos(rad, s, c);
                return float3(v.x * c - v.z * s, v.y, v.x * s + v.z * c);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.posCS = TransformObjectToHClip(IN.posOS.xyz);
                OUT.dir   = IN.posOS.xyz;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 dir = RotateAroundY(normalize(IN.dir), _Rotation);

                half3 c1 = SAMPLE_TEXTURECUBE(_Tex1, sampler_Tex1, dir).rgb;
                half3 c2 = SAMPLE_TEXTURECUBE(_Tex2, sampler_Tex2, dir).rgb;

                half3 col = lerp(c1, c2, _Blend) * _Tint.rgb * _Exposure;
                return half4(col, 1.0);
            }
            ENDHLSL
        }
    }
    Fallback Off
}

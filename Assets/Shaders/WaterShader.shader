Shader "Custom/SpriteWater"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _WaveSpeed ("Wave Speed", Range(0.1, 10)) = 1
        _WaveAmplitude ("Wave Amplitude", Range(0, 0.1)) = 0.01
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        _NoiseTexture ("Noise Texture", 2D) = "white" {}
        _FoamColor ("Foam Color", Color) = (1,1,1,1)
        _FoamWidth ("Foam Width", Range(0, 0.1)) = 0.02
        _FoamNoise ("Foam Noise", Range(0, 1)) = 0.5
        _FoamSpeed ("Foam Speed", Range(0, 2)) = 0.5
    }

    SubShader
    {
        Tags
        { 
            "Queue"="Transparent" 
            "RenderType"="Transparent" 
            "RenderPipeline"="UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_NoiseTexture);
            SAMPLER(sampler_NoiseTexture);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _RendererColor;
                float4 _Flip;
                float _WaveSpeed;
                float _WaveAmplitude;
                float4 _NoiseTexture_ST;
                float4 _FoamColor;
                float _FoamWidth;
                float _FoamNoise;
                float _FoamSpeed;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT = (Varyings)0;
                
                // Apply wave effect
                float wave = sin(_Time.y * _WaveSpeed + IN.positionOS.x * 2) * _WaveAmplitude;
                IN.positionOS.y += wave;
                
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color = IN.color * _Color * _RendererColor;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                
                if (mainTex.a < 0.1) 
                    discard;
                
                // Foam effect
                float2 noiseUV = IN.uv * 2 + _Time.y * _FoamSpeed;
                float noise = SAMPLE_TEXTURE2D(_NoiseTexture, sampler_NoiseTexture, noiseUV).r;
                
                float2 dx = ddx(mainTex.a);
                float2 dy = ddy(mainTex.a);
                float gradientMagnitude = sqrt(dot(dx, dx) + dot(dy, dy));
                
                float foamMask = smoothstep(0, _FoamWidth, gradientMagnitude);
                foamMask *= noise * _FoamNoise + (1 - _FoamNoise);
                
                // Final color
                float4 finalColor = mainTex * IN.color;
                finalColor.rgb = lerp(finalColor.rgb, _FoamColor.rgb, foamMask * _FoamColor.a);
                
                return finalColor;
            }
            ENDHLSL
        }
    }
}

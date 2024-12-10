Shader "Custom/2D/TerrainSandURP"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _SandColor ("Sand Color", Color) = (0.93, 0.87, 0.73, 1)
        _SandWidth ("Sand Width", Range(0.0, 0.1)) = 0.02
    }

    SubShader
    {
        Tags { 
            "RenderType" = "Transparent" 
            "RenderPipeline" = "UniversalPipeline" 
            "Queue" = "Transparent"
            "PreviewType" = "Plane"
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
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _MainTex_TexelSize;
                float4 _SandColor;
                float _SandWidth;
            CBUFFER_END

            float2 unity_gradientNoise_dir(float2 p)
            {
                p = p % 289;
                float x = (34 * p.x + 1) * p.x % 289 + p.y;
                x = (34 * x + 1) * x % 289;
                x = frac(x / 41) * 2 - 1;
                return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
            }

            float unity_gradientNoise(float2 p)
            {
                float2 ip = floor(p);
                float2 fp = frac(p);
                float d00 = dot(unity_gradientNoise_dir(ip), fp);
                float d01 = dot(unity_gradientNoise_dir(ip + float2(0, 1)), fp - float2(0, 1));
                float d10 = dot(unity_gradientNoise_dir(ip + float2(1, 0)), fp - float2(1, 0));
                float d11 = dot(unity_gradientNoise_dir(ip + float2(1, 1)), fp - float2(1, 1));
                fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
                return lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x) + 0.5;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.screenPos = ComputeScreenPos(OUT.positionHCS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;
                float4 center = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                
                if (center.a < 0.01) 
                    return half4(0, 0, 0, 0);

                float2 texelSize = _MainTex_TexelSize.xy;
                float gradient = 0;
                
                // Calcular gradiente usando Sobel
                for(int x = -1; x <= 1; x++)
                {
                    for(int y = -1; y <= 1; y++)
                    {
                        float2 offset = float2(x, y) * texelSize;
                        float sample = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + offset).a;
                        gradient += abs(sample - center.a);
                    }
                }
                
                float noise = unity_gradientNoise(uv * 50) * 0.5 + 0.5;
                float edge = gradient * 2 * noise;
                float sandMask = smoothstep(0, _SandWidth, edge);
                
                float3 finalColor = lerp(center.rgb, _SandColor.rgb, sandMask);
                return half4(finalColor, center.a);
            }
            ENDHLSL
        }
    }
}
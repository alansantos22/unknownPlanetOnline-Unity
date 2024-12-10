Shader "Custom/Clouds"
{
    Properties
    {
        _MainColor ("Cloud Color", Color) = (1,1,1,1)
        _Scale ("Scale", Range(1, 100)) = 10
        _Speed ("Speed", Range(0, 2)) = 0.5
        _Density ("Cloud Density", Range(0, 2)) = 1
        _Contrast ("Cloud Contrast", Range(1, 5)) = 2
        _Softness ("Cloud Softness", Range(0, 1)) = 0.5
        _CloudShape ("Cloud Shape", Range(0, 1)) = 0.5
        _LayerHeight ("Cloud Layer Height", Range(0, 1)) = 0.5
        _Coverage ("Cloud Coverage", Range(0, 1)) = 0.5
        _WindDirection ("Wind Direction", Range(0, 360)) = 0
        _Turbulence ("Cloud Turbulence", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            fixed4 _MainColor;
            float _Scale;
            float _Speed;
            float _Density;
            float _Contrast;
            float _Softness;
            float _CloudShape;
            float _LayerHeight;
            float _Coverage;
            float _WindDirection;
            float _Turbulence;

            float random(float2 st)
            {
                return frac(sin(dot(st.xy, float2(12.9898,78.233))) * 43758.5453123);
            }

            float noise(float2 st)
            {
                float2 i = floor(st);
                float2 f = frac(st);

                float a = random(i);
                float b = random(i + float2(1.0, 0.0));
                float c = random(i + float2(0.0, 1.0));
                float d = random(i + float2(1.0, 1.0));

                float2 u = f * f * (3.0 - 2.0 * f);

                return lerp(a, b, u.x) + 
                       (c - a)* u.y * (1.0 - u.x) + 
                       (d - b) * u.x * u.y;
            }

            float fbm(float2 pos, int octaves) {
                float value = 0.0;
                float amplitude = 0.5;
                float frequency = 1.0;
                float2 shift = float2(100, 100);
                
                for(int i = 0; i < octaves; i++) {
                    value += noise(pos * frequency + shift) * amplitude;
                    shift = pos;
                    amplitude *= 0.5;
                    frequency *= 2.0;
                    // Adiciona turbulência
                    pos += float2(sin(pos.y), cos(pos.x)) * _Turbulence;
                }
                return value;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float windRad = _WindDirection * 3.14159 / 180.0;
                float2 windDir = float2(cos(windRad), sin(windRad));
                
                float2 pos = i.uv * _Scale;
                float2 movement = _Time.y * _Speed * windDir;
                
                // Adicionar movimento mais orgânico
                movement += float2(
                    sin(_Time.y * 0.1) * 0.2,
                    cos(_Time.y * 0.15) * 0.2
                ) * _Turbulence;
                
                pos += movement;
                
                // Criar camadas mais naturais
                float baseLayer = fbm(pos, 4);
                float highLayer = fbm(pos * 1.2 + movement * 0.3, 3);
                float detailLayer = fbm(pos * 2.5 - movement * 0.2, 2);
                
                // Misturar camadas de forma mais suave
                float heightVar = lerp(baseLayer, highLayer, _LayerHeight);
                heightVar = lerp(heightVar, detailLayer, 0.2);
                
                // Suavizar a cobertura
                float coverage = 1.0 - _Coverage;
                float clouds = smoothstep(coverage - 0.2, coverage + 0.3, heightVar);
                
                // Adicionar detalhes mais orgânicos
                float details = fbm(pos * 3.0 + heightVar * 0.5, 2);
                clouds = lerp(clouds, clouds * (1.0 + details * _CloudShape), 0.7);
                
                // Suavização mais natural
                clouds = pow(clouds, _Contrast * 0.8);
                float finalCloud = lerp(
                    clouds,
                    smoothstep(0.2, 0.8, clouds),
                    _Softness
                );
                
                // Suavizar bordas
                float alpha = finalCloud * _Density;
                alpha *= smoothstep(0.0, 0.3, finalCloud);
                alpha = saturate(alpha);
                
                fixed4 col = _MainColor;
                col.a *= alpha;
                
                return col;
            }
            ENDCG
        }
    }
}

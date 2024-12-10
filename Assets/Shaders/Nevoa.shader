Shader "Custom/Clouds"
{
    Properties
    {
        _MainColor ("Cloud Color", Color) = (1,1,1,1)
        _Scale ("Scale", Range(1, 100)) = 10
        _Speed ("Speed", Range(0, 2)) = 0.5
        _Density ("Cloud Density", Range(0, 2)) = 1
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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 pos = i.uv * _Scale;
                pos.x += _Time.y * _Speed;
                
                float n = noise(pos) * 0.5;
                n += noise(pos * 2) * 0.25;
                n += noise(pos * 4) * 0.125;
                n += noise(pos * 8) * 0.0625;
                
                n = saturate(n * _Density);
                
                fixed4 col = _MainColor;
                col.a *= n;
                
                return col;
            }
            ENDCG
        }
    }
}

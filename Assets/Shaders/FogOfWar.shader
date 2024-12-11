Shader "Custom/FogOfWar2D"
{
    // Propriedades visíveis no Inspector
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}        // Textura base (não utilizada)
        _FogColor ("Fog Color", Color) = (0,0,0,1)         // Cor do fog
        _ExploredAreas ("Explored Areas", 2D) = "black" {} // Máscara de áreas exploradas
        _Smoothness ("Edge Smoothness", Range(0.001, 0.1)) = 0.05  // Suavização das bordas
    }
    
    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "IgnoreProjector"="True"
            "RenderPipeline"="UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        
        Pass
        {
            Name "Universal2D"
            Tags { "LightMode" = "Universal2D" }
            
            HLSLPROGRAM
            // Inclui funções básicas do URP
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            // Define as funções do vertex e fragment shader
            #pragma vertex vert
            #pragma fragment frag

            // Estruturas de dados para passar informações entre shaders
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_ExploredAreas);
            SAMPLER(sampler_ExploredAreas);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _FogColor;
                float _Smoothness;
            CBUFFER_END

            // Função do vertex shader - processa vértices
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.texcoord;
                return OUT;
            }

            // Função do fragment shader - processa pixels
            float4 frag(Varyings IN) : SV_Target
            {
                float explored = SAMPLE_TEXTURE2D(_ExploredAreas, sampler_ExploredAreas, IN.uv).r;
                explored = smoothstep(0.0, _Smoothness, explored);
                return lerp(_FogColor, float4(0,0,0,0), explored);
            }
            ENDHLSL
        }
    }
}
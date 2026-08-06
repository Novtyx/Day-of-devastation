Shader "Custom/TilemapColorBlend"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Texture", 2D) = "white" {}
        _SnowTexture("Snow Texture", 2D) = "white" {}
        _SnowColor("Snow Color", Color) = (1,1,1,1)
        _SnowIntensity("Snow Intensity", Range(0, 1)) = 0.5
        _SnowScale("Snow Scale", Range(0.1, 10)) = 1
        _SnowThreshold("Snow Threshold", Range(0, 1)) = 0.5
        _SnowNoise("Snow Noise", Range(0, 1)) = 0.2
    }

        SubShader
        {
            Tags
            {
                "RenderType" = "Transparent"
                "RenderPipeline" = "UniversalPipeline"
                "UniversalMaterialType" = "Lit"
                "IgnoreProjector" = "True"
                "Queue" = "Transparent"
            }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            Pass
            {
                Name "ForwardLit"
                Tags { "LightMode" = "Universal2D" }

                HLSLPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

                struct Attributes
                {
                    float4 positionOS : POSITION;
                    float2 uv : TEXCOORD0;
                    float4 color : COLOR;
                };

                struct Vars
                {
                    float4 positionCS : SV_POSITION;
                    float2 uv : TEXCOORD0;
                    float4 color : COLOR;
                    float2 snowUV : TEXCOORD1;
                };

                TEXTURE2D(_BaseMap);
                SAMPLER(sampler_BaseMap);
                TEXTURE2D(_SnowTexture);
                SAMPLER(sampler_SnowTexture);

                CBUFFER_START(UnityPerMaterial)
                    float4 _BaseMap_ST;
                    float4 _SnowTexture_ST;
                    half4 _SnowColor;
                    half _SnowIntensity;
                    half _SnowScale;
                    half _SnowThreshold;
                    half _SnowNoise;
                CBUFFER_END

                    // Простая шумовая функция для случайных вкраплений
                    float random(float2 uv)
                    {
                        return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
                    }

                    Vars vert(Attributes input)
                    {
                        Vars output;

                        output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                        output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                        output.snowUV = input.uv * _SnowScale;
                        output.color = input.color;
                        return output;
                    }

                    half4 frag(Vars input) : SV_Target
                    {
                        half4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                        texColor *= input.color;

                        // Получаем текстуру снега
                        half4 snowTex = SAMPLE_TEXTURE2D(_SnowTexture, sampler_SnowTexture, input.snowUV);

                        // Добавляем шум для более естественного вида
                        float noise = random(input.snowUV);
                        snowTex.rgb *= lerp(0.8, 1.2, noise);

                        // Комбинируем текстуру снега с пороговым значением
                        half snowPattern = snowTex.r * snowTex.a;
                        snowPattern = saturate(snowPattern - _SnowThreshold) / (1 - _SnowThreshold);

                        // Добавляем случайные вкрапления через шум
                        half noisePattern = step(_SnowNoise, random(input.snowUV * 2.0));
                        snowPattern = max(snowPattern, noisePattern * 0.3);

                        // Смешиваем с оригинальным цветом
                        half4 finalColor = texColor;
                        half snowEffect = snowPattern * _SnowIntensity;
                        finalColor.rgb = lerp(texColor.rgb, _SnowColor.rgb, snowEffect);

                        // Сохраняем оригинальную прозрачность
                        finalColor.a = texColor.a;

                        return finalColor;
                    }
                    ENDHLSL
                }
        }
}
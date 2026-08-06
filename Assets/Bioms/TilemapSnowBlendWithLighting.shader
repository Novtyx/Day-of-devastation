Shader "Custom/TilemapSnowBlendWithLighting"
{
    Properties
    {
        [MainTexture] _MainTex("Diffuse", 2D) = "white" {}
        _MaskTex("Mask", 2D) = "white" {}
        _NormalMap("Normal Map", 2D) = "bump" {}

        _SnowTexture("Snow Texture", 2D) = "white" {}
        _SnowColor("Snow Color", Color) = (1,1,1,1)
        _SnowIntensity("Snow Intensity", Range(0, 1)) = 0.5
        _SnowScale("Snow Scale", Range(0.1, 10)) = 1
        _SnowThreshold("Snow Threshold", Range(0, 1)) = 0.5
        _SnowNoise("Snow Noise", Range(0, 1)) = 0.2

            // Legacy properties
            [HideInInspector] _Color("Tint", Color) = (1,1,1,1)
            [HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
            [HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
            [HideInInspector] _AlphaTex("External Alpha", 2D) = "white" {}
            [HideInInspector] _EnableExternalAlpha("Enable External Alpha", Float) = 0
    }

        SubShader
            {
                Tags
                {
                    "Queue" = "Transparent"
                    "RenderType" = "Transparent"
                    "RenderPipeline" = "UniversalPipeline"
                }

                Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
                Cull Off
                ZWrite Off

                Pass
                {
                    Tags { "LightMode" = "Universal2D" }

                    HLSLPROGRAM
                    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
                    #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/LightingUtility.hlsl"

                    #pragma vertex CombinedShapeLightVertex
                    #pragma fragment CombinedShapeLightFragment

                    #pragma multi_compile USE_SHAPE_LIGHT_TYPE_0 __
                    #pragma multi_compile USE_SHAPE_LIGHT_TYPE_1 __
                    #pragma multi_compile USE_SHAPE_LIGHT_TYPE_2 __
                    #pragma multi_compile USE_SHAPE_LIGHT_TYPE_3 __
                    #pragma multi_compile _ DEBUG_DISPLAY

                    struct Attributes
                    {
                        float3 positionOS   : POSITION;
                        float4 color        : COLOR;
                        float2 uv           : TEXCOORD0;
                        UNITY_VERTEX_INPUT_INSTANCE_ID
                    };

                    struct Varyings
                    {
                        float4 positionCS   : SV_POSITION;
                        half4 color         : COLOR;
                        float2 uv           : TEXCOORD0;
                        half2 lightingUV    : TEXCOORD1;
                        float2 snowUV       : TEXCOORD2;
                        #if defined(DEBUG_DISPLAY)
                        float3 positionWS   : TEXCOORD3;
                        #endif
                        UNITY_VERTEX_OUTPUT_STEREO
                    };

                    TEXTURE2D(_MainTex);
                    SAMPLER(sampler_MainTex);
                    TEXTURE2D(_MaskTex);
                    SAMPLER(sampler_MaskTex);

                    TEXTURE2D(_SnowTexture);
                    SAMPLER(sampler_SnowTexture);

                    half4 _MainTex_ST;
                    float4 _Color;
                    half4 _RendererColor;

                    // Snow properties
                    half4 _SnowColor;
                    half _SnowIntensity;
                    half _SnowScale;
                    half _SnowThreshold;
                    half _SnowNoise;

                    #if USE_SHAPE_LIGHT_TYPE_0
                    SHAPE_LIGHT(0)
                    #endif

                    #if USE_SHAPE_LIGHT_TYPE_1
                    SHAPE_LIGHT(1)
                    #endif

                    #if USE_SHAPE_LIGHT_TYPE_2
                    SHAPE_LIGHT(2)
                    #endif

                    #if USE_SHAPE_LIGHT_TYPE_3
                    SHAPE_LIGHT(3)
                    #endif

                        // Шумовая функция для снега
                        float random(float2 uv)
                        {
                            return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
                        }

                    // Функция применения снега
                    half4 ApplySnowEffect(half4 originalColor, float2 snowUV)
                    {
                        // Получаем текстуру снега
                        half4 snowTex = SAMPLE_TEXTURE2D(_SnowTexture, sampler_SnowTexture, snowUV);

                        // Добавляем шум для более естественного вида
                        float noise = random(snowUV);
                        snowTex.rgb *= lerp(0.8, 1.2, noise);

                        // Комбинируем текстуру снега с пороговым значением
                        half snowPattern = snowTex.r * snowTex.a;
                        snowPattern = saturate(snowPattern - _SnowThreshold) / (1 - _SnowThreshold);

                        // Добавляем случайные вкрапления через шум
                        half noisePattern = step(_SnowNoise, random(snowUV * 2.0));
                        snowPattern = max(snowPattern, noisePattern * 0.3);

                        // Смешиваем с оригинальным цветом
                        half4 finalColor = originalColor;
                        half snowEffect = snowPattern * _SnowIntensity;
                        finalColor.rgb = lerp(originalColor.rgb, _SnowColor.rgb, snowEffect);

                        // Сохраняем оригинальную прозрачность
                        finalColor.a = originalColor.a;

                        return finalColor;
                    }

                    Varyings CombinedShapeLightVertex(Attributes v)
                    {
                        Varyings o = (Varyings)0;
                        UNITY_SETUP_INSTANCE_ID(v);
                        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                        o.positionCS = TransformObjectToHClip(v.positionOS);
                        #if defined(DEBUG_DISPLAY)
                        o.positionWS = TransformObjectToWorld(v.positionOS);
                        #endif
                        o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                        o.snowUV = v.uv * _SnowScale;
                        o.lightingUV = half2(ComputeScreenPos(o.positionCS / o.positionCS.w).xy);

                        o.color = v.color * _Color * _RendererColor;
                        return o;
                    }

                    #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/CombinedShapeLightShared.hlsl"

                    half4 CombinedShapeLightFragment(Varyings i) : SV_Target
                    {
                        // Получаем основной цвет и применяем снег ДО освещения
                        half4 main = i.color * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                        half4 snowMain = ApplySnowEffect(main, i.snowUV);

                        const half4 mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.uv);
                        SurfaceData2D surfaceData;
                        InputData2D inputData;

                        // Используем модифицированный цвет со снегом для освещения
                        InitializeSurfaceData(snowMain.rgb, snowMain.a, mask, surfaceData);
                        InitializeInputData(i.uv, i.lightingUV, inputData);

                        return CombinedShapeLightShared(surfaceData, inputData);
                    }
                    ENDHLSL
                }

                Pass
                {
                    Tags { "LightMode" = "NormalsRendering"}

                    HLSLPROGRAM
                    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

                    #pragma vertex NormalsRenderingVertex
                    #pragma fragment NormalsRenderingFragment

                    struct Attributes
                    {
                        float3 positionOS   : POSITION;
                        float4 color        : COLOR;
                        float2 uv           : TEXCOORD0;
                        float4 tangent      : TANGENT;
                        UNITY_VERTEX_INPUT_INSTANCE_ID
                    };

                    struct Varyings
                    {
                        float4 positionCS      : SV_POSITION;
                        half4 color            : COLOR;
                        float2 uv              : TEXCOORD0;
                        half3 normalWS         : TEXCOORD1;
                        half3 tangentWS        : TEXCOORD2;
                        half3 bitangentWS      : TEXCOORD3;
                        UNITY_VERTEX_OUTPUT_STEREO
                    };

                    TEXTURE2D(_MainTex);
                    SAMPLER(sampler_MainTex);
                    TEXTURE2D(_NormalMap);
                    SAMPLER(sampler_NormalMap);
                    half4 _NormalMap_ST;

                    Varyings NormalsRenderingVertex(Attributes attributes)
                    {
                        Varyings o = (Varyings)0;
                        UNITY_SETUP_INSTANCE_ID(attributes);
                        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                        o.positionCS = TransformObjectToHClip(attributes.positionOS);
                        o.uv = TRANSFORM_TEX(attributes.uv, _NormalMap);
                        o.color = attributes.color;
                        o.normalWS = -GetViewForwardDir();
                        o.tangentWS = TransformObjectToWorldDir(attributes.tangent.xyz);
                        o.bitangentWS = cross(o.normalWS, o.tangentWS) * attributes.tangent.w;
                        return o;
                    }

                    #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/NormalsRenderingShared.hlsl"

                    half4 NormalsRenderingFragment(Varyings i) : SV_Target
                    {
                        const half4 mainTex = i.color * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                        const half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, i.uv));

                        return NormalsRenderingShared(mainTex, normalTS, i.tangentWS.xyz, i.bitangentWS.xyz, i.normalWS.xyz);
                    }
                    ENDHLSL
                }

                Pass
                {
                    Tags { "LightMode" = "UniversalForward" "Queue" = "Transparent" "RenderType" = "Transparent"}

                    HLSLPROGRAM
                    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

                    #pragma vertex UnlitVertex
                    #pragma fragment UnlitFragment

                    struct Attributes
                    {
                        float3 positionOS   : POSITION;
                        float4 color        : COLOR;
                        float2 uv           : TEXCOORD0;
                        UNITY_VERTEX_INPUT_INSTANCE_ID
                    };

                    struct Varyings
                    {
                        float4 positionCS      : SV_POSITION;
                        float4 color           : COLOR;
                        float2 uv              : TEXCOORD0;
                        float2 snowUV          : TEXCOORD1;
                        #if defined(DEBUG_DISPLAY)
                        float3 positionWS      : TEXCOORD2;
                        #endif
                        UNITY_VERTEX_OUTPUT_STEREO
                    };

                    TEXTURE2D(_MainTex);
                    SAMPLER(sampler_MainTex);
                    TEXTURE2D(_SnowTexture);
                    SAMPLER(sampler_SnowTexture);

                    float4 _MainTex_ST;
                    float4 _Color;
                    half4 _RendererColor;

                    // Snow properties
                    half4 _SnowColor;
                    half _SnowIntensity;
                    half _SnowScale;
                    half _SnowThreshold;
                    half _SnowNoise;

                    // Шумовая функция для снега
                    float random(float2 uv)
                    {
                        return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
                    }

                    // Функция применения снега
                    half4 ApplySnowEffect(half4 originalColor, float2 snowUV)
                    {
                        half4 snowTex = SAMPLE_TEXTURE2D(_SnowTexture, sampler_SnowTexture, snowUV);
                        float noise = random(snowUV);
                        snowTex.rgb *= lerp(0.8, 1.2, noise);

                        half snowPattern = snowTex.r * snowTex.a;
                        snowPattern = saturate(snowPattern - _SnowThreshold) / (1 - _SnowThreshold);

                        half noisePattern = step(_SnowNoise, random(snowUV * 2.0));
                        snowPattern = max(snowPattern, noisePattern * 0.3);

                        half4 finalColor = originalColor;
                        half snowEffect = snowPattern * _SnowIntensity;
                        finalColor.rgb = lerp(originalColor.rgb, _SnowColor.rgb, snowEffect);
                        finalColor.a = originalColor.a;

                        return finalColor;
                    }

                    Varyings UnlitVertex(Attributes attributes)
                    {
                        Varyings o = (Varyings)0;
                        UNITY_SETUP_INSTANCE_ID(attributes);
                        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                        o.positionCS = TransformObjectToHClip(attributes.positionOS);
                        #if defined(DEBUG_DISPLAY)
                        o.positionWS = TransformObjectToWorld(attributes.positionOS);
                        #endif
                        o.uv = TRANSFORM_TEX(attributes.uv, _MainTex);
                        o.snowUV = attributes.uv * _SnowScale;
                        o.color = attributes.color * _Color * _RendererColor;
                        return o;
                    }

                    float4 UnlitFragment(Varyings i) : SV_Target
                    {
                        float4 mainTex = i.color * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                        float4 finalColor = ApplySnowEffect(mainTex, i.snowUV);

                        #if defined(DEBUG_DISPLAY)
                        SurfaceData2D surfaceData;
                        InputData2D inputData;
                        half4 debugColor = 0;

                        InitializeSurfaceData(finalColor.rgb, finalColor.a, surfaceData);
                        InitializeInputData(i.uv, inputData);
                        SETUP_DEBUG_DATA_2D(inputData, i.positionWS);

                        if (CanDebugOverrideOutputColor(surfaceData, inputData, debugColor))
                        {
                            return debugColor;
                        }
                        #endif

                        return finalColor;
                    }
                    ENDHLSL
                }
            }

                Fallback "Sprites/Default"
}

Shader "Custom/ImplosionTransition"
{
    Properties
    {
        [HideInInspector] _MainTex("Base Texture", 2D) = "white" {}

    // Главный ползунок: 0 - нормальный экран, 1 - всё стянуто в точку
    _Progress("Transition Progress", Range(0, 1)) = 0.0

        // Насколько сильно искажаются края (эффект рыбьего глаза / линзы)
        _PinchStrength("Pinch Strength", Range(0, 5)) = 2.0

        // Максимальное отдаление (во сколько раз UV увеличится к концу)
        _ScaleMult("Scale Multiplier", Range(1, 20)) = 10.0
    }

        SubShader
    {
        // Теги для работы в UI
        Tags { "RenderType" = "Transparent" "Queue" = "Overlay" }
        Cull Off ZWrite Off ZTest Always

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
                float4 color : COLOR; // Для поддержки цвета UI компонента
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float _Progress;
                float _PinchStrength;
                float _ScaleMult;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Центр экрана
                float2 center = float2(0.5, 0.5);

                // Вектор от центра к текущему пикселю
                float2 dir = input.uv - center;

                // Расстояние от центра
                float dist = length(dir);

                // 1. Вычисляем общий масштаб
                // Чем ближе _Progress к 1, тем сильнее отдаляем (увеличивая UV)
                float currentScale = lerp(1.0, _ScaleMult, _Progress);

                // 2. Добавляем искажение пространства (линзу)
                // Чем дальше от центра, тем сильнее тянутся пиксели
                float pinch = lerp(0.0, _PinchStrength, _Progress);
                float distortion = 1.0 + pinch * dist;

                // 3. Вычисляем финальные UV
                float2 finalUV = center + dir * currentScale * distortion;

                // 4. Отрезаем всё, что вышло за края экрана (оставляем черное пространство)
                if (finalUV.x < 0.0 || finalUV.x > 1.0 || finalUV.y < 0.0 || finalUV.y > 1.0)
                {
                    // Возвращаем полностью черный цвет без прозрачности
                    return half4(0.0, 0.0, 0.0, 1.0);
                }

                // Читаем текстуру по искаженным координатам
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, finalUV);

                return texColor * input.color;
            }
            ENDHLSL
        }
    }
}


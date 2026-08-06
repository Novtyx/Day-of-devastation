Shader "Custom/Water2DLit_ProceduralNew"
{
	Properties
	{
	[HideInInspector] _BaseMap("Base Texture (Ignored)", 2D) = "white" {}
	[Header(Water Colors)]
	[MainColor] _BaseColor("Shallow Water Color", Color) = (0.2, 0.4, 0.35, 1)
	_DeepColor("Deep Water Color", Color) = (0.1, 0.25, 0.2, 1)
	[Header(Water Flow and Noise)]
	_FlowVector("Flow Vector (Speed X, Speed Y)", Vector) = (0.5, 0.2, 0, 0)
	_NoiseScale("Perlin Noise Scale", Float) = 3.0
	[Header(Highlights)]
	_HighlightColor("Highlight Color", Color) = (1, 1, 1, 1)
	_HighlightDensity("Highlight Density", Range(0.0, 1.0)) = 0.05
	_HighlightScale("Highlight Scale (Pixelation)", Float) = 15.0
	_HighlightSpeed("Highlight Blink Speed", Float) = 2.0
	[Header(Blending)]
	[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend", Float) = 1
	[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend", Float) = 0
	[Enum(Off,0,On,1)] _ZWrite("Z Write", Float) = 1
	[Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 2
	}
		SubShader
	{
	Tags
	{
	"RenderType" = "Transparent"
	"RenderPipeline" = "UniversalPipeline"
	"Queue" = "Transparent"
	"IgnoreProjector" = "True"
	}
	Pass
	{
	Name "ForwardLit"
	Tags { "LightMode" = "Universal2D" }
	Blend[_SrcBlend][_DstBlend]
	ZWrite[_ZWrite]
	Cull[_Cull]
	HLSLPROGRAM
	#pragma vertex vert
	#pragma fragment frag
	#pragma multi_compile_instancing
	#pragma multi_compile _ USE_SHAPE_LIGHT_TYPE_0
	#pragma multi_compile _ USE_SHAPE_LIGHT_TYPE_1
	#pragma multi_compile _ USE_SHAPE_LIGHT_TYPE_2
	#pragma multi_compile _ USE_SHAPE_LIGHT_TYPE_3
	#pragma multi_compile _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
	#include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/LightingUtility.hlsl"
	// Явное объявление текстур 2D-освещения URP
	#if USE_SHAPE_LIGHT_TYPE_0
	TEXTURE2D(_ShapeLightTexture0);
	SAMPLER(sampler_ShapeLightTexture0);
	#endif
	#if USE_SHAPE_LIGHT_TYPE_1
	TEXTURE2D(_ShapeLightTexture1);
	SAMPLER(sampler_ShapeLightTexture1);
	#endif
	#if USE_SHAPE_LIGHT_TYPE_2
	TEXTURE2D(_ShapeLightTexture2);
	SAMPLER(sampler_ShapeLightTexture2);
	#endif
	#if USE_SHAPE_LIGHT_TYPE_3
	TEXTURE2D(_ShapeLightTexture3);
	SAMPLER(sampler_ShapeLightTexture3);
	#endif
	UNITY_INSTANCING_BUFFER_START(Props)
	UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
	UNITY_DEFINE_INSTANCED_PROP(float4, _DeepColor)
	UNITY_DEFINE_INSTANCED_PROP(float4, _HighlightColor)
	UNITY_DEFINE_INSTANCED_PROP(float4, _FlowVector)
	UNITY_DEFINE_INSTANCED_PROP(float, _NoiseScale)
	UNITY_DEFINE_INSTANCED_PROP(float, _HighlightDensity)
	UNITY_DEFINE_INSTANCED_PROP(float, _HighlightScale)
	UNITY_DEFINE_INSTANCED_PROP(float, _HighlightSpeed)
	UNITY_INSTANCING_BUFFER_END(Props)
	struct Attributes
	{
	float4 positionOS : POSITION;
	float2 uv : TEXCOORD0;
	float4 color : COLOR;
	UNITY_VERTEX_INPUT_INSTANCE_ID
	};
	struct Varyings
	{
	float4 positionCS : SV_POSITION;
	float2 uv : TEXCOORD0;
	half4 color : COLOR;
	float3 positionWS : TEXCOORD1;
	float2 screenUV : TEXCOORD2;
	UNITY_VERTEX_INPUT_INSTANCE_ID
	};
	// ---- ФУНКЦИИ ГЕНЕРАЦИИ ШУМА (БЕЗ ТЕКСТУР) ----
	// Псевдорандом для шума Перлина
	float2 unity_gradientNoise_dir(float2 p)
	{
	p = p % 289;
	float x = (34 * p.x + 1) * p.x % 289 + p.y;
	x = (34 * x + 1) * x % 289;
	x = frac(x / 41) * 2 - 1;
	return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
	}
	// Классический 2D шум (Gradient Noise)
	float proceduralPerlin(float2 p)
	{
	float2 ip = floor(p);
	float2 fp = frac(p);
	float2 d00 = float2(0, 0);
	float2 d01 = float2(0, 1);
	float2 d10 = float2(1, 0);
	float2 d11 = float2(1, 1);
	float p00 = dot(unity_gradientNoise_dir(ip + d00), fp - d00);
	float p01 = dot(unity_gradientNoise_dir(ip + d01), fp - d01);
	float p10 = dot(unity_gradientNoise_dir(ip + d10), fp - d10);
	float p11 = dot(unity_gradientNoise_dir(ip + d11), fp - d11);
	fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
	return lerp(lerp(p00, p10, fp.x), lerp(p01, p11, fp.x), fp.y);
	}
	// Псевдорандом для бликов (белый шум)
	float random(float2 uv)
	{
	return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453123);
	}
	// ---------------------------------------------
	Varyings vert(Attributes input)
	{
	Varyings output;
	UNITY_SETUP_INSTANCE_ID(input);
	UNITY_TRANSFER_INSTANCE_ID(input, output);
	VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
	output.positionCS = vertexInput.positionCS;
	output.positionWS = vertexInput.positionWS;
	// Вычисляем экранные координаты для правильного рендера 2D-света в URP
	float4 screenPos = ComputeScreenPos(vertexInput.positionCS);
	output.screenUV = screenPos.xy / screenPos.w;
	output.uv = input.uv;
	output.color = input.color;
	return output;
	}
	// Функция комбинирования света
	half4 CombineLights(half4 color, float2 screenUV)
	{
	half4 lightColor = half4(0, 0, 0, 0);
	#if USE_SHAPE_LIGHT_TYPE_0
	lightColor += SAMPLE_TEXTURE2D(_ShapeLightTexture0, sampler_ShapeLightTexture0, screenUV);
	#endif
	#if USE_SHAPE_LIGHT_TYPE_1
	lightColor += SAMPLE_TEXTURE2D(_ShapeLightTexture1, sampler_ShapeLightTexture1, screenUV);
	#endif
	#if USE_SHAPE_LIGHT_TYPE_2
	lightColor += SAMPLE_TEXTURE2D(_ShapeLightTexture2, sampler_ShapeLightTexture2, screenUV);
	#endif
	#if USE_SHAPE_LIGHT_TYPE_3
	lightColor += SAMPLE_TEXTURE2D(_ShapeLightTexture3, sampler_ShapeLightTexture3, screenUV);
	#endif
	color.rgb *= lightColor.rgb + half3(0.1, 0.1, 0.1);
	color.a *= lightColor.a;
	return color;
	}
	half4 frag(Varyings input) : SV_Target
	{
	UNITY_SETUP_INSTANCE_ID(input);
	float4 baseColor = UNITY_ACCESS_INSTANCED_PROP(Props, _BaseColor);
	float4 deepColor = UNITY_ACCESS_INSTANCED_PROP(Props, _DeepColor);
	float4 flowVec = UNITY_ACCESS_INSTANCED_PROP(Props, _FlowVector);
	float noiseScale = UNITY_ACCESS_INSTANCED_PROP(Props, _NoiseScale);
	// 1. Смещение течения по времени
	float2 flowOffset = flowVec.xy * _Time.y;
	// 2. Генерация градиента воды через шум Перлина
	float2 waterUV = input.positionWS.xy * noiseScale + flowOffset;
	float perlin = proceduralPerlin(waterUV);
	perlin = perlin * 0.5 + 0.5; // Переводим из [-1, 1] в [0, 1]
	// Смешиваем светлую и глубокую воду
	half4 waterBase = lerp(baseColor, deepColor, perlin);
	// 3. Генерация белых бликов (пиксельных)
	float highlightDensity = UNITY_ACCESS_INSTANCED_PROP(Props, _HighlightDensity);
	float highlightScale = UNITY_ACCESS_INSTANCED_PROP(Props, _HighlightScale);
	float highlightSpeed = UNITY_ACCESS_INSTANCED_PROP(Props, _HighlightSpeed);
	float4 highlightColor = UNITY_ACCESS_INSTANCED_PROP(Props, _HighlightColor);
	// Используем floor для создания "пиксельного" эффекта бликов
	float2 blinkUV = floor(input.positionWS.xy * highlightScale + flowOffset * highlightScale * 0.5);
	float blinkNoise = random(blinkUV - floor(_Time.y * highlightSpeed));
	// Если рандомное значение попадает в очень узкий порог, рисуем блик
	float isHighlight = step(1.0 - (highlightDensity * 0.1), blinkNoise);
	// Накладываем блики поверх воды
	half4 finalColor = lerp(waterBase, highlightColor, isHighlight);
	// Применяем цвет вершин (для поддержки Sprite Renderer Color)
	finalColor *= input.color;
	// Передаем экранные UV для корректной интеграции света
	finalColor = CombineLights(finalColor, input.screenUV);
	#ifdef _ALPHAPREMULTIPLY_ON
	finalColor.rgb *= finalColor.a;
	#endif
	return finalColor;
	}
	ENDHLSL
	}
	}
		FallBack "Sprites/Default"
		CustomEditor "UnityEditor.Rendering.Universal.ShaderGUI.Lit2DShaderGUI"
}
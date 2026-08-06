Shader "Custom/Simple2DLitInstanced_TreeSway"
{
	Properties
	{
	[MainTexture] _MainTex("Base Texture", 2D) = "white" {}
	[MainColor] _BaseColor("Base Color", Color) = (1,1,1,1)
	_AlphaClip("Alpha Clip", Range(0, 1)) = 0.5
	[Header(Tree Sway Settings)]
	_SwaySpeed("Sway Speed", Float) = 2.0
	_SwayStrength("Sway Strength", Float) = 0.1
	_SwayHeight("Sway Start Height", Float) = 0.0
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
		// Для корректной работы прозрачности
		#pragma multi_compile _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		#include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/LightingUtility.hlsl"
		TEXTURE2D(_MainTex);
		SAMPLER(sampler_MainTex);
			// Явное объявление текстур 2D-освещения URP вместо неработающих макросов
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
			UNITY_DEFINE_INSTANCED_PROP(float4, _MainTex_ST)
			UNITY_DEFINE_INSTANCED_PROP(float, _AlphaClip)
				// Параметры покачивания
				UNITY_DEFINE_INSTANCED_PROP(float, _SwaySpeed)
				UNITY_DEFINE_INSTANCED_PROP(float, _SwayStrength)
				UNITY_DEFINE_INSTANCED_PROP(float, _SwayHeight)
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
				float2 screenUV : TEXCOORD2; // Экранные UV для выборки 2D-освещения
				UNITY_VERTEX_INPUT_INSTANCE_ID
				};
				Varyings vert(Attributes input)
				{
				Varyings output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
					// --- Логика покачивания деревьев ---
					float swaySpeed = UNITY_ACCESS_INSTANCED_PROP(Props, _SwaySpeed);
					float swayStrength = UNITY_ACCESS_INSTANCED_PROP(Props, _SwayStrength);
					float swayHeight = UNITY_ACCESS_INSTANCED_PROP(Props, _SwayHeight);
					// Маска высоты: анимация начнется только выше _SwayHeight по локальной оси Y
					float heightMask = max(0.0, input.positionOS.y - swayHeight);
					// Смещение на основе времени и локальной позиции x
					float swayOffset = sin(_Time.y * swaySpeed + input.positionOS.x + input.positionOS.y) * swayStrength * heightMask;
					input.positionOS.x += swayOffset;
					// -----------------------------------
					VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
					output.positionCS = vertexInput.positionCS;
					output.positionWS = vertexInput.positionWS;
						// Вычисляем экранные координаты для правильного рендера 2D-света в URP
						float4 screenPos = ComputeScreenPos(vertexInput.positionCS);
						output.screenUV = screenPos.xy / screenPos.w;
						float4 baseMap_ST = UNITY_ACCESS_INSTANCED_PROP(Props, _MainTex_ST);
						output.uv = input.uv * baseMap_ST.xy + baseMap_ST.zw;
						output.color = input.color;
						return output;
						}
				// Обновленная функция комбинирования света: используем прямую выборку и screenUV
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
				half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
				half4 baseColor = UNITY_ACCESS_INSTANCED_PROP(Props, _BaseColor);
				half4 finalColor = texColor * baseColor * input.color;
				#ifdef _ALPHATEST_ON
				float alphaClip = UNITY_ACCESS_INSTANCED_PROP(Props, _AlphaClip);
				clip(finalColor.a - alphaClip);
				#endif
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
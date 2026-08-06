Shader "Custom/InstancedWaterFlowLit"
{
	Properties
	{
	[MainTexture] _BaseMap("Water Texture", 2D) = "white" {}
	[MainColor] _BaseColor("Water Color", Color) = (1,1,1,1)
	_AlphaClip("Alpha Clip", Range(0, 1)) = 0.5
		// --- Õ¿—“–Œ… » “≈◊≈Õ»ﬂ ---
		_FlowSpeedX("Flow Speed X", Float) = 0.5
		_FlowSpeedY("Flow Speed Y", Float) = 0.0
		_Distortion("Distortion Intensity", Float) = 0.05
		_WaveScale("Wave Scale", Float) = 10.0
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend", Float) = 0
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
	ZWrite Off
	Cull Off
	HLSLPROGRAM
	#pragma vertex vert
	#pragma fragment frag
	#pragma multi_compile_instancing
		// --- œŒƒƒ≈–∆ ¿ Œ—¬≈Ÿ≈Õ»ﬂ ---
		#pragma multi_compile _ USE_SHAPE_LIGHT_TYPE_0
		#pragma multi_compile _ USE_SHAPE_LIGHT_TYPE_1
		#pragma multi_compile _ USE_SHAPE_LIGHT_TYPE_2
		#pragma multi_compile _ USE_SHAPE_LIGHT_TYPE_3
		#pragma multi_compile _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		#include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/LightingUtility.hlsl"
		struct Attributes
		{
		float3 positionOS : POSITION;
		float2 uv : TEXCOORD0;
		float4 color : COLOR;
		UNITY_VERTEX_INPUT_INSTANCE_ID
		};
		struct Varyings
		{
		float4 positionCS : SV_POSITION;
		float2 uv : TEXCOORD0;
		float3 positionWS : TEXCOORD1;
		float4 color : COLOR;
		UNITY_VERTEX_INPUT_INSTANCE_ID
		};
		TEXTURE2D(_BaseMap);
		SAMPLER(sampler_BaseMap);
		// --- Ã¿ –Œ—€ »—“Œ◊Õ» Œ¬ —¬≈“¿ ---
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
		CBUFFER_START(UnityPerMaterial)
		float _FlowSpeedX;
		float _FlowSpeedY;
		float _Distortion;
		float _WaveScale;
		CBUFFER_END
		UNITY_INSTANCING_BUFFER_START(Props)
		UNITY_DEFINE_INSTANCED_PROP(float4, _BaseMap_ST)
		UNITY_DEFINE_INSTANCED_PROP(half4, _BaseColor)
		UNITY_DEFINE_INSTANCED_PROP(float, _AlphaClip)
		UNITY_INSTANCING_BUFFER_END(Props)
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
		Varyings vert(Attributes input)
		{
		Varyings output = (Varyings)0;
		UNITY_SETUP_INSTANCE_ID(input);
		UNITY_TRANSFER_INSTANCE_ID(input, output);
		float4 baseMap_ST = UNITY_ACCESS_INSTANCED_PROP(Props, _BaseMap_ST);
		output.uv = input.uv * baseMap_ST.xy + baseMap_ST.zw;
		output.color = input.color;
		output.positionWS = TransformObjectToWorld(input.positionOS);
		output.positionCS = TransformObjectToHClip(input.positionOS);
		return output;
		}
		half4 frag(Varyings input) : SV_Target
		{
		UNITY_SETUP_INSTANCE_ID(input);
		// --- ÀŒ√» ¿ “≈◊≈Õ»ﬂ » ¬ŒÀÕ ---
		float wave = sin(input.positionWS.x * _WaveScale + _Time.y) * _Distortion;
		float wave2 = cos(input.positionWS.y * _WaveScale + _Time.x) * _Distortion;
		float2 flowingUV = input.uv;
		flowingUV.x += (_Time.y * _FlowSpeedX) + wave2;
		flowingUV.y += (_Time.y * _FlowSpeedY) + wave;
		half4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, flowingUV);
		half4 baseColor = UNITY_ACCESS_INSTANCED_PROP(Props, _BaseColor);
		half4 finalColor = texColor * baseColor * input.color;
		#ifdef _ALPHATEST_ON
		float alphaClip = UNITY_ACCESS_INSTANCED_PROP(Props, _AlphaClip);
		clip(finalColor.a - alphaClip);
		#endif
		// --- œ–»Ã≈Õ≈Õ»≈ 2D —¬≈“¿ ---
		finalColor = CombineLights(finalColor, input.positionWS.xy);
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
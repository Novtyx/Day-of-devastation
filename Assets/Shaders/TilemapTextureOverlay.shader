Shader "Custom/TilemapTextureOverlay"
{
    Properties
    {
        _MainTex ("Tilemap Texture", 2D) = "white" {}
        _OverlayTex ("Overlay Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.1
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
        }

        Cull Off
        Lighting Off
        ZWrite Off
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
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            sampler2D _OverlayTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            fixed _Cutoff;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Получаем исходный цвет тайла
                fixed4 tileColor = tex2D(_MainTex, i.uv) * i.color;
                
                // Если пиксель почти прозрачный - отбрасываем его
                clip(tileColor.a - _Cutoff);
                
                // Получаем цвет из накладываемой текстуры
                fixed4 overlayColor = tex2D(_OverlayTex, i.uv);
                
                // Комбинируем цвета, сохраняя исходную прозрачность
                fixed4 finalColor;
                finalColor.rgb = overlayColor.rgb;
                finalColor.a = tileColor.a * overlayColor.a;
                
                return finalColor;
            }
            ENDCG
        }
    }
}
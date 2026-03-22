Shader "Custom/PhysicalWaterSurface"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Fill ("Fill Amount", Range(0,1)) = 1
        _FadeColor ("Fade Color", Color) = (0.5,0.5,0.5,0.5)
        [PerRendererData] _HeightTex ("Height Texture", 2D) = "black" {}
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float _Fill;
            fixed4 _FadeColor;
            sampler2D _HeightTex;

            v2f vert (appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag (v2f IN) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
                float2 uv = IN.texcoord;

                float waterLevel = tex2D(_HeightTex, float2(uv.x, 0.5)).r;
                waterLevel = saturate(waterLevel);

                if (uv.y > waterLevel)
                {
                    c.rgb = lerp(c.rgb, _FadeColor.rgb, _FadeColor.a);
                    c.a *= _FadeColor.a;
                }
                return c;
            }
            ENDCG
        }
    }
}
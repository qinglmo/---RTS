Shader "Custom/SurfaceStormFluid"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Fill ("Fill Amount", Range(0,1)) = 1
        _FadeColor ("Fade Color", Color) = (0.5,0.5,0.5,0.5)

        // 宏观倾斜（非常轻微，几乎不用）
        _Tilt ("Tilt", Range(-0.2, 0.2)) = 0
        _TiltStrength ("Tilt Strength", Range(0, 1)) = 0.2

        // 多层Gerstner波参数（表面狂暴）
        _Wave1Amp ("Wave1 Amp", Range(0, 0.3)) = 0.1
        _Wave1Freq ("Wave1 Freq", Range(5, 100)) = 20
        _Wave1Speed ("Wave1 Speed", Range(0, 100)) = 30
        _Wave1Phase ("Wave1 Phase", Range(0, 6.28)) = 0

        _Wave2Amp ("Wave2 Amp", Range(0, 0.3)) = 0.08
        _Wave2Freq ("Wave2 Freq", Range(10, 150)) = 40
        _Wave2Speed ("Wave2 Speed", Range(0, 150)) = 50
        _Wave2Phase ("Wave2 Phase", Range(0, 6.28)) = 1.2

        _Wave3Amp ("Wave3 Amp", Range(0, 0.3)) = 0.06
        _Wave3Freq ("Wave3 Freq", Range(20, 200)) = 80
        _Wave3Speed ("Wave3 Speed", Range(0, 200)) = 80
        _Wave3Phase ("Wave3 Phase", Range(0, 6.28)) = 2.5

        _Wave4Amp ("Wave4 Amp", Range(0, 0.3)) = 0.04
        _Wave4Freq ("Wave4 Freq", Range(30, 300)) = 150
        _Wave4Speed ("Wave4 Speed", Range(0, 300)) = 120
        _Wave4Phase ("Wave4 Phase", Range(0, 6.28)) = 3.8

        // 深度衰减控制：只影响表面一定区域
        _SurfaceLayer ("Surface Layer Thickness", Range(0, 0.5)) = 0.25   // 表面层厚度（占水体的比例）
        _DepthFalloff ("Depth Falloff", Range(1, 10)) = 3                // 衰减曲线陡峭度
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

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
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float _Fill;
            fixed4 _FadeColor;
            float _Tilt;
            float _TiltStrength;

            float _Wave1Amp;
            float _Wave1Freq;
            float _Wave1Speed;
            float _Wave1Phase;
            float _Wave2Amp;
            float _Wave2Freq;
            float _Wave2Speed;
            float _Wave2Phase;
            float _Wave3Amp;
            float _Wave3Freq;
            float _Wave3Speed;
            float _Wave3Phase;
            float _Wave4Amp;
            float _Wave4Freq;
            float _Wave4Speed;
            float _Wave4Phase;

            float _SurfaceLayer;
            float _DepthFalloff;

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

                // 宏观倾斜（很轻微）
                float tiltOffset = _Tilt * (uv.x - 0.5) * 2.0 * _TiltStrength;

                // 时间
                float t = _Time.y;

                // Gerstner波叠加（使用sin产生波峰尖、波谷平的特征）
                float wave = 0;
                wave += _Wave1Amp * sin(uv.x * _Wave1Freq + t * _Wave1Speed + _Wave1Phase);
                wave += _Wave2Amp * sin(uv.x * _Wave2Freq + t * _Wave2Speed + _Wave2Phase);
                wave += _Wave3Amp * sin(uv.x * _Wave3Freq + t * _Wave3Speed + _Wave3Phase);
                wave += _Wave4Amp * sin(uv.x * _Wave4Freq + t * _Wave4Speed + _Wave4Phase);

                // 深度衰减：只影响水面表层（从 _Fill - _SurfaceLayer 到 _Fill 的区域）
                // 计算当前像素相对于基础水面的深度（0 = 水面，正值表示在水面之下）
                float depthBelowSurface = _Fill - uv.y; // 如果uv.y < _Fill，则depth为正（在水下）
                // 我们只允许深度小于 _SurfaceLayer 的区域受波动影响
                float surfaceFactor = 0;
                if (depthBelowSurface > 0)
                {
                    // 越靠近水面，因子越大；在 _SurfaceLayer 深度处因子为0
                    surfaceFactor = saturate(1.0 - pow(depthBelowSurface / _SurfaceLayer, _DepthFalloff));
                }
                // 对于水面以上的点（uv.y > _Fill），depthBelowSurface为负，surfaceFactor为0，不影响

                wave *= surfaceFactor;

                // 最终水位
                float waterLevel = _Fill + tiltOffset + wave;

                // 颜色决定
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
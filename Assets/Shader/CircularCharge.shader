Shader "Custom/CircularCharge"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Progress ("Progress (0-1)", Range(0, 1)) = 0
        _Aspect ("Aspect Ratio (width/height)", Float) = 1
        _Center ("Center (UV)", Vector) = (0.5, 0.5, 0, 0)
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
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
            #pragma target 3.0
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
            float _Progress;
            float _Aspect;
            float4 _Center;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;

                // 把 UV 映射到以圆心为原点的坐标系
                float2 p = IN.texcoord - _Center.xy;
                p.x *= _Aspect; // 宽高比修正，避免非正方形时角度歪斜

                // 当前像素相对“正下方”的夹角
                // 正下方 = 0，正左/正右 = PI/2，正上方 = PI
                float d = atan2(abs(p.x), -p.y);

                float progress = saturate(_Progress);

                // 抗锯齿宽度，给一个最小值，避免端点上仍然出现细线
                float aa = max(fwidth(d) * 1.5, 0.0001);

                // 关键修改：
                // progress = 0 时，gap 稍微小于 0，保证正下方完全可见；
                // progress = 1 时，gap 稍微大于 PI，保证正上方完全透明。
                float gap = lerp(-aa, 3.14159265 + aa, progress);

                // 夹角小于 gap 的像素透明
                float mask = smoothstep(gap - aa, gap + aa, d);

                c.a *= mask;
                return c;
            }
            ENDCG
        }
    }
}
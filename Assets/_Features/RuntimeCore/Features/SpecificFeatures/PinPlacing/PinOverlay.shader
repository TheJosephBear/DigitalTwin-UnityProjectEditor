Shader "Custom/PinOverlay"
{
    // Pin head (circle) or leg (bar) drawn procedurally on a quad, on top of everything.
    // Colors and alpha come from a MaterialPropertyBlock set by PinBillboard.
    Properties
    {
        _FillColor ("Fill", Color) = (0.36, 0.29, 0.85, 1)
        _OutlineColor ("Outline", Color) = (1, 1, 1, 1)
        _GlowColor ("Glow", Color) = (0.36, 0.29, 0.85, 0.6)
        [Toggle] _Circle ("Circle (head) instead of bar (leg)", Float) = 1
        _Padding ("Quad size / shape size (room for the glow)", Float) = 1
        _Outline ("Outline width, fraction of the radius / half width", Range(0, 1)) = 0.16
        _Glow ("Glow strength", Range(0, 1)) = 0
        _Alpha ("Alpha", Range(0, 1)) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent+100" "RenderType"="Transparent" "IgnoreProjector"="True" "PreviewType"="Plane" }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _FillColor, _OutlineColor, _GlowColor;
            float _Circle, _Padding, _Outline, _Glow, _Alpha;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float2 p : TEXCOORD0; };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.p = (v.uv - 0.5) * 2.0 * _Padding; // 1 at the edge of the shape
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Distance from the center, 1 at the edge: radial for the head, across the width for the leg
                float d = _Circle > 0.5 ? length(i.p) : abs(i.p.x);
                float aa = max(fwidth(d), 0.0001);

                float inside = 1.0 - smoothstep(1.0 - aa, 1.0, d);
                float fill = 1.0 - smoothstep(1.0 - _Outline - aa, 1.0 - _Outline, d);
                fixed4 col = lerp(_OutlineColor, _FillColor, fill);
                col.a *= inside;

                // Soft halo around the head
                if (_Glow > 0.0 && _Circle > 0.5) {
                    float t = saturate((d - 1.0) / max(_Padding - 1.0, 0.0001));
                    float glow = _Glow * _GlowColor.a * (1.0 - t) * (1.0 - t);
                    col = lerp(fixed4(_GlowColor.rgb, glow), col, col.a);
                    col.a = max(col.a, glow);
                }

                col.a *= _Alpha;
                return col;
            }
            ENDCG
        }
    }
}

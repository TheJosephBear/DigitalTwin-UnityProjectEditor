Shader "Custom/Pin3D"
{
    // 3D pin: lit normally where visible, semitransparent where geometry is in front of it.
    // Marks its pixels in the stencil so the outline (PinOutline) is only drawn around pins, never over them.
    Properties
    {
        _Color ("Color", Color) = (0.898, 0.22, 0.208, 1)
        _OccludedAlpha ("Occluded Alpha", Range(0, 1)) = 0.35
        _Ambient ("Ambient", Range(0, 1)) = 0.5
    }
    SubShader
    {
        // After all opaque geometry, so the depth the hidden pass compares against is complete
        Tags { "Queue"="Transparent+100" "RenderType"="Transparent" "IgnoreProjector"="True" }

        Stencil
        {
            Ref 64
            WriteMask 64
            Comp Always
            Pass Replace
        }

        Pass
        {
            Name "Occluded"
            Tags { "LightMode"="Always" }
            ZTest Greater
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;
            float _OccludedAlpha;

            float4 vert (float4 vertex : POSITION) : SV_POSITION
            {
                return UnityObjectToClipPos(vertex);
            }

            fixed4 frag () : SV_Target
            {
                return fixed4(_Color.rgb, _Color.a * _OccludedAlpha);
            }
            ENDCG
        }

        Pass
        {
            Name "Visible"
            Tags { "LightMode"="ForwardBase" }
            ZTest LEqual
            ZWrite On

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 normal : TEXCOORD0;
            };

            fixed4 _Color;
            float _Ambient;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Half lambert from the main directional light, so the head still reads as a ball
                float ndl = dot(normalize(i.normal), normalize(_WorldSpaceLightPos0.xyz)) * 0.5 + 0.5;
                float3 light = _Ambient + (1 - _Ambient) * ndl * saturate(_LightColor0.rgb);
                return fixed4(_Color.rgb * light, 1);
            }
            ENDCG
        }
    }
}

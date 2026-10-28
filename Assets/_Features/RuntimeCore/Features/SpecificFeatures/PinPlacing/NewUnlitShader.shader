Shader "Custom/XRaySeeThrough"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Main Color", Color) = (1, 1, 1, 1)
        _XRayColor ("X-Ray / Behind Wall Color", Color) = (0, 0.8, 1, 0.6)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent+100" }

        // ----------------------------------------------------
        // PASS 1: Render object BEHIND walls
        // ----------------------------------------------------
        Pass
        {
            Name "XRayBehind"
            
            // Render only if pixel depth is GREATER than current Z-buffer depth
            ZTest Greater
            ZWrite Off
            
            // Alpha blending for transparency
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            fixed4 _XRayColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Returns the highlight color when hidden
                return _XRayColor;
            }
            ENDCG
        }

        // ----------------------------------------------------
        // PASS 2: Render object NORMALLY when visible
        // ----------------------------------------------------
        Pass
        {
            Name "VisibleNormal"
            
            // Standard depth testing (render if closer or equal depth)
            ZTest LEqual
            ZWrite On
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}

Shader "GeoMapLocalization/GeoPreviewTransparent"
{
    // See-through preview of the base map while it's placed on the geo map.
    // No keywords, so builds always have the variant (enabling _ALPHABLEND_ON on the model's Standard materials only worked in the editor).
    // The depth prepass makes the model fade as one surface, without its inner walls showing through.
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Opacity ("Opacity", Range(0, 1)) = 0.8
        _Ambient ("Ambient", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }

        Pass
        {
            Name "DepthPrepass"
            Tags { "LightMode"="Always" }
            ZWrite On
            ColorMask 0

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 vert (float4 vertex : POSITION) : SV_POSITION
            {
                return UnityObjectToClipPos(vertex);
            }

            fixed4 frag () : SV_Target
            {
                return 0;
            }
            ENDCG
        }

        Pass
        {
            Name "Color"
            Tags { "LightMode"="ForwardBase" }
            ZWrite Off
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 normal : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _Opacity;
            float _Ambient;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed3 albedo = tex2D(_MainTex, i.uv).rgb * _Color.rgb;
                // Half lambert from the main directional light
                float ndl = dot(normalize(i.normal), normalize(_WorldSpaceLightPos0.xyz)) * 0.5 + 0.5;
                float3 light = _Ambient + (1 - _Ambient) * ndl * saturate(_LightColor0.rgb);
                return fixed4(albedo * light, _Opacity);
            }
            ENDCG
        }
    }
}

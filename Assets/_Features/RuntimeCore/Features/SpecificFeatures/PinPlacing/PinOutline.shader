Shader "Custom/PinOutline"
{
    // Inverted hull outline: added as an extra material on the pin while it is being edited.
    // Extruded in screen space so it is the same width in pixels at any pin scale,
    // drawn only outside the pin (stencil from Pin3D) and dimmed where geometry is in front of it.
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (1, 0.57, 1, 1)
        _OutlineWidth ("Outline Width (px)", Range(0, 10)) = 3
        _OccludedAlpha ("Occluded Alpha", Range(0, 1)) = 0.5
    }
    SubShader
    {
        // Right after the pins (Pin3D), so their stencil is written
        Tags { "Queue"="Transparent+101" "RenderType"="Transparent" "IgnoreProjector"="True" }

        Cull Front
        ZWrite Off

        Stencil
        {
            Ref 64
            ReadMask 64
            Comp NotEqual
        }

        CGINCLUDE
        #include "UnityCG.cginc"

        struct appdata
        {
            float4 vertex : POSITION;
            float3 normal : NORMAL;
        };

        fixed4 _OutlineColor;
        float _OutlineWidth;
        float _OccludedAlpha;

        float4 vert (appdata v) : SV_POSITION
        {
            float4 pos = UnityObjectToClipPos(v.vertex);
            float3 viewNormal = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, v.normal));
            float2 offset = TransformViewToProjection(viewNormal.xy);
            float len = length(offset);
            if (len > 0.0001) offset /= len;
            // Pixels to clip space (2 clip units span the screen), times w to undo the perspective divide
            pos.xy += offset * _OutlineWidth * 2.0 / _ScreenParams.xy * pos.w;
            return pos;
        }
        ENDCG

        Pass
        {
            Name "Visible"
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            fixed4 frag () : SV_Target
            {
                return fixed4(_OutlineColor.rgb, 1);
            }
            ENDCG
        }

        Pass
        {
            Name "Occluded"
            ZTest Greater
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            fixed4 frag () : SV_Target
            {
                return fixed4(_OutlineColor.rgb, _OccludedAlpha);
            }
            ENDCG
        }
    }
}

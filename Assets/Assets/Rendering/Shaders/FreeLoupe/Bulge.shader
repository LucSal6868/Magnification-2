Shader "Custom/Bulge"
{
    Properties
    {
        _MainTex ("Webcam Texture", 2D) = "white" {}

        _CirclePosition ("Bulge Position", Vector) = (0.5, 0.5, 0, 0)

        _BulgeSize ("Bulge Size X/Y", Vector) = (0.3, 0.3, 0, 0)

        _Bulge ("Magnification", Range(0, 2)) = 0.5

        _FlatCenter ("Flat Center", Range(0, 0.9)) = 0.45

        _Falloff ("Edge Falloff", Range(0.1, 5)) = 2.0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            sampler2D _MainTex;

            float4 _CirclePosition;
            float4 _BulgeSize;

            float _Bulge;
            float _FlatCenter;
            float _Falloff;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;

                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                // Position relative to bulge center
                float2 offset =
                    i.uv - _CirclePosition.xy;

                // Normalize using ellipse dimensions
                float2 normalizedOffset =
                    offset / _BulgeSize.xy;

                // Distance from center of ellipse
                float radius =
                    length(normalizedOffset);

                // -----------------------------------------
                // Bulge magnification
                // -----------------------------------------

                float transition =
                    saturate(
                        (radius - _FlatCenter) /
                        (1.0 - _FlatCenter)
                    );

                float influence =
                    1.0 - smoothstep(
                        0.0,
                        1.0,
                        transition
                    );

                influence =
                    pow(influence, _Falloff);

                float distortion =
                    1.0 - (_Bulge * influence);

                float2 distortedUV =
                    _CirclePosition.xy +
                    offset * distortion;

                // Sample webcam
                fixed4 color =
                    tex2D(_MainTex, distortedUV);

                // Fully opaque
                color.a = 1.0;

                return color;
            }

            ENDCG
        }
    }
}

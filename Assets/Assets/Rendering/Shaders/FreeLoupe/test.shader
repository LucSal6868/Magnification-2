Shader "Custom/URP_BulgeMagnifier"
{
    Properties
    {
        _CirclePosition ("Bulge Position (mesh UV)", Vector) = (0.5, 0.5, 0, 0)
        _BulgeSize ("Bulge Size X/Y", Vector) = (0.3, 0.3, 0, 0)
        _Bulge ("Magnification", Range(0, 2)) = 0.5
        _FlatCenter ("Flat Center", Range(0, 0.9)) = 0.45
        _Falloff ("Edge Falloff", Range(0.1, 5)) = 2.0
        _EdgeSmooth ("Circle Edge Smoothness", Range(0.001, 0.25)) = 0.03
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "BulgeMagnify"
            Tags { "LightMode" = "UniversalForward" }

            Cull Off
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // URP's grabbed opaque scene color (requires "Opaque Texture" enabled)
            TEXTURE2D(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);

            float4 _CirclePosition;
            float4 _BulgeSize;
            float _Bulge;
            float _FlatCenter;
            float _Falloff;
            float _EdgeSmooth;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos       : SV_POSITION;
                float2 uv        : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                o.screenPos = ComputeScreenPos(o.pos);
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                float2 screenUV = i.screenPos.xy / i.screenPos.w;

                // ---- Same bulge math as your original, in mesh UV space ----
                float2 offset = i.uv - _CirclePosition.xy;
                float2 normalizedOffset = offset / _BulgeSize.xy;
                float distanceFromCenter = length(normalizedOffset);

                // ---- Circular mask: discard everything outside -> no square edges ----
                float circleMask = 1.0 - smoothstep(1.0 - _EdgeSmooth, 1.0, distanceFromCenter);
                clip(circleMask - 0.001);

                float radius = distanceFromCenter;
                float transition = saturate((radius - _FlatCenter) / (1.0 - _FlatCenter));
                float influence = 1.0 - smoothstep(0.0, 1.0, transition);
                influence = pow(influence, _Falloff);

                float distortion = 1.0 - (_Bulge * influence);

                // Vector we want to apply, in mesh-UV space
                float2 uvSpaceDelta = offset * (distortion - 1.0);

                // ---- Convert that UV-space delta into screen-space using local derivatives ----
                // This maps "how much mesh UV moves" to "how much screen UV moves" locally,
                // which works correctly for a camera-facing quad at any distance/scale.
                float2 scale = fwidth(screenUV) / max(fwidth(i.uv), 1e-6);
                float2 screenSpaceDelta = uvSpaceDelta * scale;

                float2 distortedScreenUV = screenUV + screenSpaceDelta;

                half3 sceneColor = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, distortedScreenUV).rgb;

                return half4(sceneColor, circleMask);
            }
            ENDHLSL
        }
    }
}
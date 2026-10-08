// SceneSpotlight's dim: a full-screen triangle blended over the picture, darkening it by
// the global _CkgSpotlightDim toward _Tint, except where the stencil holds _StencilRef
// (the spotlit renderers, marked by SpotlightMask). Drawn by SpotlightDimFeature.
Shader "Hidden/CupkekGames/SpotlightDim"
{
    Properties
    {
        _Tint ("Tint", Color) = (0, 0, 0, 1)
        _StencilRef ("Stencil Reference", Range(1, 255)) = 64
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Transparent" }

        Pass
        {
            Name "SpotlightDim"
            ZTest Always
            ZWrite Off
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

            Stencil
            {
                Ref [_StencilRef]
                ReadMask [_StencilRef]
                Comp NotEqual
            }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

            float _CkgSpotlightDim;

            CBUFFER_START(UnityPerMaterial)
                half4 _Tint;
                float _StencilRef;
            CBUFFER_END

            struct Attributes
            {
                uint vertexID : SV_VertexID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                return half4(_Tint.rgb, saturate(_CkgSpotlightDim));
            }
            ENDHLSL
        }
    }
}

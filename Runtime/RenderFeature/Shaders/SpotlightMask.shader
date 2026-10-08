// SceneSpotlight's mask: the override material of the RenderObjects passes that draw the
// spotlit layer once more, writing nothing but _StencilRef into the stencil where they are
// seen (depth-tested against the picture, so whatever stands in front still dims).
Shader "Hidden/CupkekGames/SpotlightMask"
{
    Properties
    {
        _StencilRef ("Stencil Reference", Range(1, 255)) = 64
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "SpotlightMask"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            ZTest LEqual
            ZWrite Off
            ColorMask 0

            Stencil
            {
                Ref [_StencilRef]
                WriteMask [_StencilRef]
                Comp Always
                Pass Replace
            }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _StencilRef;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }
}

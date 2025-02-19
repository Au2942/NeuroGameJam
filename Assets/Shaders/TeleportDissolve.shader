Shader "CustomEffects/Teleport_Dissolve_Shader"
{

    Properties
    {
        [MainTexture] [NoScaleOffset] _MainTex ("Base Map", 2D) = "white" {}
        _Seed ("Seed", Integer) = 0
        _Progress ("Progress", Range(0, 1)) = 0
        [HDR] _DissolveColor ("Color", color) = (0,2,1.7,1)
        _DissolveSize ("Size", Integer) = 25
        _DissolveFade ("Fade", float) = 0.1
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8.000000
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0.000000
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0.000000
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255.000000
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255.000000
        [HideInInspector] _ColorMask ("ColorMask", Float) = 15.000000
        [HideInInspector] _ClipRect ("ClipRect", Vector) = (0.000000,0.000000,0.000000,0.000000)
        [HideInInspector] _UIMaskSoftnessX ("UIMaskSoftnessX", Float) = 1.000000
        [HideInInspector] _UIMaskSoftnessY ("UIMaskSoftnessY", Float) = 1.000000
        [HideInInspector] [NoScaleOffset]  unity_Lightmaps ("unity_Lightmaps", 2DArray) = "" { }
        [HideInInspector] [NoScaleOffset]  unity_LightmapsInd ("unity_LightmapsInd", 2DArray) = "" { }
        [HideInInspector] [NoScaleOffset]  unity_ShadowMasks ("unity_ShadowMasks", 2DArray) = "" { }
    }
    SubShader
    {		
        Tags{ 
		"RenderType"="Transparent" 
		"Queue"="Transparent"
		"RenderPipeline" = "UniversalPipeline"
        "PreviewType"="Plane"
        "IGNOREPROJECTOR"="true" 
        "CanUseSpriteAtlas"="true"
	}     
        
        Pass
        {
            ZWrite Off
            Cull Off
            Stencil {
                Ref [_Stencil]
                ReadMask [_StencilReadMask]
                WriteMask [_StencilWriteMask]
                CompFront [_StencilComp]
                PassFront [_StencilOp]
                CompBack [_StencilComp]
                PassBack [_StencilOp]
            }
            Blend One OneMinusSrcAlpha
            ColorMask [_ColorMask]
            ZTest [unity_GUIZTestMode]

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

            struct Attributes
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
            CBUFFER_END
            
            uint _Seed;
            float _Progress;
            float4 _DissolveColor;
            int _DissolveSize;
            float _DissolveFade;
            
            
            Varyings vert(Attributes IN)
            {
                Varyings Output;
                Output.vertex = TransformObjectToHClip(IN.vertex.xyz);
                Output.uv = TRANSFORM_TEX(IN.texcoord, _MainTex);
                return Output;
            }
            
            
            float4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                uv.y += _Seed;
                uint index = (uint)(floor(uv.x * _DissolveSize) + floor(uv.y * _DissolveSize) * _DissolveSize);
                float rand = GenerateHashedRandomFloat(index);
                rand = lerp(0, 1-_DissolveFade, rand);
                float4 sampledColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                float alpha = smoothstep( _DissolveFade, 0, _Progress - rand);
                float t = smoothstep(0, _DissolveFade, _Progress);
                float3 rgb = lerp(sampledColor.rgb, _DissolveColor, t);
                return float4(rgb * alpha, sampledColor.a)* sampledColor.a * alpha;
            }
            ENDHLSL
        }
    }
}
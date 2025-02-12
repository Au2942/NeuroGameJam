//modified from glitch effect in Kino by Keijiro Takahashi https://github.com/keijiro/Kino

Shader "CustomEffects/Color_Stripes_Shader"
{

    Properties
    {
        [HideInInspector] _MainTex ("Base Map", 2D) = "white" {}
        _ColorA ("ColorA", Color) = (0,1,0,1)
        _ColorB ("ColorB", Color) = (1,0,0,1)
        _Frequency ("Frequency", Integer) = 100
        _Bitmask1 ("Bitmask1", Integer) = 0
        _Bitmask2 ("Bitmask2", Integer) = 0
        _Bitmask3 ("Bitmask3", Integer) = 0
        _Bitmask4 ("Bitmask4", Integer) = 0
        [Toggle] _Horizontal("Horizontal?", Float) = 0
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8.000000
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0.000000
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0.000000
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255.000000
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255.000000
        [HideInInspector] _ColorMask ("ColorMask", Float) = 15.000000
        [HideInInspector] _ClipRect ("ClipRect", Vector) = (0.000000,0.000000,0.000000,0.000000)
        [HideInInspector] _UIMaskSoftnessX ("UIMaskSoftnessX", Float) = 1.000000
        [HideInInspector] _UIMaskSoftnessY ("UIMaskSoftnessY", Float) = 1.000000
    }
    SubShader
    {		
        Tags{ 
		"RenderType"="Transparent" 
		"Queue"="Transparent"
		"RenderPipeline" = "UniversalPipeline"
        "PreviewType"="Plane"
	}
        //Blend SrcAlpha OneMinusSrcAlpha       
        
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


            Varyings vert(Attributes IN)
            {
                Varyings Output;
                Output.vertex = TransformObjectToHClip(IN.vertex.xyz);
                Output.uv = IN.texcoord;
                return Output;
            }
            
            float4 _ColorA;
            float4 _ColorB;
            float _Frequency;
            float _Horizontal;
            int _Bitmask1;
            int _Bitmask2;
            int _Bitmask3;
            int _Bitmask4;
            
            float4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;

                if(_Horizontal==1) {
                    uv = uv.yx;
                }
                
                // Get the chunk index
                uint chunkIndex = floor(uv.x * _Frequency);
                int bitIndex = chunkIndex % 32; 
                uint groupIndex = chunkIndex / 32;
                uint mask = 1 << bitIndex; 

                uint isToggled = (groupIndex == 0) ? (_Bitmask1 & mask) :
                                (groupIndex == 1) ? (_Bitmask2 & mask) :
                                (groupIndex == 2) ? (_Bitmask3 & mask) :
                                                    (_Bitmask4 & mask);

                // Select the correct color
                return (isToggled != 0) ? _ColorB : _ColorA;
            }
            ENDHLSL
        }
    }
}
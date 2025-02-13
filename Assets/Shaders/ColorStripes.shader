//modified from glitch effect in Kino by Keijiro Takahashi https://github.com/keijiro/Kino

Shader "CustomEffects/Color_Stripes_Shader"
{

    Properties
    {
        [HideInInspector] _MainTex ("Base Map", 2D) = "white" {}
        _ColorA ("ColorA", Color) = (0,1,0,1)
        _ColorB ("ColorB", Color) = (1,0,0,1)
        _ColorC ("ColorB", Color) = (0,0,1,1)
        _ColorD ("ColorB", Color) = (1,1,1,1)
        _Frequency ("Frequency", Integer) = 100
        _BitmaskA1 ("BitmaskA1", Integer) = 0
        _BitmaskA2 ("BitmaskA2", Integer) = 0
        _BitmaskA3 ("BitmaskA3", Integer) = 0
        _BitmaskA4 ("BitmaskA4", Integer) = 0
        _BitmaskB1 ("BitmaskB1", Integer) = 0
        _BitmaskB2 ("BitmaskB2", Integer) = 0
        _BitmaskB3 ("BitmaskB3", Integer) = 0
        _BitmaskB4 ("BitmaskB4", Integer) = 0
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
            
            float4 _ColorA; //00
            float4 _ColorB; //01
            float4 _ColorC; //10
            float4 _ColorD; //11
            float _Frequency;
            float _Horizontal;
            int _BitmaskA1, _BitmaskA2, _BitmaskA3, _BitmaskA4; // LSB
            int _BitmaskB1, _BitmaskB2, _BitmaskB3, _BitmaskB4; // MSB
            
            
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

                // Extract LSB (bitmaskA) and MSB (bitmaskB)
                uint bitA = (groupIndex == 0) ? (_BitmaskA1 & mask) :
                            (groupIndex == 1) ? (_BitmaskA2 & mask) :
                            (groupIndex == 2) ? (_BitmaskA3 & mask) :
                                                (_BitmaskA4 & mask);

                uint bitB = (groupIndex == 0) ? (_BitmaskB1 & mask) :
                            (groupIndex == 1) ? (_BitmaskB2 & mask) :
                            (groupIndex == 2) ? (_BitmaskB3 & mask) :
                                                (_BitmaskB4 & mask);

                uint colorIndex = (bitA != 0) + 2 * (bitB != 0); // Converts bits to an index: 00=0, 01=1, 10=2, 11=3

                // Select the correct color
                return (colorIndex == 1) ? _ColorB :  // 01
                    (colorIndex == 2) ? _ColorC : // 10
                    (colorIndex == 3) ? _ColorD : // 11
                    _ColorA; // Default 00
            }
            ENDHLSL
        }
    }
}
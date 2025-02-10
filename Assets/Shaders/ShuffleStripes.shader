//modified from glitch effect in Kino by Keijiro Takahashi https://github.com/keijiro/Kino

Shader "CustomEffects/Shuffle_Stripes_Shader"
{

    Properties
    {
        _Frequency ("Frequency", float) = 20
        _Fill ("Fill", Range(0, 1)) = 0.5
        _Seed ("Seed", Float) = 0
        _Color1 ("Color 1", Color) = (0,1,0,1)
        _Color2 ("Color 2", Color) = (1,0,0,1)
        [Toggle] _Horizontal("Horizontal?", Float) = 0
        [HideInInspector]  _StencilComp ("Stencil Comparison", Float) = 8.000000
        [HideInInspector]  _Stencil ("Stencil ID", Float) = 0.000000
        [HideInInspector]  _StencilOp ("Stencil Operation", Float) = 0.000000
        [HideInInspector]  _StencilWriteMask ("Stencil Write Mask", Float) = 255.000000
        [HideInInspector]  _StencilReadMask ("Stencil Read Mask", Float) = 255.000000
        [HideInInspector]  _ColorMask ("ColorMask", Float) = 15.000000
        [HideInInspector]  _ClipRect ("ClipRect", Vector) = (0.000000,0.000000,0.000000,0.000000)
        [HideInInspector]  _UIMaskSoftnessX ("UIMaskSoftnessX", Float) = 1.000000
        [HideInInspector]  _UIMaskSoftnessY ("UIMaskSoftnessY", Float) = 1.000000
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

            float _Frequency;
            float _Fill;
            float _Seed;
            float4 _Color1;
            float4 _Color2;
            float _Horizontal;


            float FRandom(uint seed)
            {
                return GenerateHashedRandomFloat(seed);
            }

            float shuffle_float(float x,float div,float i,float seed){
                float original = x;
                i += seed;
                float from = float(floor(FRandom((i)*.22)*div));
                float to = float(floor(FRandom(i*.82)*div));
                if(original==from){x=to;}
                if(original==to){x=from;}
                return x;
            }
            
            float cell(float x, float div, float seed)
            {
                float a = floor(x*div);
                for(float i=0.;i<70.;i++){
                    a = shuffle_float(a,div,i,seed);
                }
                return a;
            }
            

            float4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                if(_Horizontal==1) {
                    uv = uv.yx;
                }

                float a;
                a = cell(uv.x,_Frequency,1.);
                a = (a+frac(uv.x*_Frequency))/_Frequency;
                float4 color = lerp(_Color1,_Color2, step(a,_Fill));

                return color;
            }
            ENDHLSL
        }
    }
}
//modified from glitch effect in Kino by Keijiro Takahashi https://github.com/keijiro/Kino

Shader "CustomEffects/Glitch_Effect_Shader"
{

    Properties
    {
        _MainTex ("Base Map", 2D) = "white" {}
        _BlockStrength ("Block Strength", float) = 0
        _BlockSize ("Block Size", Integer) = 32
        _BlockStride ("Block Stride", Integer) = 1
        _BlockSeed1 ("Block Seed 1", Integer) = 71
        _BlockSeed2 ("Block Seed 2", Integer) = 113
        _Drift ("Drift", Vector) = (0, 0, 0, 0)
        _Jitter ("Jitter", Vector) = (0, 0, 0 ,0)
        _Jump ("Jump", Vector) = (0, 0, 0, 0)
        _Shake ("Shake", float) = 0
        
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 3
        [HideInInspector] _Stencil ("Stencil ID", Float) = 1
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 2
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 0
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 0

        [HideInInspector] _ColorMask ("Color Mask", Float) = 15

        [HideInInspector] [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
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
            Stencil
            {
                Ref [_Stencil]
                Comp [_StencilComp]
                Pass [_StencilOp]
                ReadMask [_StencilReadMask]
                WriteMask [_StencilWriteMask]
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
		        float4 screenPos : TEXCOORD1;
            };


            Varyings vert(Attributes IN)
            {
                // Declaring the output object (OUT) with the Varyings struct.
                Varyings Output;
                // The TransformObjectToHClip function transforms vertex positions
                // from object space to homogenous clip space.
                Output.vertex = TransformObjectToHClip(IN.vertex.xyz);
                // Returning the output.        
                Output.uv = IN.texcoord;
		        Output.screenPos = ComputeScreenPos(Output.vertex);
                return Output;
            }

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            uint _Seed;
            uint _BlockSize;
            float _BlockStrength;
            uint _BlockStride;
            uint _BlockSeed1;
            uint _BlockSeed2;

            float2 _Drift;
            float2 _Jitter;
            float2 _Jump;
            float _Shake;



            float FRandom(uint seed)
            {
                return GenerateHashedRandomFloat(seed);
            }

            float wrap(float value, float x, float y) {
                return x + frac((value - x) / (y - x)) * (y - x);
            }

            float4 frag(Varyings input) : SV_Target
            {

                float2 uv = input.uv;

                //
                // Block glitch
                //
                uint block_size = _BlockSize;
                uint columns = _ScreenSize.x / block_size;

                // Block index
                uint2 block_xy = uv * _ScreenSize.xy  / block_size;
                uint block = block_xy.y * columns + block_xy.x;

                uint2 seedBlock_xy = input.uv * _ScreenSize.xy  / block_size;
                uint seedBlock = seedBlock_xy.y * columns + seedBlock_xy.x;
                
                // Segment index
                uint segment = seedBlock / _BlockStride;

                // Per-block random number
                float r1 = FRandom(seedBlock     + _BlockSeed1);
                float r3 = FRandom(seedBlock / 3 + _BlockSeed2);
                uint seed = (r1 + r3) < 1 ? _BlockSeed1 : _BlockSeed2;
                float rand = FRandom(segment + seed);

                // Block damage (offsetting)
                block += rand * 20000 * (rand < _BlockStrength);

                // Screen space position reconstruction
                uint2 ssp = uint2(block % columns, block / columns) * block_size;
                ssp += (uint2)(uv * _ScreenSize.xy) % block_size;

                // UV recalculation
                uv = frac((ssp + 0.5) / (_ScreenSize.xy));
                
                // Texture position
                float tx = uv.x;
                float ty = uv.y;

                // Jump
                ty = lerp(ty, frac(ty + _Jump.x), _Jump.y);

                // Screen space Y coordinate
                uint sy = ty * _ScreenSize.y;

                // Jitter

                float jitter = Hash(sy + _Seed) * 2 - 1;
                float jx = jitter * (_Jitter.x < abs(jitter)) * _Jitter.y;
                tx += jx;

                // Shake
                tx = frac(tx + (Hash(_Seed) - 0.5) * _Shake);

                // Drift
                float drift = sin(ty * 2 + _Drift.x) * _Drift.y;

                // Source sample
                uint sx1 = (tx) * _ScreenSize.x;
                uint sx2 = (tx + drift) * _ScreenSize.x;
                float2 suv1 = uint2(sx1, sy) / (_ScreenSize.xy);
                float2 suv2 = uint2(sx2, sy) / (_ScreenSize.xy);

                

                float4 c1 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex , suv1);// + uint2(sx1, sy));
                float4 c2 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex , suv2);// + uint2(sx2, sy));
                float4 c = float4(c1.r, c2.g, c1.b, c1.a);

                
                // Block damage (color mixing)
                if (frac(rand * 1234) < _BlockStrength * 0.1)
                {
                    float3 hsv = RgbToHsv(c.rgb);
                    hsv = hsv * float3(-1, 1, 0) + float3(0.5, 0, 0.9);
                    c.rgb = HsvToRgb(hsv);
                    c.rgb *= step(_BlockStrength , frac(rand*5678));
                }
                
                
                

                return c;
            }
            ENDHLSL
        }
    }
}
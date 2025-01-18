//modified from keijiro's kino glitch shader https://github.com/keijiro/Kino
Shader "Unlit/Glitch_Block_Shader"
{

    Properties
    {
        _MainTex ("Base Map", 2D) = "white" {}
        _BlockStrength ("Block Strength", float) = 0.0001
        _BlockSize ("Block Size", Range(1, 100)) = 32
        _BlockStride ("Block Stride", Range(1, 32)) = 1
        _BlockSeed1 ("Block Seed 1", float) = 71
        _BlockSeed2 ("Block Seed 2", float) = 113
        _Drift ("Drift", Vector) = (0.1, 0.1, 0, 0)
        _Jitter ("Jitter", Vector) = (0.1, 0.1, 0 ,0)
        _Jump ("Jump", Vector) = (0.1, 0.1, 0, 0)
        _Shake ("Shake", float) = 0.1
        _MeshBound ("Mesh Bound", Vector) = (0, 0, 1920, 1080)
        _ScreenBounds ("Screen Bounds", Vector) = (0, 0, 1920, 1080)
    }
    SubShader
    {		
        Tags{ 
		"RenderType"="Transparent" 
		"Queue"="Transparent"
		"RenderPipeline" = "UniversalPipeline"
	}
        Blend SrcAlpha OneMinusSrcAlpha       
        
        Pass
        {
            ZWrite Off
            Cull Off

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
		        float4 screenPos : TEXCOORD3;
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

            TEXTURE2D(_CameraSortingLayerTexture);
            SAMPLER(sampler_CameraSortingLayerTexture);

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

            float4 _MeshBound;
            float4 _ScreenBounds;



            float FRandom(uint seed)
            {
                return GenerateHashedRandomFloat(seed);
            }

            float4 frag(Varyings input) : SV_Target
            {

                float2 _MeshSize = float2(_MeshBound.z-_MeshBound.x, _MeshBound.w-_MeshBound.y);
                float2 screenUV = input.screenPos.xy / input.screenPos.w;
                float2 screenSize = float2(_ScreenBounds.z-_ScreenBounds.x, _ScreenBounds.w-_ScreenBounds.y);
                float2 uv;

                //
                // Block glitch
                //
                uint block_size = 32;
                uint columns = _MeshSize.x / block_size;

                // Block index
                uint2 block_xy = screenUV * _MeshSize.xy / block_size;
                uint block = block_xy.y * columns + block_xy.x;

                // Segment index
                uint segment = block / _BlockStride;

                // Per-block random number
                float r1 = FRandom(block     + _BlockSeed1);
                float r3 = FRandom(block / 3 + _BlockSeed2);
                uint seed = (r1 + r3) < 1 ? _BlockSeed1 : _BlockSeed2;
                float rand = FRandom(segment + seed);

                // Block damage (offsetting)
                block += rand * 20000 * (rand < _BlockStrength);

                // Screen space position reconstruction
                uint2 ssp = uint2(block % columns, block / columns) * block_size;
                ssp += (uint2)(screenUV * _MeshSize.xy) % block_size;

                // UV recalculation
                uv = frac((ssp + 0.5) / _MeshSize.xy);

                // Clamp the UV to the bounds of the mesh in screen space
                uv.x = (uv.x < _MeshBound.x / screenSize.x || uv.x > _MeshBound.z / screenSize.x) ? screenUV.x : uv.x;
                uv.y = (uv.y < _MeshBound.y / screenSize.y || uv.y > _MeshBound.w / screenSize.y) ? screenUV.y : uv.y;
                
                // Texture position
                float tx = uv.x;
                float ty = uv.y;

                // Jump
                ty = lerp(ty, frac(ty + _Jump.x), _Jump.y);

                // Screen space Y coordinate
                uint sy = ty * _MeshSize.y;

                // Jitter
                float jitter = Hash(sy + _Seed) * 2 - 1;
                tx += jitter * (_Jitter.x < abs(jitter)) * _Jitter.y;

                // Shake
                tx = frac(tx + (Hash(_Seed) - 0.5) * sin(_Shake*_Time.y) * _Shake);

                // Drift
                float drift = sin(ty * 2 + _Drift.x) * _Drift.y;

                // Source sample
                uint sx1 = (tx        ) * _MeshSize.x;
                uint sx2 = (tx + drift) * _MeshSize.x;
                float4 c1 = SAMPLE_TEXTURE2D(_CameraSortingLayerTexture, sampler_CameraSortingLayerTexture , uint2(sx1, sy)/_MeshSize);// + uint2(sx1, sy));
                float4 c2 = SAMPLE_TEXTURE2D(_CameraSortingLayerTexture, sampler_CameraSortingLayerTexture , uint2(sx2, sy)/_MeshSize);// + uint2(sx2, sy));
                float4 c = float4(c1.r, c2.g, c1.b, c1.a);


                //float4 outTexture = SAMPLE_TEXTURE2D(_CameraSortingLayerTexture, sampler_CameraSortingLayerTexture, input.screenPos + blockOffset);
                
                // Block damage (color mixing)
                if (frac(rand * 1234) < _BlockStrength * 0.1)
                {
                    float3 hsv = RgbToHsv(c.rgb);
                    hsv = hsv * float3(-1, 1, 0) + float3(0.5, 0, 0.9);
                    c.rgb = HsvToRgb(hsv);
                }

                return c;
            }
            ENDHLSL
        }
    }
}
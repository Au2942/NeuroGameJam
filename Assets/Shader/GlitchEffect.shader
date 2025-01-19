Shader "Unlit/Glitch_Effect_Shader"
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
        _NoiseStrength ("Noise Strength", Range(0, 1)) = 1
        _NoiseAmount ("Noise Amount", Range(0, 100)) = 50
        _NoiseIntensity ("Noise Intensity", Range(0, 1)) = 0.1
        _ScanlineStrength ("Scanline Strength", Range(0, 1)) = 1
        _ScanlineAmount ("Scanline Amount", Range(0, 1000)) = 600
        _MeshBound ("Mesh Bound", Vector) = (0, 0, 1920, 1080)
        _TargetScreenBounds ("Screen Bounds", Vector) = (0, 0, 1920, 1080)
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

            float _NoiseStrength;
            float _NoiseAmount;
            float _NoiseIntensity;
            float _ScanlineStrength;
            float _ScanlineAmount;

            float4 _MeshBound;
            float4 _TargetScreenBounds;

            float2 unity_gradientNoise_dir(float2 p)
            {
                p = p % 289;
                float x = (34 * p.x + 1) * p.x % 289 + p.y;
                x = (34 * x + 1) * x % 289;
                x = frac(x / 41) * 2 - 1;
                return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
            }
            
            float unity_gradientNoise(float2 p)
            {
                float2 ip = floor(p);
                float2 fp = frac(p);
                float d00 = dot(unity_gradientNoise_dir(ip), fp);
                float d01 = dot(unity_gradientNoise_dir(ip + float2(0, 1)), fp - float2(0, 1));
                float d10 = dot(unity_gradientNoise_dir(ip + float2(1, 0)), fp - float2(1, 0));
                float d11 = dot(unity_gradientNoise_dir(ip + float2(1, 1)), fp - float2(1, 1));
                fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
                return lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x);
            }
            
            void Unity_GradientNoise_float(float2 UV, float Scale, out float Out)
            {
                Out = unity_gradientNoise(UV * Scale) + 0.5;
            }

            void Unity_Remap_float(float In, float2 InMinMax, float2 OutMinMax, out float Out)
            {
                Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
            }

            float FRandom(uint seed)
            {
                return GenerateHashedRandomFloat(seed);
            }

            float wrap(float value, float x, float y) {
                return x + frac((value - x) / (y - x)) * (y - x);
            }

            float4 frag(Varyings input) : SV_Target
            {

                float2 meshSize = float2(_MeshBound.z-_MeshBound.x, _MeshBound.w-_MeshBound.y);
                float2 screenUV = input.screenPos.xy / input.screenPos.w;
                float2 targetScreenSize = float2(_TargetScreenBounds.z-_TargetScreenBounds.x, _TargetScreenBounds.w-_TargetScreenBounds.y);
                float3 cameraPos = _WorldSpaceCameraPos;
                float2 cameraBottomLeft = float2(cameraPos.x, cameraPos.y );
                float4 targetUVBound = float4((_MeshBound.x - cameraBottomLeft.x)/(_ScreenSize.x), (_MeshBound.y - cameraBottomLeft.y)/(_ScreenSize.y), (_MeshBound.z - cameraBottomLeft.x)/(_ScreenSize.x), (_MeshBound.w - cameraBottomLeft.y)/(_ScreenSize.y));
                float2 uv;

                //
                // Block glitch
                //
                uint block_size = _BlockSize;
                uint columns = targetScreenSize.x / block_size;

                // Block index
                uint2 block_xy = screenUV * targetScreenSize.xy  / block_size;
                uint block = block_xy.y * columns + block_xy.x;

                uint2 seedBlock_xy = input.uv * targetScreenSize.xy  / block_size;
                uint2 seedBlock = seedBlock_xy.y * columns + seedBlock_xy.x;
                
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
                ssp += (uint2)(screenUV * targetScreenSize.xy) % block_size;

                // UV recalculation
                uv = frac((ssp + 0.5) / (targetScreenSize.xy));
                
                // Texture position
                float tx = uv.x;
                float ty = uv.y;

                // Jump
                ty = lerp(ty, frac(ty + _Jump.x), _Jump.y);

                // Screen space Y coordinate
                uint sy = ty * targetScreenSize.y;

                // Jitter
                // float jitter = Hash(sy + _Seed) * 2 - 1;
                // tx += jitter * (_Jitter.x < abs(jitter)) * _Jitter.y;
                float jitter = Hash(sy + _Seed) * 2 - 1;
                float jx = jitter * (_Jitter.x < abs(jitter)) * _Jitter.y;
                //tx = (tx + jx < _MeshBound.x / screenSize.x || tx + jx > _MeshBound.z / screenSize.x) ? tx : tx + jx;
                tx += jx;

                // Shake
                tx = frac(tx + (Hash(_Seed) - 0.5) * sin(_Shake*_Time.y) * _Shake);

                float shx = frac(tx + (Hash(_Seed) - 0.5) * sin(_Time.y) * _Shake);
                //tx = (shx < _MeshBound.x / screenSize.x || shx > _MeshBound.z / screenSize.x) ? tx : shx;
                // Drift
                float drift = sin(ty * 2 + _Drift.x) * _Drift.y;

                // Flicker
                float strength = _Time.y * _NoiseStrength;
                float stripe = (input.uv.y + strength);
                float2 noiseUV = float2(stripe, stripe);
                float gradientNoise;
                Unity_GradientNoise_float(noiseUV, _NoiseAmount, gradientNoise) ;
                float noise1;
                Unity_Remap_float(gradientNoise, float2(0, 1), float2(-_NoiseIntensity, _NoiseIntensity), noise1);

                float noise2;
                Unity_GradientNoise_float(strength, 10, noise2);
                noise2 *= noise2;
                noise2 *= noise2;
                noise2 *= 0.1;

                float finalNoise = (_NoiseStrength > 0 ) ? noise1 * noise2 : 0;

                // Source sample
                uint sx1 = (tx) * targetScreenSize.x;
                uint sx2 = (tx + drift) * targetScreenSize.x;
                float2 suv1 = uint2(sx1, sy) / (targetScreenSize.xy);
                float2 suv2 = uint2(sx2, sy) / (targetScreenSize.xy);

                suv1.x = wrap(suv1.x + finalNoise, targetUVBound.x, targetUVBound.z);
                suv2.x = wrap(suv2.x + finalNoise, targetUVBound.x, targetUVBound.z);


                float4 c1 = SAMPLE_TEXTURE2D(_CameraSortingLayerTexture, sampler_CameraSortingLayerTexture , suv1);// + uint2(sx1, sy));
                float4 c2 = SAMPLE_TEXTURE2D(_CameraSortingLayerTexture, sampler_CameraSortingLayerTexture , suv2);// + uint2(sx2, sy));
                float4 c = float4(c1.r, c2.g, c1.b, c1.a);

                //scanline
                float scanline = clamp(sin((input.uv.y*_ScanlineAmount + strength)), 1-_ScanlineStrength ,1);
                float remappedScanline;
                Unity_Remap_float(scanline, float2(-1, 1), float2(0.2, 1), remappedScanline);

                
                // Block damage (color mixing)
                if (frac(rand * 1234) < _BlockStrength * 0.1)
                {
                    float3 hsv = RgbToHsv(c.rgb);
                    hsv = hsv * float3(-1, 1, 0) + float3(0.5, 0, 0.9);
                    c.rgb = HsvToRgb(hsv);
                }
                c.rgb *= remappedScanline;

                

                return c;
            }
            ENDHLSL
        }
    }
}
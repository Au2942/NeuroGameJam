Shader "Unlit/Glitch_Block_Shader"
{

    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NoiseStrength ("Noise Strength", Range(0, 1)) = 0.5
        _NoiseAmount ("Noise Amount", Range(0, 100)) = 50
        _NoiseIntensity ("Noise Intensity", Range(0, 10)) = 1
        _ScanlineStrength ("Scanline Strength", Range(0, 1)) = 1
        _ScanlineAmount ("Scanline Amount", Range(0, 1000)) = 600
        _Padding ("Padding", Range(0, 1)) = 0.05
        _BlockSize ("Block Size", Range(1, 100)) = 32
        _BlockSeed1 ("Block Seed 1", float) = 71
        _BlockSeed2 ("Block Seed 2", float) = 113
        _BlockStride ("Block Stride", Range(1, 32)) = 1
        _BlockStrength ("Block Strength", Range(0, 1)) = 0.5
    }
    SubShader
    {		
        Tags{ 
		"RenderType"="Transparent" 
		"Queue"="Transparent"
		"RenderPipeline" = "UniversalPipeline"
	}
        Blend SrcAlpha OneMinusSrcAlpha       
        
	ZWrite off
	Cull off
      
        LOD 100 
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert alpha
            #pragma fragment frag alpha

            #include "UnityCG.cginc"


            struct appdata
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct Interpolation
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
		        float4 screenPos : TEXCOORD3;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _NoiseStrength;
            float _NoiseAmount;
            float _NoiseIntensity;
            float _ScanlineStrength;
            float _ScanlineAmount;
            float _Padding;
            float _BlockSize;
            float _BlockSeed1;
            float _BlockSeed2;
            float _BlockStride;
            float _BlockStrength;

	        sampler2D _CameraSortingLayerTexture;

    
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

            void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
            {
                Out = UV * Tiling + Offset;
            }



            float Hash(float value)
            {
                // A simple hashing function
                return frac(sin(value * 1.5) * 43758.5453);
            }

            float GenerateHashedRandomFloat(float input)
            {
                // Create a unique hash based on the input
                float hashedValue = Hash(input);
                
                // You can scale and offset this value if needed, for example:
                return hashedValue * 2.0 - 1.0; // Scale to range [-1, 1]
            }

            float FRandom(uint seed)
            {
                return GenerateHashedRandomFloat(seed);
            }

            // Converts RGB to HSV
            float3 RGBToHSV(float3 rgb)
            {
                float3 hsv;
                float maxVal = max(rgb.r, max(rgb.g, rgb.b));
                float minVal = min(rgb.r, min(rgb.g, rgb.b));
                float delta = maxVal - minVal;

                // Hue calculation
                if (delta == 0)
                    hsv.x = 0; // Hue is undefined
                else if (maxVal == rgb.r)
                    hsv.x = fmod((rgb.g - rgb.b) / delta, 6);
                else if (maxVal == rgb.g)
                    hsv.x = (rgb.b - rgb.r) / delta + 2;
                else
                    hsv.x = (rgb.r - rgb.g) / delta + 4;

                hsv.x *= 60; // Convert hue to degrees
                if (hsv.x < 0)
                    hsv.x += 360; // Ensure hue is positive

                // Saturation calculation
                hsv.y = (maxVal == 0) ? 0 : (delta / maxVal);

                // Value calculation
                hsv.z = maxVal;
                return hsv;
            }
            
            float3 HSVToRGB(float3 hsv)
            {
                float3 rgb;
                float c = hsv.z * hsv.y; // Chroma
                float x = c * (1 - abs(fmod(hsv.x / 60, 2) - 1));
                float m = hsv.z - c;

                if (hsv.x < 60)
                    rgb = float3(c, x, 0);
                else if (hsv.x < 120)
                    rgb = float3(x, c, 0);
                else if (hsv.x < 180)
                    rgb = float3(0, c, x);
                else if (hsv.x < 240)
                    rgb = float3(0, x, c);
                else if (hsv.x < 300)
                    rgb = float3(x, 0, c);
                else
                    rgb = float3(c, 0, x);

                rgb += m; // Adjust to match original value

                return rgb;
            }

            Interpolation vert (appdata v)
            {
                Interpolation o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);                
		        o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag(Interpolation i) : SV_Target
            {

                // Block glitch
                
                uint block_size = _BlockSize;
                uint columns = _ScreenParams.x / block_size;

                // Block index
                uint2 block_xy = i.uv * _ScreenParams.xy / block_size;
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
                ssp += (uint2)(i.uv * _ScreenParams.xy) % block_size;

                // UV recalculation
                float2 blockOffset = frac((ssp + 0.5) / _ScreenParams.xy);
                blockOffset *= _ScreenParams.xy;

                //flickering
                float2 center = float2(0.5, 0.5);
                float strenght = _Time.y * _NoiseStrength;
                float stripe = (i.uv.y + strenght);
                float2 noiseUV = float2(stripe, stripe);
                float gradientNoise;
                Unity_GradientNoise_float(noiseUV, _NoiseAmount, gradientNoise) ;
                float noise1;
                Unity_Remap_float(gradientNoise, float2(0, 1), float2(-_NoiseIntensity, _NoiseIntensity), noise1);

                float noise2;
                Unity_GradientNoise_float(strenght, _NoiseAmount, noise2);
                noise2 *= noise2;
                noise2 *= noise2;
                noise2 *= 0.1;

                float finalNoise = noise1 * noise2;

                //scanline
                float scanline = clamp(sin((i.uv.y*_ScanlineAmount + strenght)), 1-_ScanlineStrength ,1);
                float remappedScanline;
                Unity_Remap_float(scanline, float2(-1, 1), float2(0.2, 1), remappedScanline);

                float mask = step(0 + _Padding, i.uv.x) * step( i.uv.x , 1 - _Padding); 
                float4 outTexture = tex2D(_CameraSortingLayerTexture, i.screenPos + (float2(finalNoise, 0) * mask)) * remappedScanline;
                
                if (frac(rand * 1234) < _BlockStrength * 0.1)
                {
                    float3 hsv = RGBToHSV(outTexture.rgb);
                    hsv = hsv * float3(-1, 1, 0) + float3(0.5, 0, 0.9);
                    outTexture.rgb = HSVToRGB(hsv);
                }
                
                return (outTexture);
            }
            ENDCG
        }
    }
}
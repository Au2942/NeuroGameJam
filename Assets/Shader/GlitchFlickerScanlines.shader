Shader "Unlit/Glitch_Flicker_Shader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NoiseStrength ("Noise Strength", Range(0, 1)) = 0.5
        _NoiseAmount ("Noise Amount", Range(0, 100)) = 50
        _NoiseIntensity ("Noise Intensity", Range(0, 10)) = 1
        _ScanlineStrength ("Scanline Strength", Range(0, 1)) = 1
        _ScanlineAmount ("Scanline Amount", Range(0, 1000)) = 600
        _MeshBound ("Mesh Bound", Vector) = (0, 0, 1920, 1080)
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
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _NoiseStrength;
            float _NoiseAmount;
            float _NoiseIntensity;
            float _ScanlineStrength;
            float _ScanlineAmount;
            float4 _MeshBound;
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

            Varyings vert(Attributes IN)
            {
                // Declaring the output object (OUT) with the Varyings struct.
                Varyings Output;
                // The TransformObjectToHClip function transforms vertex positions
                // from object space to homogenous clip space.
                Output.vertex = TransformObjectToHClip(IN.vertex.xyz);
                Output.uv = IN.texcoord;
                // Returning the output.        
		        Output.screenPos = ComputeScreenPos(Output.vertex);
                return Output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 screenUV = input.screenPos.xy / input.screenPos.w;
                
                //flickering
                
                float2 center = float2(0.5, 0.5);
                float strenght = _Time.y * _NoiseStrength;
                float stripe = (input.uv.y + strenght);
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
                float scanline = clamp(sin((input.uv.y*_ScanlineAmount + strenght)), 1-_ScanlineStrength ,1);
                float remappedScanline;
                Unity_Remap_float(scanline, float2(-1, 1), float2(0.2, 1), remappedScanline);

                float2 finalUV;
                finalUV.x = clamp(screenUV.x + finalNoise, _MeshBound.x / _ScreenSize.x, _MeshBound.z / _ScreenSize.x);
                finalUV.y = screenUV.y;
                float4 outTexture = tex2D(_CameraSortingLayerTexture,  finalUV) * remappedScanline;
                
                
                return (outTexture);
            }
            ENDHLSL
        }
    }
}
Shader "CustomEffects/FlickerScanline_Shader"
{
    HLSLINCLUDE
    
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

        float _NoiseStrength;
        float _NoiseAmount;
        float _NoiseIntensity;
        float _ScanlineStrength;
        float _ScanlineAmount;
            

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

        float4 Frag(Varyings input) : SV_Target
        {
            
            //flickering
            float strength = _Time.y * _NoiseStrength;
            float stripe = (input.texcoord.y + strength);
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

            //scanline
            float scanline = clamp(sin((input.texcoord.y*_ScanlineAmount + strength)), 1-_ScanlineStrength ,1);
            float remappedScanline;
            Unity_Remap_float(scanline, float2(-1, 1), float2(0.2, 1), remappedScanline);

            float2 uv = float2(input.texcoord.x + finalNoise, input.texcoord.y);

            float4 c = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv);
            c.rgb *= remappedScanline;
            
            return c;
        }
    ENDHLSL
    Properties
    {
        _NoiseStrength ("Noise Strength", Range(0, 1)) = 0.5
        _NoiseAmount ("Noise Amount", Range(0, 100)) = 50
        _NoiseIntensity ("Noise Intensity", Range(0, 1)) = 0.1
        _ScanlineStrength ("Scanline Strength", Range(0, 1)) = 1
        _ScanlineAmount ("Scanline Amount", Range(0, 1000)) = 600
    }
    SubShader
    {		
        
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        ZWrite Off Cull Off
        
        Pass
        {
            
            Name "FlickerScanlinePass"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            ENDHLSL
        }
    }
}
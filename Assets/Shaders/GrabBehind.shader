Shader "Unlit/Grab_Behind"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
	        sampler2D _CameraSortingLayerTexture;

    

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
                float4 outTexture = tex2D(_CameraSortingLayerTexture, i.screenPos);
                return (outTexture);
            }
            ENDCG
        }
    }
}
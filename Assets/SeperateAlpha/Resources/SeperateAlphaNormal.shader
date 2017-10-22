Shader "SeperateAlpha/Normal"
{
    Properties
    {
        _MainTex ("RGB", 2D) = "black" {}
        _AlphaTex ("Alpha", 2D) = "black" {}
    }
    
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag 

            #pragma multi_compile __ ENABLE_SPLITALPHA     

            #include "UnityCG.cginc"  

            sampler2D _MainTex;
			float4 _MainTex_ST;

            #ifdef ENABLE_SPLITALPHA
            sampler2D _AlphaTex;
            #endif
    
            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                fixed4 color : COLOR;
            };
    
            struct v2f
            {
                float4 vertex : SV_POSITION;
                half2 texcoord : TEXCOORD0;
                fixed4 color : COLOR;
            };
    
            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color;
                return o;
            }
                
            fixed4 frag (v2f IN) : COLOR
            {
                fixed4 col;
            #ifdef ENABLE_SPLITALPHA                
                col.rgb = tex2D(_MainTex, IN.texcoord).rgb;
                col.a = tex2D(_AlphaTex, IN.texcoord).r;
                col = col * IN.color;
            #else
                col = tex2D(_MainTex, IN.texcoord);
            #endif
                
                return col;  
            }
            ENDCG
        }
    }
	CustomEditor "SeperateAlphaGUI"
}

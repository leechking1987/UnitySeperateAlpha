Shader "SeperateAlpha/UI"
{
    Properties
    {
        [PerRendererData]_MainTex ("RGB", 2D) = "black" {}
        _AlphaTex ("Alpha", 2D) = "black" {}

		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255

		_ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }
    
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
        }

		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp] 
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag 

            #pragma multi_compile __ ENABLE_SPLITALPHA     

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile __ UNITY_UI_ALPHACLIP      

            sampler2D _MainTex;

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
                o.texcoord = v.texcoord;
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

            #ifdef UNITY_UI_ALPHACLIP
                clip (col.a - 0.001);
            #endif
                
                return col;  
            }
            ENDCG
        }
    }
	CustomEditor "SeperateAlphaGUI"
}

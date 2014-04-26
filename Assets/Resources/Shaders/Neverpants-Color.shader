Shader "Neverpants/Color"
{
	Properties 
	{
		_Color ("Color", Color) = (1, 1, 1, 1)
	}

	SubShader 
	{
		Tags {"Queue"="Transparent" "RenderType"="Transparent"}
		LOD 100
		
		Lighting Off
		Blend SrcAlpha OneMinusSrcAlpha 
		
        Pass 
        {            
			CGPROGRAM 
			#pragma vertex vert
			#pragma fragment frag
							
			#include "UnityCG.cginc"
			
			float4 _Color;
					
			struct v2f 
			{
			    float4 pos : SV_POSITION;
			    float2 uv : TEXCOORD1;
			};
			
			v2f vert(appdata_base v)
			{
			    v2f o;			    
			    o.uv = v.texcoord.xy;
			    o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			    
			    return o;
			}
			
			half4 frag(v2f i) : COLOR
			{   
			    return _Color;
			}
			ENDCG
	  	}
	}
}
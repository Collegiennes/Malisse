Shader "Neverpants/Camera Fog of War"
{
	Properties 
	{
		_Color ("Color", Color) = (1, 1, 1, 1)
		_Bounds ("display name", Vector) = (0, 0, 0, 0)			// Bounds as top, left, bottom, right
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
			float4 _Bounds;
					
			struct v2f 
			{
			    float4 pos : SV_POSITION;
			    float2 uv : TEXCOORD1;
			    float3 worldPos : TEXCOORD2;
			};
			
			v2f vert(appdata_base v)
			{
			    v2f o;			    
			    o.uv = v.texcoord.xy;
			    o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			    o.worldPos = mul(_Object2World, v.vertex).xyz;
			    
			    return o;
			}
			
			half4 frag(v2f i) : COLOR
			{   
				float top = _Bounds.x;
				float left = _Bounds.y;
				float bottom = _Bounds.z;
				float right = _Bounds.w;
				float4 colorTemp = _Color;
				
				if (i.worldPos.x >= left && i.worldPos.x <= right && i.worldPos.y >= bottom && i.worldPos.y <= top)
				{
					colorTemp = half4(_Color.r, _Color.g, _Color.b, 0.0);
				}
			
			    return colorTemp;
			}
			ENDCG
	  	}
	}
}
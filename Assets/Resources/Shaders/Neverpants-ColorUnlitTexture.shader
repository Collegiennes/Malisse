Shader "Neverpants/ColorUnlitTexture"
{
	Properties 
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	
	Category 
	{
		SubShader 
		{
			Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
			Lighting Off 
  	 		Blend SrcAlpha OneMinusSrcAlpha 
			
			Pass 
			{
				SetTexture [_MainTex] 
				{
					constantColor [_Color]
					Combine texture * constant
				}
			}
		}
	}
}

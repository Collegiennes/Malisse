Shader "Neverpants/ColorUnlitTextureAlwaysOnTop"
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
				ZTest Always
				SetTexture [_MainTex] 
				{
					constantColor [_Color]
					Combine texture * constant
				}
			}
		}
	}
}

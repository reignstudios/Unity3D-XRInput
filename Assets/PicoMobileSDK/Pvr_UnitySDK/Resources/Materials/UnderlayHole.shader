Shader "Pvr_UnitySDK/UnderlayHole"
{
	Properties
	{
	   _MainTex("Albedo (RGB)", 2D) = "black" {}
	}
		
	SubShader
	{
			Tags { "Queue" = "Geometry+1" "RenderType" = "Opaque" }
			LOD 200
			//ColorMask A

			CGPROGRAM
		   #pragma surface surf Standard fullforwardshadows keepalpha

		   sampler2D _MainTex;

		   struct Input 
		   {
			   float2 uv_MainTex;
		   };


		   void surf(Input IN, inout SurfaceOutputStandard o) 
		   {
			   fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			   o.Albedo = c.rgb;
			   o.Alpha = 0.;
		   }
		   ENDCG
	}

		
	FallBack "Diffuse"
}

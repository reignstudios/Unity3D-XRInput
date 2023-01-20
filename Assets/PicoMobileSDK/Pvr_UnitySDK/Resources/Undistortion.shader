// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Pvr_UnitySDK/Undistortion" {

  Properties {
    _MainTex ("Base (RGB)", 2D) = "white" {}
  }

  SubShader {
    Tags { "RenderType" = "Opaque" }
    LOD 150

    Pass {
      CGPROGRAM
      #include "UnityCG.cginc"
      #pragma vertex vert
      #pragma fragment frag

      struct v2f {
        float4 vertex  : SV_POSITION;
        half2 texcoord : TEXCOORD0;
      };

      sampler2D _MainTex;
      float4    _MainTex_ST;

      float4 _Distortion1;   // Radial distortion coefficients  k1, k2, k3.
	  float4 _Distortion2;   // Radial distortion coefficients  k4, k5, k6.
      float4 _Projection;   // Bits of the lens-affected projection matrix.
      float4 _Unprojection; // Bits of the no-lens projection matrix.

      v2f vert (appdata_base v) {
        // This is totally standard code, including the _MainTex transform.
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
        return o;
      }

      fixed4 frag (v2f i) : SV_Target {
        // i.texcoord refers to the onscreen pixel we want to color.  We have to find the
        // corresponding pixel in the rendered texture, which means going through the lens'
        // distortion transformation.
        // First: go from these onscreen texture coordinates back into camera space, using parts
        // of the projection matrix for the "no lens" frustum.  (We don't need the whole matrix
        // because we don't care about Z.)
        float2 tex = (i.texcoord + _Unprojection.zw) / _Unprojection.xy;
        // Second: Radially distort the vector according to the lens' coefficients (pincushion).
		float r2 = dot(tex, tex);
		float r = sqrt( r2 );
		r = r / _Distortion1.w;
		float r3 = _Distortion1.x * pow(r, 5) + _Distortion1.y * pow(r, 4) + _Distortion1.z * pow(r, 3) + _Distortion2.x * pow(r, 2) + _Distortion2.y * r  + _Distortion2.z;
		r3 =  r3 / r;
		tex *= r3;
        // Reproject the vector into the lens-affected frustum to get the texture coordinates
        // to look up.
        tex = saturate(tex * _Projection.xy - _Projection.zw);
        // Get the color.
        fixed vignette = saturate(min(min(tex.x, 1-tex.x), min(tex.y, 1-tex.y)) / 0.03);
        fixed4 col = lerp(fixed4(0,0,0,1), tex2D(_MainTex, tex), vignette);
        return col;
      }
      ENDCG
    }
  }

}

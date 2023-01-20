
Shader "Pvr_UnitySDK/Laser" {
  Properties {
    _MainTex ("Particle Texture", 2D) = "white" {}
  }

  Category {
    Tags {
      "Queue"="Overlay+100"
      "IgnoreProjector"="True"
      "RenderType"="Transparent"
      "PreviewType"="Plane"
    }

    Blend SrcAlpha One
    Cull Off Lighting Off ZWrite Off Fog { Color (0,0,0,0) }

    BindChannels {
      Bind "Color", color
      Bind "Vertex", vertex
      Bind "TexCoord", texcoord
    }

    SubShader {
      Pass {
        SetTexture [_MainTex] {
          combine texture * primary
        }
      }
    }
  }
}

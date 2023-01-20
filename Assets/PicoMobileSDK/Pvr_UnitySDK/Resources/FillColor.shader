Shader "Pvr_UnitySDK/FillColor" {
    Properties {
        _Color ("Main Color", COLOR) = (1,1,1,1)
    }
    SubShader {
        Pass { Color [_Color] }
    }
}
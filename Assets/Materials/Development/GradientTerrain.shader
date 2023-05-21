Shader "Custom/GradientTerrain"
{
    Properties
    {
        [Header(Gradient Control)]
        _FlatColor              ("Flat Color", Color)            = (0, 0, 0, 1)
        _GradientColor          ("Gradient Color", Color)        = (0, 0, 0, 1)
        _SwitchGradient         ("Switch Point", Range(0, 1))    = 0.5
        _GradientBlendPower     ("Blend Power", Range(0, 1))     = 0
    }

    SubShader {

      Tags { "RenderType" = "Opaque" "disablebatching" = "True" }

      CGPROGRAM

      #pragma surface surf Lambert
      #pragma target 3.5
      
      struct Input {
          float2 uv_MainTex;  //INTERNAL
          float3 worldPos;    //INTERNAL
          float3 worldNormal; //INTERNAL
          float4 tangent;     //INTERNAL
      };
      
      float _Radius;

      fixed3 _FlatColor;
      fixed3 _GradientColor;
      float _SwitchGradient;
      float _GradientBlendPower;
      
      void surf (Input IN, inout SurfaceOutput o) {
          float3 localPos = IN.worldPos - mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;

          //Steepness
          float _GradientBlend = abs(_SwitchGradient) * _GradientBlendPower;

          float steepness = 1 - dot(float3(0, 1, 0), IN.worldNormal);
          float3 steepnessAlbedo;
          if(steepness > _SwitchGradient + _GradientBlend) steepnessAlbedo = _GradientColor;
          else if(steepness < _SwitchGradient - _GradientBlend) steepnessAlbedo = _FlatColor;
          else{
              float remaining = steepness - (_SwitchGradient - _GradientBlend);
              steepnessAlbedo = lerp(_FlatColor, _GradientColor, remaining / (_GradientBlend * 2));
          }

          o.Albedo = steepnessAlbedo;
      }

      ENDCG
    }
    Fallback "Diffuse"
}

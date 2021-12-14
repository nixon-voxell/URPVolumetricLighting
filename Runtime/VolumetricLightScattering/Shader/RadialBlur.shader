Shader "Hidden/RadialBlur"
{
  Properties
  {
    _MainTex ("Texture", 2D) = "white" {}
    _Color ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
    _BlurWidth ("Blur Width", Range(0, 1)) = 0.85
    _Intensity ("Intensity", Range(0, 1)) = 1
    _Center ("Center", Vector) = (0.5, 0.5, 0, 0)
    _NumSamples ("Number of Samples", Range(50, 200)) = 100

    // noise
    _NoiseSpeed ("Noise Speed", Vector) = (0.5, 0.5, 0.0, 0.0)
    _NoiseScale ("Noise Scale", Float) = 1.0
    _NoiseStrength ("Noise Strength", Range(0, 1)) = 0.6
  }
  SubShader
  {
    // additive blend mode
    Blend One One

    Pass
    {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag

      #include "UnityCG.cginc"
      #include "./GradientNoise.hlsl"

      struct appdata
      {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
      };

      struct v2f
      {
        float2 uv : TEXCOORD0;
        float4 vertex : SV_POSITION;
      };

      v2f vert (appdata v)
      {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = v.uv;
        return o;
      }

      sampler2D _MainTex;
      half4 _Color;
      float _BlurWidth;
      float _Intensity;
      float4 _Center;
      uint _NumSamples;

      // noise
      float2 _NoiseSpeed;
      float _NoiseScale;
      float _NoiseStrength;

      fixed4 frag (v2f i) : SV_Target
      {
        fixed4 color = fixed4(0.0f, 0.0f, 0.0f, 1.0f);
        float2 texCoord = i.uv;

        float2 ray = texCoord - _Center.xy;
        float denom = 1.0 / float(_NumSamples) * _BlurWidth;

        // UNITY_UNROLL
        for (int i=0; i < _NumSamples; i++)
        {
          float scale = 1.0f - float(i) * denom;
          fixed3 texCol = tex2D(_MainTex, (ray * scale) + _Center.xy).xyz;
          color.xyz += texCol * denom;
        }

        float noise = gradientNoise_float(texCoord + _Time.y*_NoiseSpeed, _NoiseScale);
        noise = noise*_NoiseStrength + (1.0 - _NoiseStrength);

        return color * noise * _Intensity * _Color;
      }
      ENDCG
    }
  }
}

Shader "Hidden/Occluder"
{
  Properties
  {
    _Color("Main Color", Color) = (0.0, 0.0, 0.0, 0.0)
  }

  SubShader
  {
    Tags { "RenderType" = "Opaque" }
    ZWrite Off Cull Off
    Fog {Mode Off}
    Color[_Color]

    Pass {}
  }
}

// Shader "Hidden/Occluder"
// {
//   SubShader
//   {
//     Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
//     LOD 100
//     ZWrite Off Cull Off
//     Pass
//     {
//       Name "OccluderPass"

//       HLSLPROGRAM
//       #pragma vertex vert
//       #pragma fragment frag
//       #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

//       struct Attributes
//       {
//         float4 positionHCS   : POSITION;
//         float2 uv           : TEXCOORD0;
//         UNITY_VERTEX_INPUT_INSTANCE_ID
//       };

//       struct Varyings
//       {
//         float4  positionCS  : SV_POSITION;
//         float2  uv          : TEXCOORD0;
//         UNITY_VERTEX_OUTPUT_STEREO
//       };

//       Varyings vert(Attributes input)
//       {
//         Varyings output;
//         UNITY_SETUP_INSTANCE_ID(input);
//         UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

//         // Note: The pass is setup with a mesh already in clip
//         // space, that's why, it's enough to just output vertex
//         // positions
//         output.positionCS = float4(input.positionHCS.xyz, 1.0);

//         #if UNITY_UV_STARTS_AT_TOP
//         output.positionCS.y *= -1;
//         #endif

//         output.uv = input.uv;
//         return output;
//       }

//       TEXTURE2D_X(_CameraOpaqueTexture);
//       SAMPLER(sampler_CameraOpaqueTexture);

//       half4 frag (Varyings input) : SV_Target
//       {
//         UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
//         float4 color = SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, input.uv);
//         return color;
//       }
//       ENDHLSL
//     }
//   }
// }
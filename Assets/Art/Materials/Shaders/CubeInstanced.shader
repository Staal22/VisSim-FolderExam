Shader "Custom/CubeInstanced"
{
    Properties
    {
        _FarColor("Far color", Color) = (.2, .2, .2, 1)
    }
    SubShader
    {
        Pass
        {
            Tags
            {
                "RenderType"="Opaque"
                "RenderPipeline" = "UniversalRenderPipeline"
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            float4 _FarColor;

            StructuredBuffer<float4> position_buffer;

            struct attributes
            {
                float3 normal : NORMAL;
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct varyings
            {
                float4 vertex : SV_POSITION;
                float3 diffuse : TEXCOORD2;
                float3 color : TEXCOORD3;
            };

            varyings vert(attributes v, const uint instance_id : SV_InstanceID)
            {
                float4 position = position_buffer[instance_id];
                
                const float3 world_position = position.xyz + v.vertex.xyz;

                const float3 pos = world_position;
                const float3 color = float3(0.4f,0.4f,0.4f);

                varyings o;
                o.vertex = mul(UNITY_MATRIX_VP, float4(pos, 1.0f));
                o.diffuse = saturate(dot(v.normal, _MainLightPosition.xyz));
                o.color = color;
                
                return o;
            }

            half4 frag(const varyings i) : SV_Target
            {
                const float3 lighting = i.diffuse *  1.7;
                return half4(i.color * lighting, 1);;
            }
            ENDHLSL
        }
    }
}
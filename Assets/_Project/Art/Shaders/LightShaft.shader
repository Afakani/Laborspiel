// Weicher Lichtkegel (Fake-Volumetrik): additiv, Rand blendet über den Blickwinkel aus,
// Helligkeit fällt von der Düse (UV v = 1) zum Boden (v = 0) ab, weiche Kante an Geometrie über die Tiefe.
Shader "PlanErfuellt/LightShaft"
{
    Properties
    {
        [HDR] _Color ("Farbe", Color) = (0.4, 1, 0.35, 1)
        _Intensity ("Intensität", Range(0, 4)) = 0.6
        _FresnelPower ("Randweichheit", Range(0.5, 8)) = 2.5
        _Falloff ("Abfall zum Boden", Range(0.2, 6)) = 1.6
        _SoftDepth ("Weiche Kante (m)", Range(0.01, 3)) = 0.6
        _NoiseScale ("Schlierengröße", Range(0.5, 20)) = 6
        _NoiseSpeed ("Schlierentempo", Range(0, 2)) = 0.25
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent+20" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }
        Pass
        {
            Name "Shaft"
            Tags { "LightMode" = "UniversalForward" }
            Blend One One
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Intensity;
                half _FresnelPower;
                half _Falloff;
                half _SoftDepth;
                half _NoiseScale;
                half _NoiseSpeed;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
                float4 screenPos : TEXCOORD3;
                float3 positionWS : TEXCOORD4;
            };

            Varyings Vert(Attributes v)
            {
                Varyings o;
                VertexPositionInputs p = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionCS = p.positionCS;
                o.positionWS = p.positionWS;
                o.uv = v.uv;
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.viewDirWS = GetWorldSpaceViewDir(p.positionWS);
                o.screenPos = ComputeScreenPos(p.positionCS);
                return o;
            }

            // Billiges Wertrauschen für leichte Schlieren im Kegel
            float Hash(float2 p) { return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453); }
            float Noise(float2 p)
            {
                float2 i = floor(p), f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(Hash(i), Hash(i + float2(1, 0)), f.x), lerp(Hash(i + float2(0, 1)), Hash(i + 1), f.x), f.y);
            }

            half4 Frag(Varyings i) : SV_Target
            {
                float3 n = normalize(i.normalWS);
                float3 vdir = normalize(i.viewDirWS);
                // Mitte des Kegels hell, Silhouette weich
                half facing = pow(saturate(abs(dot(n, vdir))), _FresnelPower);
                half along = pow(saturate(i.uv.y), _Falloff);

                float2 suv = i.screenPos.xy / i.screenPos.w;
                float sceneDepth = LinearEyeDepth(SampleSceneDepth(suv), _ZBufferParams);
                float fragDepth = i.screenPos.w;
                half soft = saturate((sceneDepth - fragDepth) / _SoftDepth);
                // Nahe der Kamera ausblenden, damit man nicht „in“ einer Fläche steht
                half nearFade = saturate((fragDepth - 0.3) / 1.2);

                float t = _Time.y * _NoiseSpeed;
                half streak = lerp(0.65, 1.15, Noise(float2((i.positionWS.x + i.positionWS.z) * _NoiseScale * 0.5, i.positionWS.y * 1.5 + t)));

                half a = facing * along * soft * nearFade * streak * _Intensity;
                return half4(_Color.rgb * a, 1);
            }
            ENDHLSL
        }
    }
}

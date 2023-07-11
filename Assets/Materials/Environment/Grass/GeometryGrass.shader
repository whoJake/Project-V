Shader "Environment/GeometryGrass"
{
    Properties
    {
        _TipColor("Blade Tip Color", Color) = (1, 1, 1)
        _BaseColor("Blade Base Color", Color) = (1, 1, 1)
        _BladeWidth("Blade Width", Float) = 0.01
        _BladeBodyHeight("Blade Body Height", Float) = 0.2
        _BladeTipHeight("Blade Tip Height", Float) = 0.1
    }
    SubShader
    {
        Cull Back
        ZWrite On
        Tags{
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma require geometry
            #pragma geometry geom

            //Needed for geometry shaders
            #pragma target 4.5

            #include "UnityCG.cginc"

            struct vd
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2g
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            struct g2f{
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };


            v2g vert (vd v)
            {
                v2g o;
                o.vertex = v.vertex;
                o.uv = v.uv;
                return o;
            }

            float _BladeWidth;
            float _BladeBodyHeight;
            float _BladeTipHeight;

            [maxvertexcount(9)]
            void geom(point v2g points[1], inout TriangleStream<g2f> triStream){
                float3 root = points[0].vertex;
                float3 forward = normalize(mul(unity_ObjectToWorld, float4(root, 1)) - _WorldSpaceCameraPos);
                float3 up = float3(0, 1, 0);
                forward.y = 0;
                forward = normalize(forward);

                float3 right = cross(forward, up);

                g2f verts[5];
                float3 v0 = root + (-0.5 * _BladeWidth * right);

                float3 v1 = root + (0.5 * _BladeWidth * right);

                float3 v2 = root + (0.5 * _BladeWidth * right) + _BladeBodyHeight * up;

                float3 v3 = root + (-0.5 * _BladeWidth * right) + _BladeBodyHeight * up;

                float3 v4 = root + (_BladeBodyHeight + _BladeTipHeight) * up;

                verts[0].vertex = UnityObjectToClipPos(float4(v0, 1));
                verts[0].uv = float2(0, 0);
                verts[1].vertex = UnityObjectToClipPos(float4(v1, 1));
                verts[1].uv = float2(1, 0);
                verts[2].vertex = UnityObjectToClipPos(float4(v2, 1));
                verts[2].uv = float2(1, 0.5);
                verts[3].vertex = UnityObjectToClipPos(float4(v3, 1));
                verts[3].uv = float2(0, 0.5);
                verts[4].vertex = UnityObjectToClipPos(float4(v4, 1));
                verts[4].uv = float2(0.5, 1);

                triStream.Append(verts[0]);
                triStream.Append(verts[1]);
                triStream.Append(verts[3]);

                triStream.Append(verts[2]);
                triStream.Append(verts[3]);
                triStream.Append(verts[1]);
                
                triStream.Append(verts[2]);
                triStream.Append(verts[4]);
                triStream.Append(verts[3]);


                triStream.RestartStrip();
            }

            fixed4 _TipColor;
            fixed4 _BaseColor;

            fixed4 frag (g2f i) : SV_Target
            {
                return lerp(_BaseColor, _TipColor, i.uv.y);
            }
            ENDCG
        }
    }
}

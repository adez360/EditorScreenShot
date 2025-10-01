Shader "Hidden/Freecam/Fisheye"
{
    Properties { _MainTex("Tex", 2D) = "white" {}  _Strength("Strength", Range(0,1)) = 0 }
    SubShader
    {
        Tags{ "RenderType"="Opaque" "Queue"="Overlay" }
        Cull Off ZTest Always ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _Strength;

            struct v2f { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; };
            v2f vert(uint id:SV_VertexID){
                float2 v=float2((id<<1)&2,id&2);
                v2f o; o.pos=float4(v*2-1,0,1); o.uv=v; return o;
            }

            float2 distort(float2 uv, float k)
            {
                float2 p = uv*2-1;
                float r = length(p);
                float nr = r + (r*r - r)*k; // simple barrel distortion
                float2 dir = (r>1e-5) ? p/r : 0;
                float2 q = dir * nr;
                return (q+1)*0.5;
            }

            float4 frag(v2f i):SV_Target
            {
                float2 uv = distort(i.uv, _Strength);
                return tex2D(_MainTex, uv);
            }
            ENDHLSL
        }
    }
}
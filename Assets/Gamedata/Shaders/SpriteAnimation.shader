Shader "SimplestInstancedShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "IgnoreProjector"="True" "RenderQueue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID // necessary only if you want to access instanced properties in fragment Shader.
            };
            
            sampler2D _MainTex;
//            float4 _MainTex_UV;

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
                UNITY_DEFINE_INSTANCED_PROP(fixed4, _MainTex_UV)
            UNITY_INSTANCING_BUFFER_END(Props)
           
            v2f vert(appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o); // necessary only if you want to access instanced properties in the fragment Shader.

//                fixed4 uvs = UNITY_ACCESS_INSTANCED_PROP(Props, _MainTex_UV);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = (v.uv * UNITY_ACCESS_INSTANCED_PROP(Props, _MainTex_UV).xy) + UNITY_ACCESS_INSTANCED_PROP(Props, _MainTex_UV).zw; 
                return o;
            }
           
            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i); // necessary only if any instanced properties are going to be accessed in the fragment Shader.
                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}
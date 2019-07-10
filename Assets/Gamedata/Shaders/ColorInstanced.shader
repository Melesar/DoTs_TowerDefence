Shader "ColorInstanced"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _ColorZero("Color zero health", Color) = (1, 0, 0, 1)
        _ColorFull("Color full health", Color) = (0, 1, 0, 1)
        [PerRendererData]_Fill ("Fill", float) = 1
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
            fixed4 _ColorZero;
            fixed4 _ColorFull;

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float, _Fill)
                UNITY_DEFINE_INSTANCED_PROP(fixed4, _MainTex_UV)
            UNITY_INSTANCING_BUFFER_END(Props)
           
            v2f vert(appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o); // necessary only if you want to access instanced properties in the fragment Shader.

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv; 
                return o;
            }
           
            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i); // necessary only if any instanced properties are going to be accessed in the fragment Shader.
                
                float fill = UNITY_ACCESS_INSTANCED_PROP(Props, _Fill);
                fixed4 color = lerp(_ColorZero, _ColorFull, fill);
                color = tex2D(_MainTex, i.uv) * color;
                color.a = i.uv.x > fill ? 0 : 1;
                
                return color;
            }
            ENDCG
        }
    }
}
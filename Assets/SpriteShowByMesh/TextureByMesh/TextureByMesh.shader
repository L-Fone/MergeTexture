Shader "HM/TextureByMesh"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {}
    //渲染大小
    _SizeRange("Size Range", Range(1, 20)) = 20
    }

        SubShader
    {
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            half4 _MainTex_TexelSize;
            fixed _SizeRange;

            struct appdata
            {
                float4 vertex   : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                half2 uv  : TEXCOORD0;
            };

            v2f vert(appdata i)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(i.vertex * _SizeRange);
                o.uv = TRANSFORM_TEX(i.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv);
            return c;
            }

            ENDCG
        }
    }
}

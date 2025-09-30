Shader "Custom/DissolveUnlitSprite"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _DissolveAmount ("Dissolve Amount", Range(0,1)) = 0
        _EdgeColor ("Edge Color", Color) = (1,0.5,0,1)
        _EdgeWidth ("Edge Width", Range(0.01,0.2)) = 0.08
        _Color ("Tint", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _NoiseTex;
            float4 _NoiseTex_ST;
            float _DissolveAmount;
            float4 _EdgeColor;
            float _EdgeWidth;
            float4 _Color;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float dissolve = _DissolveAmount;
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                float noise = tex2D(_NoiseTex, i.uv).r;

                // Disolver por ruido
                if(noise < dissolve)
                {
                    discard;
                }

                // Borde
                if (noise < dissolve + _EdgeWidth && noise >= dissolve)
                {
                    col.rgb = _EdgeColor.rgb;
                    col.a = max(_EdgeColor.a, col.a);
                }

                return col;
            }
            ENDCG
        }
    }
}

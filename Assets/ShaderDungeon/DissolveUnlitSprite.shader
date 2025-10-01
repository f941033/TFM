Shader "Custom/DissolveUnlitSprite"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0
        _EdgeWidth ("Edge Width", Range(0.01, 0.2)) = 0.1
        _EdgeColor ("Edge Color", Color) = (1, 0.5, 0, 1)
        _Color ("Tint", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="True"}
        
        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            float4 _MainTex_ST;
            float4 _NoiseTex_ST;
            float _DissolveAmount;
            float _EdgeWidth;
            fixed4 _EdgeColor;
            fixed4 _Color;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
                
                // Muestra de ruido
                float noise = tex2D(_NoiseTex, IN.texcoord * _NoiseTex_ST.xy).r;
                
                // Calcular disolución
                float dissolve = _DissolveAmount;
                
                // Si el ruido es menor que el dissolve amount, descartar pixel
                if (noise < dissolve)
                    discard;
                
                // Efecto de borde brillante
                if (noise < dissolve + _EdgeWidth)
                {
                    c.rgb = _EdgeColor.rgb;
                    c.a = _EdgeColor.a;
                }
                
                return c;
            }
            ENDCG
        }
    }
}

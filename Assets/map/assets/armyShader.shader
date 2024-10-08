Shader "Unlit/NewUnlitShader"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _MaskTex ("Mask Texture", 2D) = "white" {}   // Tekstura maski
        _ColorChange ("Color", Color) = (1,1,1,1)    // Kolor dla tu³owia
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        // Zastosowanie blendingu dla przezroczystoœci
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Lighting Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float2 texcoord : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _MaskTex; // Tekstura maski
            float4 _ColorChange;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Oryginalny kolor i alfa sprite'a
                fixed4 originalColor = tex2D(_MainTex, i.texcoord);
                
                // Odczytanie maski
                float maskValue = tex2D(_MaskTex, i.texcoord).r; // Zak³adamy maskê w skali szaroœci
                
                // Mieszamy kolor, jeœli maska ma wartoœæ powy¿ej 0.5
                if (maskValue > 0.5)
                {
                    originalColor.rgb = lerp(originalColor.rgb, _ColorChange.rgb, maskValue);
                }
                
                // Zwróæ kolor z oryginaln¹ przezroczystoœci¹
                return originalColor;
            }
            ENDCG
        }
    }
}
Shader "Unlit/NewUnlitShader"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _MaskTex ("Mask Texture", 2D) = "white" {}  // Mask texture
        _ColorChange ("Color", Color) = (1,1,1,1)    // Color for the torso
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        // Applying blending for transparency
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
            sampler2D _MaskTex; // Mask texture
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
                // Original color and alpha of the sprite
                fixed4 originalColor = tex2D(_MainTex, i.texcoord);
                
                // Reading the mask
                float maskValue = tex2D(_MaskTex, i.texcoord).r; // Assuming a grayscale mask
                
                // Blending the color if the mask value is above 0.5
                if (maskValue > 0.5)
                {
                    originalColor.rgb = lerp(originalColor.rgb, _ColorChange.rgb, maskValue);
                }
                
                // Return the color with original transparency
                return originalColor;
            }
            ENDCG
        }
    }
}
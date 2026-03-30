Shader "Custom/DamageFlash"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        // ✨ 新增：Boss 或菁英怪的專屬底色 (預設為透明無色)
        _OverlayColor ("Base Tint Color", Color) = (0,0,0,0) 
        _FlashColor ("Flash Color", Color) = (1,1,1,1) 
        _FlashAmount ("Flash Amount", Range(0,1)) = 0.0 
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };
            struct v2f { float2 uv : TEXCOORD0; float4 vertex : SV_POSITION; fixed4 color : COLOR; };

            sampler2D _MainTex;
            float4 _OverlayColor; // ✨ 接收剛剛新增的顏色變數
            float4 _FlashColor;
            float _FlashAmount;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color; 
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 讀取原圖
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                
                // ✨ 魔法 1：疊加專屬染色
                // 利用 _OverlayColor 的「Alpha (透明度)」來決定要染多深！
                col.rgb = lerp(col.rgb, _OverlayColor.rgb, _OverlayColor.a);
                
                // ✨ 魔法 2：蓄力純白閃爍 (優先級最高，會蓋過上面的染色)
                col.rgb = lerp(col.rgb, _FlashColor.rgb, _FlashAmount);
                
                return col;
            }
            ENDCG
        }
    }
}
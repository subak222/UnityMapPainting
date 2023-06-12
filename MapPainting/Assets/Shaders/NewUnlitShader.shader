Shader "Unlit/NewUnlitShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

Pass {
    CGPROGRAM
    #pragma vertex vert
    #pragma fragment frag

    #include "UnityCG.cginc"

    struct appdata {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
    };

    struct v2f {
        float4 vertex : SV_POSITION;
        float2 uv : TEXCOORD0;
    };

    sampler2D _MainTex;
    fixed4 _Color;

    v2f vert(appdata v) {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = v.uv;
        return o;
    }

    fixed4 frag(v2f i) : SV_Target {
        // 닿은 부분을 판단하고, 색상 값을 계산하여 반환
        fixed4 color = fixed4(1, 1, 1, 1); // 초기 색상 값 설정

        // 닿은 부분의 픽셀 색상을 읽어옴
        fixed4 texColor = tex2D(_MainTex, i.uv);

        // 닿은 부분의 색상을 계산
        color.rgb = texColor.rgb * _Color.rgb;

        return color;
    }
    ENDCG
}

    }
}

Shader "Custom/PaintShader"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _PaintTex ("Paint Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 200

        Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        #pragma surface surf Lambert

        sampler2D _MainTex;
        sampler2D _PaintTex;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_PaintTex;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
            fixed4 paint = tex2D(_PaintTex, IN.uv_PaintTex);

            // Multiply the main texture color with the paint texture color
            c *= paint;

            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
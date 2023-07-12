Shader "TOZ/Object/Splat/SplatColors/Unlit" {
	Properties {
		_MaskTex("Mask Texture", 2D) = "red" {}
		_Splat0("Layer 0 (R)", 2D) = "red" {}
		_Splat1("Layer 1 (G)", 2D) = "green" {}
		_Splat2("Layer 2 (B)", 2D) = "blue" {}
		_Splat3("Layer 3 (A)", 2D) = "black" {}
	}

	SubShader {
		Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
		LOD 100

		Pass {
			Name "BASE"
			Tags { "LightMode" = "Always" }

			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#pragma multi_compile_instancing

			UNITY_INSTANCING_BUFFER_START(Props)
				UNITY_DEFINE_INSTANCED_PROP(sampler2D, _MaskTex)
			UNITY_INSTANCING_BUFFER_END(Props)
			//sampler2D _MaskTex;
			sampler2D _Splat0, _Splat1, _Splat2, _Splat3;
			uniform float4 _Splat0_ST, _Splat1_ST, _Splat2_ST, _Splat3_ST;

			struct a2v {
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f {
				float4 pos : SV_POSITION;
				fixed4 coord : TEXCOORD0;
				float4 coord0 : TEXCOORD1;
				float4 coord1 : TEXCOORD2;
				UNITY_FOG_COORDS(3)
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f vert(a2v v) {
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.pos = UnityObjectToClipPos(v.vertex);
				o.coord.xy = v.texcoord.xy;
				o.coord0.xy = v.texcoord.xy * _Splat0_ST.xy + _Splat0_ST.zw;
				o.coord0.zw = v.texcoord.xy * _Splat1_ST.xy + _Splat1_ST.zw;
				o.coord1.xy = v.texcoord.xy * _Splat2_ST.xy + _Splat2_ST.zw;
				o.coord1.zw = v.texcoord.xy * _Splat3_ST.xy + _Splat3_ST.zw;
				UNITY_TRANSFER_FOG(o, o.pos);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target {
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				fixed4 mask = tex2D(UNITY_ACCESS_INSTANCED_PROP(Props, _MaskTex), i.coord.xy);
				fixed4 result;
				result = mask.r * tex2D(_Splat0, i.coord0.xy);
				result += mask.g * tex2D(_Splat1, i.coord0.zw);
				result += mask.b * tex2D(_Splat2, i.coord1.xy);
				result += mask.a * tex2D(_Splat3, i.coord1.zw);
				UNITY_APPLY_FOG(i.fogCoord, result);
				UNITY_OPAQUE_ALPHA(result.a);
				return result;
			}
			ENDCG 
		}
	}

	Fallback "VertexLit"
}
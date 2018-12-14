﻿Shader "Custom/uv" {
	Properties {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_FlowSpeed("Flow Speed", float) = 1
	}
	SubShader {
		Tags { "RenderType"="Opaque" }

		CGPROGRAM
		#pragma surface surf Standard

		sampler2D _MainTex;
		float _FlowSpeed;

		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutputStandard o) {
			//fixed4 c = tex2D (_MainTex, IN.uv_MainTex + _Time.y);
			//fixed4 c = tex2D (_MainTex, float2(IN.uv_MainTex.x + _Time.y, IN.uv_MainTex.y));
			fixed4 c = tex2D (_MainTex, float2(IN.uv_MainTex.x, IN.uv_MainTex.y  + _Time.g * _FlowSpeed));
			o.Emission = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}

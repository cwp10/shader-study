Shader "Custom/holo" {
	Properties {
		_BumpMap ("NormalMap", 2D) = "white" {}
		_RimColor ("RimColor", Color) = (1,1,1,1)
		_RimPower ("RimPower", Range(1, 10)) = 3
		_FlowSpeed ("Flow Speed", float) = 1
		_LineInterval ("_Line Interval", Range(1, 30)) = 10
	}
	SubShader {
		Tags { "RenderType"="Transparent" "Opaque"="Transparent" }

		CGPROGRAM
		#pragma surface surf nolight noambient alpha:fade

		sampler2D _BumpMap;
		float4 _RimColor;
		float _RimPower;
		float _FlowSpeed;
		float _LineInterval;

		struct Input {
			float2 uv_BumpMap;
			float3 viewDir;
			float3 worldPos;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			o.Normal = UnpackNormal(tex2D (_BumpMap, IN.uv_BumpMap));
			o.Emission = _RimColor;
			float rim = saturate(dot(o.Normal, IN.viewDir));
			rim = saturate(pow(1 - rim, _RimPower) + pow(frac(IN.worldPos.g * _LineInterval - _Time.y * _FlowSpeed), 5) * 0.1);
			o.Alpha = rim;
		}

		float4 Lightingnolight(SurfaceOutput s, float3 lightDir, float atten) {
			return float4(0,0,0,s.Alpha);
		}
		ENDCG
	}
	FallBack "Transparent/Diffuse"
}

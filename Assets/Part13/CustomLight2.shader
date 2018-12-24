Shader "Custom/CustomLight2" {
	Properties {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_BumpMap ("NormalMap", 2D) = "bump" {}
		_GlossTex ("Gloss Tex", 2D) = "white" {}
		_SpecCol ("Specular Color", Color) = (1,1,1,1)
		_SpecPow ("Specular Power", Range(10, 200)) = 100
		_SpecCol2 ("Specular Color2", Color) = (1,1,1,1)
		_SpecPow2 ("Specular Power2", Range(10, 200)) = 100
		_RimCol ("Rim Color", Color) = (1,1,1,1)
		_RimPow ("Rim Power", Range(1,10)) = 3
	}
	SubShader {
		Tags { "RenderType"="Opaque" }

		CGPROGRAM
		#pragma surface surf Test

		sampler2D _MainTex;
		sampler2D _BumpMap;
		sampler2D _GlossTex;
		float4 _SpecCol;
		float _SpecPow;
		float4 _SpecCol2;
		float _SpecPow2;
		float4 _RimCol;
		float _RimPow;

		struct Input {
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float2 uv_GlossTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
			float4 m = tex2D (_GlossTex, IN.uv_GlossTex);
			o.Albedo = c.rgb;
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
			o.Gloss = m.a;
			o.Alpha = c.a;
		}

		float4 LightingTest (SurfaceOutput s, float3 lightDir, float3 viewDir, float atten) {
			float4 final;

			// Lambert term
			float3 DiffColor;
			float ndotl = saturate(dot(s.Normal, lightDir));
			DiffColor = ndotl * s.Albedo * _LightColor0.rgb * atten;

			// Spec term
			float3 SpecColor;
			float3 H = normalize(lightDir + viewDir);
			float spec = saturate(dot(H,s.Normal));
			spec = pow(spec, _SpecPow);
			SpecColor = spec * _SpecCol.rgb * s.Gloss;

			// Rim term
			float3 rimColor;
			float rim = abs(dot(viewDir,s.Normal));
			float invrim = 1-rim;
			rimColor = pow(invrim,_RimPow) * _RimCol.rgb;

			// fake Spec term
			float3 SpecColor2;
			SpecColor2 = pow(rim,_SpecPow2) * _SpecCol2.rgb * s.Gloss;

			// final term
			final.rgb = DiffColor + SpecColor.rgb + rimColor.rgb + SpecColor2.rgb;
			final.a = s.Alpha;
			return final;
		}

		ENDCG
	}
	FallBack "Diffuse"
}

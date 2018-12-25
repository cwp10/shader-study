Shader "Custom/warp2" {
	Properties {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_BumpMap ("NormalMap", 2D) = "bump" {}
		_RampTex ("RampTex", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }

		cull back

		CGPROGRAM
		#pragma surface surf warp noambient

		sampler2D _MainTex;
		sampler2D _BumpMap;
		sampler2D _RampTex;

		struct Input {
			float2 uv_MainTex;
			float2 uv_BumpMap;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}

		float4 Lightingwarp (SurfaceOutput s, float3 lightDir, float3 viewDir, float atten) {
			float3 H = normalize(lightDir + viewDir);
			float spec = saturate(dot(s.Normal, H));
			float ndotl = dot(s.Normal, lightDir) * 0.5 + 0.5;
			float4 ramp = tex2D(_RampTex, float2(ndotl, spec));

			float4 final;

			final.rgb = (s.Albedo.rgb * ramp.rgb) + (ramp.rgb * 0.1);
			final.a = s.Alpha;

			return final;
		}

		ENDCG
	}
	FallBack "Diffuse"
}

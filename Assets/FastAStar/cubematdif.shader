Shader "Custom/cubematdif" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Alpha("Alpha", Range (0.0, 1)) = 0.5
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		//Cull Off
		ZWrite On
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert alpha

		sampler2D _MainTex;
		half _Alpha;

		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			half4 c = tex2D (_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			o.Alpha = _Alpha;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}

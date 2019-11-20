Shader "Custom/HexMapSurfaceShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Main Texture (A)", 2D) = "white" {}
		_TextureB ("Texture B", 2D) = "white" {}
		_GradientAB ("Gradient from A to B", Range(0,1)) = 1.0
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 300

		Name "FORWARD"
		//Tags { "LightMode" = "ForwardBase" }
		//LOD 200

		Cull Back
    ZWrite On ZTest LEqual
    Blend SrcAlpha OneMinusSrcAlpha // Traditional transparency

		CGPROGRAM
		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows
		//#pragma multi_compile_fwdbase
		//#pragma multi_compile_fwdadd_fullshadows
		//#pragma shader_feature _ALPHABLEND_ON
		//#pragma shader_feature _ALPHATEST_ON



		sampler2D _MainTex;
		sampler2D _TextureB;

		struct Input {
			float2 uv_MainTex;
			float2 uv2_TextureB;
			float4 color : COLOR;
			float3 worldPos;
		};

		half _Glossiness;
		half _Metallic;
		half _GradientAB;
		fixed4 _Color;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			float g = IN.uv2_TextureB.x * _GradientAB * 1.3f;
			//float g = _GradientAB;
			// Albedo comes from a texture tinted by color
			fixed4 a = tex2D (_MainTex, IN.uv_MainTex) * _Color * g;
			fixed4 b = tex2D (_TextureB, IN.uv_MainTex) * _Color * ( 1.0 - g);

			fixed4 c = a + b;
			o.Albedo = c.rgb * IN.color;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			//o.Alpha = c.a;
			float camDistance = distance(_WorldSpaceCameraPos,IN.worldPos);
			if ( camDistance < 1 ) {
				clip(-1);
				//o.Alpha = 0.2f;
			} else
			if ( camDistance > 15000 ) {
				//o.Alpha = 0.2f;
				clip(-1);
			} else {
				o.Alpha = 1;
			}

		}


		ENDCG
	}
	FallBack "Diffuse"
}

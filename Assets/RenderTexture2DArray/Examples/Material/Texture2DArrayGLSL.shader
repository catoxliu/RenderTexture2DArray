// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Texture2DArrayGLSL" {
	Properties
	{
		_MyArr("Tex", 2DArray) = "" {}
		_SliceRange("Slices", Range(0,1)) = 0
	}
		SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		Pass
	{
		GLSLPROGRAM

		#ifdef VERTEX
			in vec4 _glesVertex;
			in vec4 _glesMultiTexCoord0;
			out vec3 TextureCoordinate;
			uniform float _SliceRange;

			void main()
			{
				gl_Position = gl_ModelViewProjectionMatrix * _glesVertex;
				TextureCoordinate.xy = (_glesVertex.xy + 0.5);
				TextureCoordinate.z = _SliceRange;
			}

		#endif

		#ifdef FRAGMENT
			uniform highp sampler2DArray _MyArr;
			in vec3 TextureCoordinate;
			layout(location = 0) out mediump vec4 _glesFragColor;

			void main()
			{
				vec4 color = texture(_MyArr, TextureCoordinate);
				_glesFragColor = color;
			}
		#endif

		ENDGLSL
	}
	}

}

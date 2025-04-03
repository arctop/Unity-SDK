Shader "QaCircleShader"
{
	Properties
	{
		_ringNumber("ringNumber", Int) = 0
		_dotNumber("dotNumber", Int) = 0
		[HideInInspector]_objectOffset("objectOffset", Int) = 0
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Off
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma multi_compile_instancing
		#pragma surface surf Unlit keepalpha noshadow noambient novertexlights nolightmap  nodynlightmap nodirlightmap nofog 
		struct Input
		{
			float3 worldPos;
		};

		uniform int _ringNumber;
		uniform int _dotNumber;
		uniform float4 CurrentColorsArray[40];

		UNITY_INSTANCING_BUFFER_START(QaCircleShader)
			UNITY_DEFINE_INSTANCED_PROP(int, _objectOffset)
#define _objectOffset_arr QaCircleShader
		UNITY_INSTANCING_BUFFER_END(QaCircleShader)

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float temp_output_20_0 = ( ( distance( float3(0,0,0) , ase_vertex3Pos ) * 2.0 ) * _ringNumber );
			int _objectOffset_Instance = UNITY_ACCESS_INSTANCED_PROP(_objectOffset_arr, _objectOffset);
			clip( ( 1.0 - step( (float)_ringNumber , temp_output_20_0 ) ) - 1.0);
			o.Emission = CurrentColorsArray[(int)( ( floor( temp_output_20_0 ) * _dotNumber ) + _objectOffset_Instance )].rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
}
Shader "Custom/UnlitNonLinearGradientShader"
{
	Properties
	{
		_TopColor("Top Color", Color) = (1, 0, 0, 1)
		_BottomColor("Bottom Color", Color) = (0, 0, 1, 0.5)
		_GradientPower("Gradient Power", Range(0.1, 5.0)) = 1.0
	}

		SubShader
	{
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		LOD 100

		// ����͸���Ȼ��
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
	{
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"

		struct appdata
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
	};

	struct v2f
	{
		float2 uv : TEXCOORD0;
		float4 vertex : SV_POSITION;
	};

	fixed4 _TopColor;
	fixed4 _BottomColor;
	float _GradientPower;

	v2f vert(appdata v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.uv;
		return o;
	}

	fixed4 frag(v2f i) : SV_Target
	{
		// ʹ�� pow() �������з����Բ�ֵ
		float t = pow(i.uv.y, _GradientPower);

	// ���Բ�ֵ�����͵ײ���ɫ��������͸����
	fixed4 color = lerp(_BottomColor, _TopColor, t);
	return color;
	}
		ENDCG
	}
	}

		Fallback "Transparent/Diffuse"
}

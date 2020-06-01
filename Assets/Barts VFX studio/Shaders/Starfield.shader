Shader "Starfield"
{
	Properties
	{
		_iMouse("iMouse", Vector) = (0,0,0,0)
		_Iterations("Iterations", int) = 17
		_Formuparam("Formuparam", float) = .53
		_Steps("Steps", int) = 20
		_StepSize("StepSize", float) = .1
		_Zoom("Zoom", float) = .8
		_Tile("Tile", float) = .85
		_Speed("Speed", float) = .010
		_Brightness("Brightness", float) = .0015
		_DarkMatter("DarkMatter", float) = .300
		_DistFading("DistFading", float) = .730
		_Saturation("Saturation", float) = .850
	}

		SubShader
	{

		Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }

		Pass
		{

			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct VertexInput {
			fixed4 vertex : POSITION;
			fixed2 uv : TEXCOORD0;
			fixed4 tangent : TANGENT;
			fixed3 normal : NORMAL;
			//VertexInput
			};


			struct VertexOutput {
			fixed4 pos : SV_POSITION;
			fixed2 uv : TEXCOORD0;
			//VertexOutput
			};

			//Variables
			float4 _iMouse;
			int _Iterations;
			int _Steps;
			float _Formuparam;
			float _StepSize;
			float _Zoom;
			float _Tile;
			float _Speed;
			float _Brightness;
			float _DarkMatter;
			float _DistFading;
			float _Saturation;


			VertexOutput vert(VertexInput v)
			{
				VertexOutput o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			fixed4 frag(VertexOutput i) : SV_Target
			{

				//get coords and direction
				fixed2 uv = i.uv / 1 - .5;
				uv.y *= 1 / 1;
				fixed3 dir = fixed3(uv*_Zoom,1.);
				fixed time = _Time.y*_Speed + .25;

				//mouse rotation
				fixed a1 = .5 + _iMouse.x / 1 * 2.;
				fixed a2 = .8 + _iMouse.y / 1 * 2.;
				fixed2x2 rot1 = fixed2x2(cos(a1),sin(a1),-sin(a1),cos(a1));
				fixed2x2 rot2 = fixed2x2(cos(a2),sin(a2),-sin(a2),cos(a2));
				//dir.xz*=rot1;
				//dir.xy*=rot2;
				fixed3 from = fixed3(1.,.5,0.5);
				from += fixed3(-time * 2.,-time,-2.);
				//from.xz*=rot1;
				//from.xy*=rot2;

				//volumetric rendering
				fixed s = 0.6,fade = 1.;
				fixed3 v = fixed3(0.,0.,0.);
				for (int r = 0; r < _Steps; r++)
				{
					fixed3 p = from + s * dir*.5;
					p = abs(fixed3(_Tile,_Tile,_Tile) - fmod(p,fixed3(_Tile*2.,_Tile*2.,_Tile*2.))); // tiling fold
					fixed pa,a = pa = 0.;
					for (int i = 0; i < _Iterations; i++)
					{
						p = abs(p) / dot(p,p) - _Formuparam; // the magic formula
						a += abs(length(p) - pa); // absolute sum of average change
						pa = length(p);
					}
					float dm = max(0.,_DarkMatter - a * a*.001); //dark matter
					a *= a * a; // add contrast
					if (r > 6) fade *= 1. - dm; // dark matter, don't render near
					//v+=vec3(dm,dm*.5,0.);
					v += fade;
					v += fixed3(s,s*s,s*s*s*s)*a*_Brightness*fade; // coloring based on distance
					fade *= _DistFading; // distance fading
					s += _StepSize;
				}
				v = lerp(length(v),v,_Saturation); //color adjust
				fixed4 fragColor = float4(v*.01,1.);
				return fragColor;
			}

			ENDCG
		}
	}
}
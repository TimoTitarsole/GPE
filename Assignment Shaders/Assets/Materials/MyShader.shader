Shader "Unlit/Yejesadf"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MaskTex("Alpha Mask Texture", 2D) = "white" {}

        _DiffuseColor ("Diffuse Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_SpecularColor ("Specular Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_Shininess ("Shininess", Float) = 10
    }
    SubShader
    {
        Tags {  "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" "LightMode" = "ForwardBase"}
        LOD 100

        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            uniform float4 _LightColor0; 
			uniform	float4 _DiffuseColor;
			uniform	float4 _SpecularColor;
			uniform	float _Shininess;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
				float4 color : COLOR0;
            };

            sampler2D _MainTex;
            sampler2D _MaskTex;

            float4 _MainTex_ST;
            float4 _MaskTex_ST;


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                // movement
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.vertex.y += sin(worldPos.x + _Time.w);

                // normals
                float3 normalDir = normalize(mul(unity_ObjectToWorld, v.normal));//normal direction in world space for light calc
				float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);//Pos0 gives the direction vector of the first directional light in the scene
				float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, v.vertex));//camera direction relative to the vertex

                // diffuse
				float NdotL = max(0.0, dot(normalDir, lightDir));
				float3 diffuse = _DiffuseColor * _LightColor0.rgb * NdotL;
			
				// reflection of LightDir by the normal
				float3 reflectedDir = reflect(-lightDir, normalDir);
				float rv = max(0.0, dot(reflectedDir, viewDir));
				
				float specularAmount = pow(rv, _Shininess);

				float3 specularLight = _SpecularColor.rgb * _LightColor0.rgb * specularAmount;
                
                // uv
                o.uv = TRANSFORM_TEX(v.uv, _MaskTex);


				o.color = float4(diffuse + specularLight, 1);

                return o;
            }
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 mask = tex2D(_MaskTex, i.uv);
                col *= i.color;
                col.a = mask.g;
                return col;
            }
            ENDCG
        }
    }
}

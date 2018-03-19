// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/ShaderForNPC" {
	Properties {
	 	_Color ("Main Color", color) = (1.0,1.0,1.0,1.0)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_AmbientAdd ("Ambient Intensity", Vector) = (0,1,0,0)                       //Range (0, 3)
        _LightDiffScale ("Light Diff Intensity", Float) = 0.6                  //Range (0, 3)
		//_LightSpecScale ("Light Spec Intensity", Float) = 1.0                  //Range (0, 3)
		//_Shininess ("Shininess", Float) = 0.078125   
        [HideInInspector] _FinalColorScale ("Final Color Scale", Float) = 1.0  
		_FinalColorFactor ("Final Color Factor", Float) = 1.0  
		
		_Cutoff("Alpha Cutoff",float) = 0.5
		
		_RimColor("Rim Color", color) = (0,0,0,0)
        _RimPower("Rim Power", Range(0.1,3) ) = 3
		
        _IceTex("Ice Texture", 2D) = "white" {}
        _IceEfFadingProgress("IceEf Fading Progress", Range(0, 1)) = 0.0
        _Lum("Luminace", float) = 20.0
		
		_Amount ("Amount", Range (0, 1)) = 0
		_DissolveInfo ("StartAmount, Illuminate, Tile, Power", Vector) = (0.1, 0.2, 1, 0)
		_DissolveColor ("Dissolve Color", color) = (1,1,1,1)
		_DissolveSrc ("Dissolve Src", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" "Queue"="AlphaTest+3" }
		LOD 950
		
		Pass
        {
            Tags {"LightMode"="ForwardBase"}

			CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #pragma target 3.0
            #pragma multi_compile RIMLIGHT_OFF  RIMLIGHT_ON
            #pragma multi_compile IceEffect_OFF IceEffect_ON   
			#pragma multi_compile ALPHATEST_OFF ALPHATEST_ON
            #pragma multi_compile DissolveEffect_OFF DissolveEffect_ON  


    		fixed4      _Color;
            sampler2D   _MainTex;

    		fixed4       _AmbientAdd;
            fixed       _LightDiffScale;
			//fixed       _LightSpecScale;
 			//fixed       _Shininess;


            half        _FinalColorScale;
			half        _FinalColorFactor;

#if RIMLIGHT_ON
            fixed 		_RimPower;
            fixed4 		_RimColor;
#endif

#if IceEffect_ON
            sampler2D   _IceTex;
            fixed       _Lum;
            fixed       _IceEfFadingProgress;
#endif         

#if ALPHATEST_ON     
     fixed _Cutoff;
#endif
			
#if DissolveEffect_ON			
			fixed		_Amount;
			fixed4		_DissolveInfo;
			fixed4		_DissolveColor;
			sampler2D	_DissolveSrc;
#endif			
	
            struct vertexOutput {
                float4 pos      : SV_POSITION;
                float2 uv_MainTex: TEXCOORD0;
                float3 viewDir  : TEXCOORD1;
                float3 lightDir : TEXCOORD2;
                float3  vlight  : TEXCOORD3;
				float3  normal  : TEXCOORD4;
  	        };


 			uniform float4 _MainTex_ST;
            vertexOutput vert(appdata_tan v) 
            {
                vertexOutput o;
                
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
  
                //TANGENT_SPACE_ROTATION;
				o.normal = v.normal;
 				//o.viewDir = mul(rotation, ObjSpaceViewDir( v.vertex ));
                //o.lightDir = mul(rotation, ObjSpaceLightDir(v.vertex));

				o.viewDir = ObjSpaceViewDir( v.vertex );
                o.lightDir = ObjSpaceLightDir(v.vertex);
                o.uv_MainTex = TRANSFORM_TEX(v.texcoord, _MainTex);
                
 				fixed3 worldN = UnityObjectToWorldNormal(v.normal);
				//o.vlight = 0;
				//o.vlight = ShadeSHPerVertex (worldN, o.vlight);
				o.vlight = ShadeSH9(half4(worldN, 1.0));
				
                return o;
            }

            fixed4 frag(vertexOutput input):COLOR 
            {

				fixed4 outcolor = fixed4(0.0, 0.0, 0.0,1.0);
  
  				half3 wn = normalize(input.normal);
  
                half4 diffuseTexColor = tex2D(_MainTex, input.uv_MainTex);
				
				half4 diffuseColor = diffuseTexColor *  _Color;
 
 #if ALPHATEST_ON       
				clip ( diffuseColor.a - _Cutoff);
#endif

             	outcolor.rgb = (input.vlight * _AmbientAdd.x + _AmbientAdd.y) * diffuseColor.xyz;

            	//fixed3 viewDir = normalize(input.viewDir);
            
				float len = length(input.lightDir);
              	fixed3 lightDir = (len == 0) ? fixed3(0,0,0) : (input.lightDir / len);

				//half3 h = normalize (lightDir + viewDir);
	
				fixed diff = max (0, dot (wn, lightDir)) * _LightDiffScale;
	
				//float nh = max (0, dot (wn, h));
	
				//_Shininess = clamp(_Shininess * 128, 0.0001, 128);
				//float spec = pow (nh,  _Shininess) * _LightSpecScale;// * s.Gloss;
	 
				outcolor.rgb  += (diffuseColor.rgb * _LightColor0.rgb * diff);
			
				//outcolor.a     = diffuseColor.a;
  
  				outcolor.rgb  += outcolor.rgb * ( _FinalColorScale - 1 ) * _FinalColorFactor;
 
#if  IceEffect_ON
                fixed3 ice = tex2D(_IceTex, input.uv_MainTex).rgb;

                fixed gray = dot( fixed3(0.299,0.587,0.114), outcolor.rgb );
 
                outcolor.rgb = lerp(outcolor.rgb, ice.rgb * saturate( gray * _Lum ), saturate((input.uv_MainTex.x - 1) + _IceEfFadingProgress * 2));
#endif

#if RIMLIGHT_ON
                float rim =  1 - saturate(dot( normalize(input.viewDir), wn));
                fixed powrim = pow(rim, _RimPower);

                outcolor.rgb = lerp( outcolor.rgb, _RimColor.rgb, _RimColor.a * powrim );
#endif  

#if DissolveEffect_ON
				fixed ClipTex = tex2D (_DissolveSrc, input.uv_MainTex/_DissolveInfo.z).r;
				fixed ClipAmount = ClipTex - _Amount;
				if (_Amount > 0)
				{
					if (ClipAmount < 0)
					{
						clip(-1);
					}
					else if (ClipAmount < _DissolveInfo.x)
					{
						outcolor = lerp(outcolor, _DissolveColor * _DissolveInfo.y, pow((1-ClipAmount/_DissolveInfo.x), _DissolveInfo.w));
					}
				}
#endif				
	
             	return outcolor;
            }
  			ENDCG
		}
	}
	
	SubShader {
		Tags { "RenderType"="Opaque" "Queue"="AlphaTest+2" }
		LOD 200

		Pass
        {
            Tags {"LightMode"="ForwardBase"}

			CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Lighting.cginc" 

    		fixed4      _Color;
            sampler2D   _MainTex;
    		half4       _AmbientAdd;
            fixed       _LightDiffScale;
            half        _FinalColorScale;			
			
            struct vertexOutput {
                float4 pos      : SV_POSITION;
                float2 uv_MainTex: TEXCOORD0;
                float3 normal  : TEXCOORD1;
                float3 lightDir : TEXCOORD2;
                float3 vlight   : TEXCOORD3;
  	        };


 			uniform float4 _MainTex_ST;
            vertexOutput vert(appdata_tan v) 
            {
                vertexOutput o;
                
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                o.lightDir = ObjSpaceLightDir(v.vertex);
                o.uv_MainTex = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.normal = v.normal.xyz;
 				fixed3 worldN = UnityObjectToWorldNormal(v.normal);
				o.vlight = ShadeSH9 (float4(worldN,1.0));
                
                return o;
            }

            fixed4 frag(vertexOutput input):COLOR 
            {

				fixed4 outcolor = fixed4(0.0, 0.0, 0.0,1.0);
				half3 wn = normalize(input.normal);
                half4 diffuseTexColor = tex2D(_MainTex, input.uv_MainTex);
				half4 diffuseColor = diffuseTexColor *  _Color;
 
             	outcolor.rgb = (input.vlight * _AmbientAdd.x + _AmbientAdd.y) * diffuseColor.xyz;
				float len = length(input.lightDir);
              	fixed3 lightDir = (len == 0) ? fixed3(0,0,0) : (input.lightDir / len);
				fixed diff = max (0, dot (wn, lightDir)) * _LightDiffScale;
				outcolor.rgb  += (diffuseColor.rgb * _LightColor0.rgb * diff);
			
				//outcolor.a     = diffuseColor.a;
  
  				outcolor.rgb  *=_FinalColorScale;  				
	
             	return outcolor;
            }
  			ENDCG
		}
		
	}

	CustomEditor "ShaderForNPCMaterialEditor"
	FallBack "Diffuse"
}

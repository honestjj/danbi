Shader "Danbi/PanoramicCustom" {
Properties {
    
     _Tex ("Spherical Panorama", 2D) = "grey" {}
    
}

SubShader {
    Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
    Cull Off
    //ZWrite Off

    Pass {

        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
    
       

        #include "UnityCG.cginc"

        sampler2D _Tex;
       
        float4 _centerOfMesh; // set within the script (Material.SetVector("_centerOfMesh", position);

        inline float2 ToPanoramaUV(float3 xyz)
        {
            float3 normalizedxyz = normalize(xyz);
            float latitude = asin(normalizedxyz.y); // -pi/2 ~ pi/2 ==> -1/2 ~ 1/2 => 
            float longitude = atan2(normalizedxyz.z, normalizedxyz.x); // -pi ~ pi
            float v = latitude / UNITY_PI + 0.5; // 0 ~~1
            float u = longitude / UNITY_PI * 0.5 + 0.5;
            
            return float2(u,v);
        }

      

        struct appdata_t {
            float4 vertex : POSITION;
            
        };

        struct v2f {
            float4 vertex : SV_POSITION;
            float3 texcoord : TEXCOORD0;

        };

        v2f vert (appdata_t v)
        {
            v2f o;
            
            o.vertex = UnityObjectToClipPos(v.vertex);
            float3 globalPos = mul(unity_ObjectToWorld, v.vertex).xyz;
            o.texcoord = globalPos -  _centerOfMesh.xyz;

            return o;
        }

        fixed4 frag (v2f i) : SV_Target
        {
            float2 uv = ToPanoramaUV(i.texcoord);   // i.texcoord = local position within triangle      
            half4 tex = tex2D (_Tex, uv);
           
            return tex; ;
        }
        ENDCG
    }
}


Fallback Off

}

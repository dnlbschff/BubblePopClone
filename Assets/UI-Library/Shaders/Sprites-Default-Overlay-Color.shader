// Sprite-Shader for applying "Overlay" blend mode to _Color and underlying texture

Shader "DB/Sprites/Default-Overlay-Color"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex SpriteVert
            #pragma fragment SpriteFragOverlay
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"
            
            fixed4 SpriteFragOverlay(v2f IN) : SV_Target
            {
                fixed4 texSample = SampleSpriteTexture (IN.texcoord);
                fixed4 c =
                    (texSample > 0.5) * (1 - (1-2*(texSample-0.5)) * (1 - IN.color)) + 
                    (texSample <= 0.5) * ((2*texSample) * IN.color);
                c.a = texSample.a * IN.color.a;
                c.rgb *= c.a;
                return c;
            }
        ENDCG
        }
    }
}

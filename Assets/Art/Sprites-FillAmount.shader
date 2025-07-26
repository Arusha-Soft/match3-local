Shader "Sprites/Fill Amount"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        [HideInInspector] _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
        _FillAmount ("FillAmount", Range(0, 1)) = 1
        [MaterialToggle] _Reverse ("Reverse", Range(0, 1)) = 0
        [MaterialToggle] _Vertical ("Vertical", Range(0, 1)) = 0
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
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"

            float _FillAmount;
            float _Reverse;
            float _Vertical;

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = SampleSpriteTexture (IN.texcoord) * IN.color;
                c.rgb *= c.a;
                clip(
                    lerp(
                        lerp(
                            1 - IN.texcoord.x - (1 - _FillAmount),
                            IN.texcoord.x - (1 - _FillAmount),
                            _Reverse
                        ),
                        lerp(
                            1 - IN.texcoord.y - (1 - _FillAmount),
                            IN.texcoord.y - (1 - _FillAmount),
                            _Reverse
                        ),
                        _Vertical
                    )
                );
                return c;
            }
        ENDCG
        }
    }
}

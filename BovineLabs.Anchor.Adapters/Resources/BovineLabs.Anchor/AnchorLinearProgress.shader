Shader "Hidden/BovineLabs/Anchor/LinearProgress"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Rounded ("Rounded", Int) = 1
        _Start ("Start", Float) = 0
        _End ("End", Float) = 0
        _BufferStart ("Buffer Start", Float) = 0
        _BufferEnd ("Buffer End", Float) = 0
        _BufferOpacity ("Buffer Opacity", Float) = 0.1
        _AA ("Anti-Aliasing", Float) = 0.005
        _Phase ("Phase", Vector) = (0,0,0,0)
        _Ratio ("Ratio", Float) = 1
        _Padding ("Padding", Float) = 0
        _Vertical ("Vertical", Int) = 0
        _Reverse ("Reverse", Int) = 0
        _MaskTexture ("Mask Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_local __ ANCHOR_PROGRESS_INDETERMINATE
            #include "UnityCG.cginc"

            struct Attributes
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 vertex : SV_POSITION;
                float2 progressUv : TEXCOORD0;
                float2 maskUv : TEXCOORD1;
            };

            float _Ratio;
            float _Padding;
            int _Vertical;
            int _Reverse;

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.vertex = UnityObjectToClipPos(input.vertex);

                float2 orientedUv = _Vertical ? input.uv.yx : input.uv;
                orientedUv.x = _Reverse ? 1.0 - orientedUv.x : orientedUv.x;
                output.progressUv = float2(
                    orientedUv.x / (1.0 - _Padding * 2.0) - _Padding,
                    (orientedUv.y - 0.5) / _Ratio);
                output.maskUv = input.uv;
                return output;
            }

            int _Rounded;
            float _Start;
            float _End;
            float _BufferStart;
            float _BufferEnd;
            float _BufferOpacity;
            half4 _Color;
            float _AA;
            float4 _Phase;
            sampler2D _MaskTexture;

            float Circle(float2 uv, float2 position, float radius)
            {
                return 1.0 - smoothstep(radius, radius + _AA, length(uv - position));
            }

            half4 Frag(Varyings input) : SV_Target
            {
                #if ANCHOR_PROGRESS_INDETERMINATE
                const float duration = 1.0;
                const float time = fmod(_Phase.y / duration, 1.0);
                _Start = time < 0.5 ? lerp(0.0, 0.15, time / 0.5) :
                    time < 0.75 ? lerp(0.15, 0.2, (time - 0.5) / 0.25) : lerp(0.2, 0.99, (time - 0.75) / 0.25);
                _End = time < 0.5 ? lerp(0.0, 0.8, time / 0.5) :
                    time < 0.65 ? lerp(0.8, 0.85, (time - 0.5) / 0.15) :
                    time < 0.8 ? lerp(0.85, 1.0, (time - 0.65) / 0.15) : 1.0;
                #endif

                const float progress = input.progressUv.x;
                const float radius = 1.0 / _Ratio * 0.5;
                float valueMask = progress >= _Start && progress <= _End ? 1.0 : 0.0;
                valueMask = max(valueMask, Circle(input.progressUv, float2(_Start, 0), radius) * _Rounded);
                valueMask = max(valueMask, Circle(input.progressUv, float2(_End, 0), radius) * _Rounded);

                half4 color = half4(_Color.rgb, valueMask);

                #ifndef ANCHOR_PROGRESS_INDETERMINATE
                float bufferMask = progress >= _BufferStart && progress <= _BufferEnd ? 1.0 : 0.0;
                bufferMask = max(bufferMask, Circle(input.progressUv, float2(_BufferStart, 0), radius) * _Rounded);
                bufferMask = max(bufferMask, Circle(input.progressUv, float2(_BufferEnd, 0), radius) * _Rounded);
                color.a = max(color.a, _BufferEnd > 0 ? _BufferOpacity * bufferMask : _BufferOpacity);
                #else
                color.a = max(color.a, _BufferOpacity);
                #endif

                color.a *= tex2D(_MaskTexture, input.maskUv).a;
                return color;
            }
            ENDHLSL
        }
    }
}

precision highp float;

uniform highp sampler2D inValues;
uniform lowp sampler2D coltx;
uniform highp float iterations;

varying highp vec2 c0;
varying highp vec2 t0;

void main()
{
    vec4 inVals = texture2D(inValues, t0);
    vec2 col = vec2((inVals.z >= iterations ? 0.0 : float(inVals.z)) / 256.0, 0.0);
    gl_FragColor = texture2D(coltx, col);
}

precision highp float;

uniform highp sampler2D inValues;
uniform lowp sampler2D coltx;

varying highp vec2 c0;
varying highp vec2 t0;

void main()
{
    float maxIter = 64.0;
    vec4 inVals = texture2D(inValues, t0);
    vec2 col = vec2((inVals.z >= maxIter ? 0.0 : float(inVals.z)) / maxIter, 0.0);
    gl_FragColor = texture2D(coltx, col);
}

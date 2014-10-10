precision highp float;

uniform highp float iterations;

varying highp vec2 c0;
varying highp vec2 t0;

void main()
{
    vec4 inVals = texture2D(inValues, t0);
    //vec2 col = vec2((inVals.z >= iterations ? 0.0 : inVals.z) / iterations, 0.0);

    // store difference between c0 and current z value and iteration count
    gl_FragColor.xy = z - c0;
    gl_FragColor.z  = inVals.z + i;
    gl_FragColor.w  = 1.0;
}

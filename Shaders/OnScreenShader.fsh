precision highp float;

uniform highp sampler2D approx;
uniform highp sampler2D inValues;
uniform lowp sampler2D coltx;
uniform highp float curIterations;
uniform highp float maxIterations;

varying highp vec2 c0;
varying highp vec2 t0;

void main()
{
    vec4 inVals = texture2D(inValues, t0);


    // check if done
    //vec4 approxz = texture2D(approx, t0);
    //vec2 z1 = c0 + approxz.xy;
    //vec2 z2 = z1 * z1;
    //float z = (((z2.x + z2.y) > 4.0) || inVals.z > approxz.z) ? inVals.z : approxz.z;

    //float z = approxz.z;
    float z = inVals.z;

    // fun crazy coloring A
//    if (inVals.z < maxIterations) {
//    	float zn = sqrt(inVals.x*inVals.x + inVals.y*inVals.y);
//    	float nu = log(log(zn) / log(2.0)) / log(2.0);
//    	float it = inVals.z + 1.0 - nu;
//    	gl_FragColor = texture2D(coltx, vec2(it, 0.0));
//    } else {
//    	gl_FragColor = vec4(0.0, 0.0, 0.0, 1.0);
//    }

    if (z < curIterations) {
    	gl_FragColor = texture2D(coltx, vec2(z / 100.0, 0.0));
    } else {
    	gl_FragColor = vec4(0.0, 0.0, 0.0, 1.0);
    }
}

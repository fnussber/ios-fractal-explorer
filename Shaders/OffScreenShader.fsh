precision highp float;

uniform highp sampler2D inValues;
uniform highp float iterations;

varying highp vec2 c0;
varying highp vec2 t0;

void main()
{
	// claim our last result (i.e. current z and iteration count)
    vec4 inVals = texture2D(inValues, t0);

    // init c and z accordingly
    vec2 z = c0 + inVals.xy;
 
	// calculate the next series of steps
    float i;
    for (i = 0.0; i < iterations; i++) {
        vec2 z2 = z * z;
        if((z2.x + z2.y) > 4.0) break;

        z = vec2(
        	  z2.x - z2.y,
        	  2.0 * z.y * z.x
        	) + c0;
    }

    // store difference between c0 and current z value and iteration count
    gl_FragColor.xy = z - c0;
    gl_FragColor.z  = inVals.z + i;
    gl_FragColor.w  = 1.0;

}

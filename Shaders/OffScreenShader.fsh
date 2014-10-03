precision highp float;

uniform highp sampler2D inValues;
uniform highp float steps;

varying highp vec2 c0;
varying highp vec2 t0;

void main()
{
	// claim our last result (i.e. current z and iteration count)
    vec4 inVals = texture2D(inValues, t0);

    // init c and z accordingly
    vec2 z = inVals.xy;
 
	// calculate the next series of steps
    float i;
    for (i = 0.0; i < 64.0; i++) {
        vec2 z2 = z * z;
        if((z2.x + z2.y) > 4.0) break;

        z = vec2(
        	  z2.x - z2.y,
        	  2.0 * z.y * z.x
        	) + c0;
    }

    // store current z value and iteration count
    gl_FragColor.xy = z;
    gl_FragColor.z  = inVals.z + i;
    gl_FragColor.w  = 1.0;

}

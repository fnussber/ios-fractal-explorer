precision highp float;

uniform highp sampler2D inValues;
uniform highp float steps;

varying highp vec2 c0;

void main()
{
	// claim our last result (i.e. current z and iteration count)
    vec4 inVals = texture2D(inValues, c0);

    // init c and z accordingly
    vec2 c = inVals.xy;
    vec2 z = c;
 
	// calculate the next series of steps
    float i;
    for (i = 0.0; i < steps; i++) {
        vec2 z2 = z * z;
        if((z2.x + z2.y) > 4.0) break;

        z = vec2(
        	  z2.x - z2.y,
        	  2.0 * z.y * z.x
        	) + c;
    }

    // store current z value and iteration count
//    gl_FragColor.xy = z;
//    gl_FragColor.z  = inVals.z + i;
//    gl_FragColor.w  = 1.0;

    gl_FragColor = vec4(1.0,0.0,1.5,1.0);

}

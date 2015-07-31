precision highp float;

uniform highp sampler2D preview;
uniform highp sampler2D iterate;
uniform lowp  sampler2D coltx;
uniform highp float curIterations;
uniform highp float maxIterations;

varying highp vec2 t0;

void main()
{
    vec4 iter = texture2D(iterate, t0);
    vec4 prev = texture2D(preview, t0);

    // check if done
    float i;
    if (iter.z < curIterations) {
    	// iterations have reached maximum for this point, we have the final value
    	i = iter.z;
    } else if (iter.z < prev.z) {
    	// less iterations calculated than for preview, take preview value
		i = prev.z;
	} else {
		// better than preview but still more iterations needed
		// for now assume that we'll hit the maximum
        i = maxIterations;
	}

    if (i < maxIterations) {
    	// get color for i iterations from color texture
    	gl_FragColor = texture2D(coltx, vec2(i / 100.0, 0.0));
    } else {
    	// if i is >= max iterations color is black
    	gl_FragColor = vec4(0.0, 0.0, 0.0, 1.0);
    }

}

﻿precision highp float;
precision lowp sampler2D;

uniform sampler2D coltx;
uniform float maxIter;

varying highp vec2 c;

void main()
{
    vec2 z = c;
 
    float i;
    for (i = 0.0; i <= maxIter; i++) {
        vec2 z2 = z * z;
        if((z2.x + z2.y) > 4.0) break;

        z = vec2(
        	  z2.x - z2.y,
        	  2.0 * z.y * z.x
        	) + c;
    }

    vec2 col = vec2((i >= maxIter ? 0.0 : i) / 100.0, 0);
    gl_FragColor = texture2D(coltx, col);
}

precision highp float;
precision lowp sampler2D;

uniform sampler2D coltx;
uniform float maxIter;
uniform float scale;
uniform vec2 trans;

void main()
{
    vec4 fc = gl_FragCoord * scale;
    vec2 c = vec2(trans.x + fc.x, trans.y + fc.y);
    vec2 z = vec2(c.x, c.y);
 
    float i;
    for (i = 0.0; i < maxIter; i++) {
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

uniform sampler2D input;
uniform float curIteration;
void main ()
{
	// Lookup value from last iteration
	vec4 inputValue = texture2D(input, gl_TexCoord[0].xy);
	vec2 z = inputValue.xy;
	vec2 c = gl_TexCoord[0].xy;
	
	// Only process if still within radius-2 boundary
	if (dot(z, z) > 4.0)
		// Leave pixel unchanged (but copy 
		//through to destination buffer)
		gl_FragColor = inputValue;
	else
	{
		gl_FragColor.xy = vec2(z.x*z.x - z.y*z.y, 2.0*z.x*z.y) + c;
		gl_FragColor.z = curIteration;
		gl_FragColor.w = 0.0;
	}
}

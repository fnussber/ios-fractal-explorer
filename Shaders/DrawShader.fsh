uniform sampler2D input;
uniform vec4 insideColor;
uniform sampler1D outsideColorTable;
uniform float maxIterations;
void main ()
{
	// Lookup value from last iteration
	vec4 inputValue = texture2D(input, gl_TexCoord[0].xy);
	vec2 z = inputValue.xy;
	
	// If Z has escaped radius-2 boundary, shade by outer color
	if (dot(z, z) > 4.0)
		gl_FragColor = texture1D(outsideColorTable, 
		inputValue.z / maxIterations);
	else
		gl_FragColor = insideColor;
}

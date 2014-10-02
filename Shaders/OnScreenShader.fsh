precision highp float;

uniform highp sampler2D inValues;
uniform lowp sampler2D coltx;

varying highp vec2 c0;

void main()
{
    float maxIter = 64.0;
    vec4 inVals = texture2D(inValues, vec2((c0.x+1.0)/2.0, (c0.y+1.0)/2.0));
    vec2 col = vec2((inVals.z >= maxIter ? 0.0 : float(inVals.z)) / 64.0, 0.0);
    gl_FragColor = texture2D(coltx, col);


//    if (inVals.x > 0.0) 
//      gl_FragColor = vec4(1.0, 0.0, 0.0, 1.0); //texture2D(coltx, col);
//    if (inVals.x > 0.25)
//      gl_FragColor = vec4(0.0, 1.0, 0.0, 1.0); //texture2D(coltx, col);
//    if (inVals.x > 0.5)
//      gl_FragColor = vec4(0.0, 0.0, 1.0, 1.0); //texture2D(coltx, col);

//    float vvv =  c0.x; //(inVals.x+1.5)/3.0;
//    gl_FragColor = inVals; //texture2D(coltx, vec2(0.1, 0.0));
}

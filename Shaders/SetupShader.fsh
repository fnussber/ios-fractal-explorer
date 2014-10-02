precision highp float;

varying highp vec2 c;

void main()
{
    gl_FragColor = vec4(c,0.0,1.0);
}
attribute vec4 position;

uniform highp mat4 matrix;

varying highp vec2 c0;

void main()
{
  c0 = position.xy;
  gl_Position = position;
}
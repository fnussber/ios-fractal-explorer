attribute vec4 position;
uniform highp mat4 matrix;
varying highp vec2 c;

void main()
{
  c = position.xy;
  gl_Position = matrix * position;
}

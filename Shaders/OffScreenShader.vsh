attribute highp vec4 position;
attribute highp vec4 texture;

uniform highp mat4 matrix;

varying highp vec2 c0;
varying highp vec2 t0;

void main()
{
  c0 = (matrix * position).xy;
  t0 = texture.xy;
  gl_Position = position;
}
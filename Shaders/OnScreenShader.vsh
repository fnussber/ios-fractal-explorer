attribute highp vec4 position;
attribute highp vec2 texture;

uniform highp mat4 matrix;

varying highp vec2 t0;

void main()
{
  t0 = texture.xy;
  gl_Position = position;
}
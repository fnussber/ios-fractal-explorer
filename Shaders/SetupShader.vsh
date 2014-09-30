attribute vec4 position;

varying highp vec2 c;

void main()
{
  c = position.xy;
  gl_Position = position;
}
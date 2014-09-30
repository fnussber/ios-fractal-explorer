attribute vec4 position;
uniform highp float scale;
uniform highp vec2 trans;

varying highp vec2 c;

void main()
{
    //vec4 fc = position * scale;
    //vec2 c = vec2(trans.x + fc.x, trans.y + fc.y);

  c = position.xy + trans;
  gl_Position = position;
}

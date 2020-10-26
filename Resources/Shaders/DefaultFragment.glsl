#version 330 core

uniform sampler2D tex0;

in vec2 fPos;
in vec4 fColor;
out vec4 color;

void main()
{
    color = texture(tex0, fPos) + 0.5;
}
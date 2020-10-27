#version 330 core

uniform sampler2DArray _blocks;

in vec3 texUV;
in vec4 vertexColor;

out vec4 color;

void main()
{
    color = texture(_blocks, texUV);
}
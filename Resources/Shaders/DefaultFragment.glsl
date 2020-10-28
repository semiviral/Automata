#version 330 core

uniform sampler2D _blocks;

in vec3 texUV;
in vec4 vertexColor;

out vec4 color;

void main()
{
    color =  vertexColor;// texture(_blocks, vec2(0) / textureSize(_blocks, 0));
}
#version 330 core

uniform sampler2DArray _blocks;

in flat int texIndex;
in vec2 texUV;
in vec3 vertexColor;

out vec4 color;

void main()
{
    color = texture(_blocks, vec3(texUV, texIndex)) * vec4(vertexColor, 1.0);
}
#version 330 core

uniform sampler2DArray _tex0;

in vec3 texUV;
in vec3 vertexColor;

out vec4 color;

void main()
{
    color = texture(_tex0, texUV) * vec4(vertexColor, 1.0);
}
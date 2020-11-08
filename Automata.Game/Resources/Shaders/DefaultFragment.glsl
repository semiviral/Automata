#version 330 core

uniform sampler2DArray _tex0;

in vec3 texUV;
in vec3 vertexColor;

out vec4 color;

void main()
{
    vec4 texColor = texture(_tex0, texUV);

    if (texColor.a == 0.0)
        discard;

    color = texColor * vec4(vertexColor, 1.0);
}
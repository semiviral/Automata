#version 330 core

layout (location = 0) in int vert;
layout (location = 1) in int uv;

uniform mat4 _mvp;

out vec3 texUV;
out vec4 vertexColor;

void main()
{
    vec4 uncompressedPosition = vec4((vert >> 0) & 63, (vert >> 6) & 63, (vert >> 12) & 63, 1.0);
    vec3 uncompressedNormal = vec3(((vert >> 18) & 3) - 1, ((vert >> 20) & 3) - 1, ((vert >> 22) & 3) - 1);
    vec3 uncompressedUV = vec3((uv >> 0) & 63, (uv >> 6) & 63, (uv >> 12) & 63);

    gl_Position = _mvp * uncompressedPosition;
    texUV = uncompressedUV;
    vertexColor = vec4(0.2);
}
#version 330 core

layout (location = 0) in int vert;
layout (location = 1) in int uv;

uniform mat4 _mvp;

out vec3 texUV;
out vec3 vertexColor;

void main()
{
    vec4 uncompressedPosition = vec4((vert >> 0) & 63, (vert >> 6) & 63, (vert >> 12) & 63, 1.0);
    ivec3 uncompressedNormal = ivec3(((vert >> 18) & 3) - 1, ((vert >> 20) & 3) - 1, ((vert >> 22) & 3) - 1);
    ivec3 uncompressedUV = ivec3((uv >> 0) & 63, (uv >> 6) & 63, (uv >> 12) & 63);
    vec3 smoothNormals = vec3(smoothstep(-1, 1, uncompressedNormal.x), smoothstep(-1, 1, uncompressedNormal.y), smoothstep(-1, 1, uncompressedNormal.z));

    gl_Position = _mvp * uncompressedPosition;
    texUV = uncompressedUV;
    vertexColor = vec3(1.0);//smoothNormals;
}
#version 330 core

layout (location = 0) in int vert;
layout (location = 1) in int uv;

uniform mat4 _mvp;

out vec2 fPos;
out vec4 fColor;

void main()
{
    vec4 uncompressedPosition = vec4((vert >> 0) & 63, (vert >> 6) & 63, (vert >> 12) & 63, 1.0);
    vec3 uncompressedUV = vec3((uv >> 0) & 63, (uv >> 6) & 63, (uv >> 12) & 63);

    vec3 uncompressedNormal = vec3(((vert >> 18) & 3) - 1, ((vert >> 20) & 3) - 1, ((vert >> 22) & 3) - 1);
    vec3 normalLerpedColor = vec3
        (
            smoothstep(-1.0, 1.0, uncompressedNormal.x),
            smoothstep(-1.0, 1.0, uncompressedNormal.y),
            smoothstep(-1.0, 1.0, uncompressedNormal.z)
        );

    gl_Position = _mvp * uncompressedPosition;
    fColor = vec4(normalLerpedColor, 1.0);
    fPos = uncompressedUV.xy;
}
#version 330 core

layout (location = 0) in int vert;
layout (location = 1) in int uv;

uniform int _componentMask;
uniform int _componentShift;
uniform int _normalsShift;
uniform mat4 _mvp;

out vec3 texUV;
out vec3 vertexColor;

void main()
{
    vec4 uncompressedPosition =
        vec4(
            (vert >> (_componentShift * 0)) & _componentMask,
            (vert >> (_componentShift * 1)) & _componentMask,
            (vert >> (_componentShift * 2)) & _componentMask,
            1.0
        );

    int coordinatesOffset = _componentMask * 3;
    ivec3 uncompressedNormal =
        ivec3(
            ((vert >> (coordinatesOffset + (_normalsShift * 0))) & 3) - 1,
            ((vert >> (coordinatesOffset + (_normalsShift * 1))) & 3) - 1,
            ((vert >> (coordinatesOffset + (_normalsShift * 2))) & 3) - 1
        );
    ivec3 uncompressedUV =
        ivec3(
            (uv >> (_componentShift * 0)) & _componentMask,
            (uv >> (_componentShift * 1)) & _componentMask,
            (uv >> (_componentShift * 2)) & _componentMask
        );

    vec3 smoothNormals = vec3(smoothstep(-1, 1, uncompressedNormal.x), smoothstep(-1, 1, uncompressedNormal.y), smoothstep(-1, 1, uncompressedNormal.z));

    gl_Position = _mvp * uncompressedPosition;
    texUV = uncompressedUV;
    vertexColor = vec3(1.0); //* smoothNormals;
}
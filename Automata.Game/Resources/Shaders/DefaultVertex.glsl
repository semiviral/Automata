#version 330 core

layout (location = 0) in vec3 vert;

uniform mat4 _mvp;

out vec4 vertexColor;

void main() {
    vec4 vert4 = vec4(vert, 1);
    gl_Position = _mvp * vert4;
    vertexColor = normalize(vert4);
}

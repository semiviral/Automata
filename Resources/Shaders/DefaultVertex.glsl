#version 330 core

layout (location = 0) in ivec3 vert;

uniform mat4 _mvp;

out vec4 fColor;

void main() {
    vec4 vertf = vec4(vert, 1);
    gl_Position = _mvp * vertf;
    fColor = normalize(vertf);
}

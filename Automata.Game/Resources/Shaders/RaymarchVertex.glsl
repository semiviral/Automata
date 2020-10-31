#version 330 core

layout (location = 0) in vec3 vertex;

out rayOut
{
    vec3 rayOrigin;
    vec3 rayDestination;
    vec4 screen;
} v2f;

// automata built-ins
uniform mat4 _mvp;
uniform mat4 _object;
uniform vec3 _camera;
uniform vec4 _viewport;

void main()
{
    vec4 clip = _mvp * vec4(vertex, 1.0);
    gl_Position = clip;

    v2f.rayOrigin = (_object * vec4(_camera, 1.0)).xyz;
    v2f.rayDestination = vertex;
    v2f.screen = ((clip.xy + 1.0) / 2.0) * _viewport.zw;
}
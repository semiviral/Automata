#version 450 core

uniform sampler2DArray _tex0;

layout (location = 0) in fragment
{
    vec3 uv;
    vec3 color;
} fragment;

out vec4 color;

void main()
{
    vec4 tex = texture(_tex0, fragment.uv);

    if (tex.a == 0.0)
        discard;

    color = tex * vec4(fragment.color, 1.0);
}
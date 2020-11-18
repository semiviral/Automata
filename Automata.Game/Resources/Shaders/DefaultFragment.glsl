#version 450 core

uniform sampler2DArray tex_Blocks;

layout (location = 0) in fragment
{
    vec3 uv;
    vec3 color;
} fragment;

out vec4 color;

void main()
{
    vec4 texColor = texture(tex_Blocks, fragment.uv);

    if (texColor.a == 0.0)
        discard;

    color = texColor * vec4(fragment.color, 1.0);
}
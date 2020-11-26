struct DrawCommand
{
    uint Count;
    uint InstanceCount;
    uint FirstIndex;
    uint FirstVertex;
    uint BaseInstance;
};

layout (std140, binding = 0) writeonly buffer DrawCommands
{
    DrawCommand drawCommands[];
};

layout (std430, binding = 3) buffer DrawParameters
{
    uint count;
    mat4 mvps[];
};

void main()
{
    const uint index = gl_LocalInvocationID.x;
}
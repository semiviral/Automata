using System;
using System.Diagnostics.CodeAnalysis;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.OpenGL.Buffers
{
    public class Tenant : OpenGLObject, IDisposable
    {
        private readonly ApartmentBuffer _Owner;

        public uint Index { get; }
        public uint Offset { get; }

        internal Tenant(GL gl, ApartmentBuffer owner, uint index, uint offset) : base(gl)
        {
            _Owner = owner;
            Handle = owner.Handle;
            Index = index;
            Offset = offset;
        }

        public unsafe void SetBufferData(byte* data, uint length, BufferDraw bufferDraw) =>
            GL.NamedBufferData(Handle, length, data, (VertexBufferObjectUsage)bufferDraw);

        public void SetBufferData(Span<byte> data, BufferDraw bufferDraw) =>
            GL.NamedBufferData(Handle, (uint)data.Length, data, (VertexBufferObjectUsage)bufferDraw);

        public void Dispose() => _Owner.Return(this);
    }

    public class ApartmentBuffer : OpenGLObject, IDisposable
    {
        private readonly bool[] _Slots;

        public uint SlotCount { get; }
        public uint SlotSize { get; }

        public uint RentedSlots { get; private set; }

        public ApartmentBuffer(GL gl, uint slotCount, uint slotSize) : base(gl)
        {
            const uint storage_flags = (uint)BufferStorageMask.DynamicStorageBit | (uint)MapBufferAccessMask.MapWriteBit;

            _Slots = new bool[slotCount];

            SlotCount = slotCount;
            SlotSize = slotSize;
            Handle = GL.CreateBuffer();

            uint size = slotCount * slotSize;
            GL.NamedBufferStorage(Handle, size, Span<byte>.Empty, storage_flags);
        }

        public bool TryRent([NotNullWhen(true)] out Tenant? bufferSlot)
        {
            bufferSlot = null;

            if (RentedSlots == _Slots.Length) return false;

            uint index = 0;

            for (; index < _Slots.Length; index++)
                if (!_Slots[index])
                    break;

            bufferSlot = new Tenant(GL, this, index, (uint)(index * SlotSize));
            _Slots[index] = true;
            RentedSlots += 1;
            return true;
        }

        internal unsafe void Return(Tenant tenant)
        {
            if (!_Slots[tenant.Index]) throw new ArgumentException("Slot is not rented.");

            byte zero = 0;
            GL.ClearNamedBufferData(tenant.Handle, InternalFormat.R8, PixelFormat.Red, PixelType.Byte, (void*)&zero);
            _Slots[tenant.Index] = false;
            RentedSlots -= 1;
        }

        public void Dispose() => GL.DeleteBuffer(Handle);
    }
}

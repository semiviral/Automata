using System;
using System.Diagnostics.CodeAnalysis;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.OpenGL.Buffers
{
    public class Tenant : OpenGLObject, IDisposable
    {
        private readonly ApartmentBuffer _Owner;

        private bool _Disposed;

        public uint Index { get; }
        public uint Offset { get; }

        internal Tenant(GL gl, ApartmentBuffer owner, uint index, uint offset) : base(gl)
        {
            _Owner = owner;
            Handle = owner.Handle;
            Index = index;
            Offset = offset;
        }

        public unsafe void SetBufferData(int offset, byte* data, uint length) => GL.NamedBufferSubData(Handle, offset + (int)Offset, length, data);
        public void SetBufferData(int offset, Span<byte> data) => GL.NamedBufferSubData(Handle, offset + (int)Offset, (uint)data.Length, ref data[0]);

        public void Dispose()
        {
            if (_Disposed) return;

            _Owner.Return(this);
            _Disposed = true;
        }

        ~Tenant() => Dispose();
    }

    public class ApartmentBuffer : OpenGLObject, IDisposable
    {
        private readonly bool[] _RoomTracker;

        public uint RoomCount { get; }
        public uint TenantSize { get; }

        public uint Tenant { get; private set; }

        public ApartmentBuffer(GL gl, uint roomCount, uint tenantSize) : base(gl)
        {
            const uint storage_flags = (uint)BufferStorageMask.DynamicStorageBit | (uint)MapBufferAccessMask.MapWriteBit;

            _RoomTracker = new bool[roomCount];

            RoomCount = roomCount;
            TenantSize = tenantSize;
            Handle = GL.CreateBuffer();

            uint size = roomCount * tenantSize;
            GL.NamedBufferStorage(Handle, size, Span<byte>.Empty, storage_flags);
        }

        public bool TryRent([NotNullWhen(true)] out Tenant? tenant)
        {
            tenant = null;

            if (Tenant == _RoomTracker.Length) return false;

            uint index = 0;

            for (; index < _RoomTracker.Length; index++)
                if (!_RoomTracker[index])
                    break;

            tenant = new Tenant(GL, this, index, (uint)(index * TenantSize));
            _RoomTracker[index] = true;
            Tenant += 1;
            return true;
        }

        internal unsafe void Return(Tenant tenant)
        {
            if (!_RoomTracker[tenant.Index]) throw new ArgumentException("Slot is not rented.");

            byte zero = 0;
            GL.ClearNamedBufferData(tenant.Handle, InternalFormat.R8, PixelFormat.Red, PixelType.Byte, (void*)&zero);
            _RoomTracker[tenant.Index] = false;
            Tenant -= 1;
        }

        public void Dispose() => GL.DeleteBuffer(Handle);
    }
}

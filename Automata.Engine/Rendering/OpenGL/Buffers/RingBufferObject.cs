using System;
using System.Runtime.InteropServices;
using Silk.NET.OpenGL;
using Buffer = System.Buffer;

namespace Automata.Engine.Rendering.OpenGL.Buffers
{
    public unsafe class RingBufferObject : OpenGLObject
    {
        private const MapBufferAccessMask _ACCESS_MASK = MapBufferAccessMask.MapWriteBit
                                                         | MapBufferAccessMask.MapPersistentBit
                                                         | MapBufferAccessMask.MapCoherentBit;

        private readonly Ring _Ring;
        private readonly FenceSync?[] _RingSyncs;
        private readonly byte* _Pointer;

        public nuint Size { get; }
        public uint Buffers { get; }
        public nuint BufferedSize { get; }
        public bool Written { get; private set; }

        public RingBufferObject(GL gl, nuint size, uint buffers) : base(gl)
        {
            Size = size;
            Buffers = buffers;
            BufferedSize = size * (uint)buffers;

            Handle = GL.CreateBuffer();
            GL.NamedBufferStorage(Handle, BufferedSize, Span<byte>.Empty, (uint)_ACCESS_MASK);

            _Ring = new Ring((nuint)buffers);
            _Pointer = (byte*)GL.MapNamedBuffer(Handle, BufferAccessARB.WriteOnly);
            _RingSyncs = new FenceSync[(int)buffers];
        }

        public void Write(ReadOnlySpan<byte> data)
        {
            if ((nuint)data.Length != Size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(data), "Cannot write segments into ring buffer; your data may be too small or large.");
            }
            else if (Written)
            {
                ThrowHelper.ThrowInvalidOperationException("Can only write to ring buffer once per ring.");
            }

            data.CopyTo(new Span<byte>(_Pointer + GetCurrentOffset(), (int)Size));
            Written = true;
        }

        public void Write(void* pointer)
        {
            if (Written)
            {
                ThrowHelper.ThrowInvalidOperationException("Can only write to ring buffer once per ring.");
            }

            Buffer.MemoryCopy(pointer, _Pointer + GetCurrentOffset(), Size, Size);
            Written = true;
        }

        public void Write<T>(ref T data) where T : unmanaged
        {
            if ((nuint)sizeof(T) != Size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(data), "Cannot write segments into ring buffer; your data may be too small or large.");
            }
            else if (Written)
            {
                ThrowHelper.ThrowInvalidOperationException("Can only write to ring buffer once per ring.");
            }

            MemoryMarshal.Write(new Span<byte>(_Pointer + GetCurrentOffset(), (int)Size), ref data);
            Written = true;
        }

        public void CycleRing()
        {
            // create fence for current ring
            _RingSyncs[(int)_Ring.Current] = new FenceSync(GL);

            // wait to enter next ring, then increment to it
            _RingSyncs[(int)_Ring.NextRing()]?.BusyWaitCPU();
            _Ring.Increment();

            Written = false;
        }

        private nint GetCurrentOffset() => (nint)(Size * _Ring.Current);


        #region Binding

        public void Bind(BufferTargetARB target, uint index) => GL.BindBufferRange(target, index, Handle, GetCurrentOffset(), Size);

        #endregion


        #region IDisposable

        protected override void CleanupNativeResources()
        {
            foreach (FenceSync? fenceSync in _RingSyncs)
            {
                fenceSync?.Dispose();
            }

            GL.UnmapNamedBuffer(Handle);
            GL.DeleteBuffer(Handle);
        }

        #endregion
    }
}

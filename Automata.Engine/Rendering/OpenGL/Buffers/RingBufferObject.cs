using System;
using System.Buffers;
using System.Runtime.InteropServices;
using Automata.Engine.Memory;
using Silk.NET.OpenGL;

namespace Automata.Engine.Rendering.OpenGL.Buffers
{
    public unsafe class RingBufferObject : OpenGLObject
    {
        private const MapBufferAccessMask _ACCESS_MASK = MapBufferAccessMask.MapWriteBit | MapBufferAccessMask.MapPersistentBit;

        private readonly Ring _Ring;
        private readonly FenceSync?[] _RingSyncs;
        private readonly IMemoryOwner<byte> _MemoryOwner;

        private Span<byte> RingSegment => _MemoryOwner.Memory.Span.Slice((int)GetCurrentOffset());

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
            _RingSyncs = new FenceSync[(int)buffers];
            _MemoryOwner = new NativeMemoryManager<byte>((byte*)GL.MapNamedBuffer(Handle, BufferAccessARB.WriteOnly), (int)BufferedSize);
        }

        public void Write(ReadOnlySpan<byte> data)
        {
            if ((nuint)data.Length != Size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(data), "Cannot write segments into ring buffer; your data size is too small or large.");
            }
            else if (Written)
            {
                ThrowHelper.ThrowInvalidOperationException("Can only write to ring buffer once per ring.");
            }

            data.CopyTo(RingSegment);
            Written = true;
        }

        public void Write<T>(ref T data) where T : unmanaged
        {
            if ((nuint)sizeof(T) != Size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(data), "Cannot write segments into ring buffer; your data size is too small or large.");
            }
            else if (Written)
            {
                ThrowHelper.ThrowInvalidOperationException("Can only write to ring buffer once per ring.");
            }

            MemoryMarshal.Write(RingSegment, ref data);
            Written = true;
        }

        public void CycleRing()
        {
            // create fence for current ring
            _RingSyncs[(int)_Ring.Current]?.Dispose();
            _RingSyncs[(int)_Ring.Current] = new FenceSync(GL);

            // wait to enter next ring, then increment to it
            _RingSyncs[(int)_Ring.NextRing()]?.BusyWaitCPU();
            _Ring.Increment();

            Written = false;
        }

        private nint GetCurrentOffset() => (nint)(Size * _Ring.Current);


        #region Binding

        public void Bind(BufferTargetARB target, uint bindingIndex) => GL.BindBufferRange(target, bindingIndex, Handle, GetCurrentOffset(), Size);

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

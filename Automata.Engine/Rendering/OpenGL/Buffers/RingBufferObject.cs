using System;
using System.Runtime.CompilerServices;
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
        public nuint Buffers { get; }
        public nuint BufferedSize { get; }

        public RingBufferObject(GL gl, nuint size, nuint buffers, nuint alignment = 0u) : base(gl)
        {
            nuint remainder = size % alignment;

            if (remainder is not 0u)
            {
                size += alignment - remainder;
            }

            Size = size;
            Buffers = buffers;
            BufferedSize = size * buffers;

            Handle = GL.CreateBuffer();
            GL.NamedBufferStorage(Handle, BufferedSize, (void*)null!, (uint)_ACCESS_MASK);

            _Ring = new Ring((nuint)buffers);
            _RingSyncs = new FenceSync[(int)buffers];
            _Pointer = (byte*)GL.MapNamedBuffer(Handle, BufferAccessARB.WriteOnly);
        }

        public void Write(ReadOnlySpan<byte> data)
        {
            if ((nuint)data.Length > Size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(data), "Cannot write segment larger than ring buffer segment size.");
            }

            WaitRing();
            data.CopyTo(new Span<byte>(_Pointer + GetCurrentOffset(), (int)Size));
        }

        public void Write<T>(ref T data) where T : unmanaged
        {
            if ((nuint)sizeof(T) > Size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(data), "Cannot write segment larger than ring buffer segment size.");
            }

            WaitRing();
            Unsafe.Write(_Pointer + GetCurrentOffset(), data);
        }

        public void Write(void* data, nuint length)
        {
            if (length > Size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(data), "Cannot write segment larger than ring buffer segment size.");
            }

            WaitRing();
            Buffer.MemoryCopy(data, _Pointer + GetCurrentOffset(), Size, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WaitRing()
        {
            // wait to enter next ring, then increment to it
            _RingSyncs[(int)_Ring.NextRing()]?.BusyWaitCPU();
            _Ring.Increment();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private nint GetCurrentOffset() => (nint)(Size * _Ring.Current);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FenceRing()
        {
            // create fence for current ring
            _RingSyncs[(int)_Ring.Current]?.Dispose();
            _RingSyncs[(int)_Ring.Current] = new FenceSync(GL);
        }


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

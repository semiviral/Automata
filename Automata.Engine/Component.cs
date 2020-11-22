using System;

namespace Automata.Engine
{
    public abstract class Component : IEquatable<Component>, IDisposable
    {
        public bool Disposed { get; private set; }

        public bool Equals(Component? other) => other is not null && (other.GetType() == GetType());
        public override bool Equals(object? obj) => obj is Component component && Equals(component);

        public override int GetHashCode() => GetType().GetHashCode();

        public static bool operator ==(Component? left, Component? right) => Equals(left, right);
        public static bool operator !=(Component? left, Component? right) => !Equals(left, right);


        #region IDisposable

        public void Dispose()
        {
            Dispose(false);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool finalizer)
        {
            if (Disposed)
            {
                return;
            }

            if (!finalizer)
            {
                CleanupManagedResources();
            }

            CleanupNativeResources();
            Disposed = true;
        }

        protected virtual void CleanupManagedResources() { }
        protected virtual void CleanupNativeResources() { }

        ~Component() => Dispose(true);

        #endregion
    }

    public abstract class ComponentChangeable : Component
    {
        public bool Changed { get; set; }
    }
}

using System;

namespace Automata.Engine
{
    public abstract class Component : IEquatable<Component>, IDisposable
    {
        public Guid ID { get; }
        public bool Disposed { get; private set; }

        public Component() => ID = Guid.NewGuid();

        public bool Equals(Component? other) => other is not null && ID.Equals(other.ID);
        public override bool Equals(object? obj) => obj is Component component && Equals(component);

        public override int GetHashCode() => ID.GetHashCode();

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

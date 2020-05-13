#region

using System.Numerics;
using Automata.Core.Components;

#endregion

namespace Automata.Rendering
{
    public class Camera : IComponent
    {
        private Matrix4x4 _View = Matrix4x4.Identity;
        private Matrix4x4 _Projection = Matrix4x4.Identity;
        private Matrix4x4 _Model = Matrix4x4.Identity;

        public bool Changed { get; set; }

        public Matrix4x4 View
        {
            get => _View;
            set
            {
                Changed = true;
                _View = value;
            }
        }

        public Matrix4x4 Projection
        {
            get => _Projection;
            set
            {
                Changed = true;
                _Projection = value;
            }
        }

        public Matrix4x4 Model
        {
            get => _Model;
            set
            {
                Changed = true;
                _Model = value;
            }
        }
    }
}

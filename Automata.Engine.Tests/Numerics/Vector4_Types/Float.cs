using System.Diagnostics;
using Automata.Engine.Numerics;
using Xunit;

namespace Automata.Engine.Tests.Numerics.Vector4_Types
{
    public class Float
    {
        private static readonly Vector4<float> _A = new Vector4<float>(0, 10, 10, float.MaxValue);
        private static readonly Vector4<float> _B = new Vector4<float>(0, 0, 20, float.MaxValue);

        [Fact]
        public void AddOperator()
        {
            Vector4<float> result = _A + _B;

            Debug.Assert(result.X is 0);
            Debug.Assert(result.Y is 10);
            Debug.Assert(result.Z is 30);
            Debug.Assert(result.W == float.PositiveInfinity);
        }

        [Fact]
        public void SubtractOperator()
        {
            Vector4<float> result = _A - _B;

            Debug.Assert(result.X is 0);
            Debug.Assert(result.Y is 10);
            Debug.Assert(result.Z is -10);
            Debug.Assert(result.W is 0);
        }

        [Fact]
        public void MultiplyOperator()
        {
            Vector4<float> result = _A * _B;

            Debug.Assert(result.X is 0);
            Debug.Assert(result.Y is 0);
            Debug.Assert(result.Z is 200);
            Debug.Assert(result.W == float.PositiveInfinity);
        }

        [Fact]
        public void DivideOperator()
        {
            Vector4<float> result = _A / _B;

            Debug.Assert(result.X is float.NaN);
            Debug.Assert(result.Y is float.PositiveInfinity);
            Debug.Assert(result.Z is 0.5f);
            Debug.Assert(result.W is 1);
        }

        [Fact]
        public void AbsOperator()
        {
            Vector4<float> result = Vector4<float>.Abs(new Vector4<float>(-0.5f));

            Debug.Assert(result.X is 0.5f);
            Debug.Assert(result.Y is 0.5f);
            Debug.Assert(result.Z is 0.5f);
            Debug.Assert(result.W is 0.5f);
        }

        [Fact]
        public void FloorOperator()
        {
            Vector4<float> result = Vector4<float>.Ceiling(new Vector4<float>(-0.5f));

            Debug.Assert(result.X is 0);
            Debug.Assert(result.Y is 0);
            Debug.Assert(result.Z is 0);
            Debug.Assert(result.W is 0);
        }

        [Fact]
        public void CeilingOperator()
        {
            Vector4<float> result = Vector4<float>.Floor(new Vector4<float>(-0.5f));

            Debug.Assert(result.X is -1);
            Debug.Assert(result.Y is -1);
            Debug.Assert(result.Z is -1);
            Debug.Assert(result.W is -1);
        }

        [Fact]
        public void EqualsOperator()
        {
            Vector4<bool> result = _A == _B;

            Debug.Assert(result.X is true);
            Debug.Assert(result.Y is false);
            Debug.Assert(result.Z is false);
            Debug.Assert(result.W is true);
        }

        [Fact]
        public void NotEqualOperator()
        {
            Vector4<bool> result = _A != _B;

            Debug.Assert(result.X is false);
            Debug.Assert(result.Y is true);
            Debug.Assert(result.Z is true);
            Debug.Assert(result.W is false);
        }

        [Fact]
        public void GreaterThanOperator()
        {
            Vector4<bool> result = _A > _B;

            Debug.Assert(result.X is false);
            Debug.Assert(result.Y is true);
            Debug.Assert(result.Z is false);
            Debug.Assert(result.W is false);
        }

        [Fact]
        public void LessThanOperator()
        {
            Vector4<bool> result = _A < _B;

            Debug.Assert(result.X is false);
            Debug.Assert(result.Y is false);
            Debug.Assert(result.Z is true);
            Debug.Assert(result.W is false);
        }

        [Fact]
        public void GreaterThanOrEqualOperator()
        {
            Vector4<bool> result = _A >= _B;

            Debug.Assert(result.X is true);
            Debug.Assert(result.Y is true);
            Debug.Assert(result.Z is false);
            Debug.Assert(result.W is true);
        }

        [Fact]
        public void LessThanOrEqualOperator()
        {
            Vector4<bool> result = _A <= _B;

            Debug.Assert(result.X is true);
            Debug.Assert(result.Y is false);
            Debug.Assert(result.Z is true);
            Debug.Assert(result.W is true);
        }
    }
}

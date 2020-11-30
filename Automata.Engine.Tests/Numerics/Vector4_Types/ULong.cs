using System.Diagnostics;
using Automata.Engine.Numerics;
using Xunit;

namespace Automata.Engine.Tests.Numerics.Vector4_Types
{
    public class ULong
    {
        private static readonly Vector4<ulong> _A = new Vector4<ulong>(0, 10, 10, ulong.MaxValue);
        private static readonly Vector4<ulong> _B = new Vector4<ulong>(0, 0, 20, ulong.MaxValue);

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

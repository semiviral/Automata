using System;
using System.Diagnostics;
using Automata.Engine.Numerics;
using Xunit;

namespace Automata.Engine.Tests.Numerics.Vector4_Types
{
    public class Int
    {
        private static readonly Vector4<int> _A = new Vector4<int>(0, 10, 10, int.MaxValue);
        private static readonly Vector4<int> _B = new Vector4<int>(0, 0, 20, int.MaxValue);

        [Fact]
        public void AddOperator()
        {
            Vector4<int> result = _A + _B;

            Debug.Assert(result.X is 0);
            Debug.Assert(result.Y is 10);
            Debug.Assert(result.Z is 30);
            Debug.Assert(result.W is -2);
        }

        [Fact]
        public void SubtractOperator()
        {
            Vector4<int> result = _A - _B;

            Debug.Assert(result.X is 0);
            Debug.Assert(result.Y is 10);
            Debug.Assert(result.Z is -10);
            Debug.Assert(result.W is 0);
        }

        [Fact]
        public void MultiplyOperator()
        {
            Vector4<int> result = _A * _B;

            Debug.Assert(result.X is 0);
            Debug.Assert(result.Y is 0);
            Debug.Assert(result.Z is 200);
            Debug.Assert(result.W is 1);
        }

        [Fact]
        public void DivideOperator()
        {
            Vector4<int> result = default;

            try
            {
                result = _A / _B;
            }
            catch (DivideByZeroException)
            {
                // this is expected
            }
            finally
            {
                Debug.Assert(result.X is 0);
                Debug.Assert(result.Y is 0);
                Debug.Assert(result.Z is 0);
                Debug.Assert(result.W is 0);
            }
        }

        [Fact]
        public void AbsOperator()
        {
            Vector4<int> result = Vector4<int>.Abs(new Vector4<int>(1));

            Debug.Assert(result.X is 1);
            Debug.Assert(result.Y is 1);
            Debug.Assert(result.Z is 1);
            Debug.Assert(result.W is 1);
        }

        [Fact]
        public void FloorOperator()
        {
            Vector4<int> result = Vector4<int>.Abs(new Vector4<int>(1));

            Debug.Assert(result.X is 1);
            Debug.Assert(result.Y is 1);
            Debug.Assert(result.Z is 1);
            Debug.Assert(result.W is 1);
        }

        [Fact]
        public void CeilingOperator()
        {
            Vector4<int> result = Vector4<int>.Abs(new Vector4<int>(1));

            Debug.Assert(result.X is 1);
            Debug.Assert(result.Y is 1);
            Debug.Assert(result.Z is 1);
            Debug.Assert(result.W is 1);
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

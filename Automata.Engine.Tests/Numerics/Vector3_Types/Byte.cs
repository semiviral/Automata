using System;
using System.Diagnostics;
using Automata.Engine.Numerics;
using Xunit;

namespace Automata.Engine.Tests.Numerics.Vector3_Types
{
    public class Byte
    {
        private static readonly Vector3<byte> _A = new Vector3<byte>(0, 10, 10);
        private static readonly Vector3<byte> _B = new Vector3<byte>(0, 0, 20);

        [Fact]
        public void AddOperator()
        {
            Vector3<byte> result = _A + _B;

            Debug.Assert(result.X is 0);
            Debug.Assert(result.Y is 10);
            Debug.Assert(result.Z is 30);
        }

        [Fact]
        public void SubtractOperator()
        {
            Vector3<byte> result = _A - _B;

            Debug.Assert(result.X is 0);
            Debug.Assert(result.Y is 10);
            Debug.Assert(result.Z is 246);
        }

        [Fact]
        public void MultiplyOperator()
        {
            Vector3<byte> result = _A * _B;

            Debug.Assert(result.X is 0);
            Debug.Assert(result.Y is 0);
            Debug.Assert(result.Z is 200);
        }

        [Fact]
        public void DivideOperator()
        {
            Vector3<byte> result = default;

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
            }
        }

        [Fact]
        public void AbsOperator()
        {
            Vector3<byte> result = Vector3<byte>.Abs(new Vector3<byte>(1));

            Debug.Assert(result.X is 1);
            Debug.Assert(result.Y is 1);
            Debug.Assert(result.Z is 1);
        }

        [Fact]
        public void FloorOperator()
        {
            Vector3<byte> result = Vector3<byte>.Abs(new Vector3<byte>(1));

            Debug.Assert(result.X is 1);
            Debug.Assert(result.Y is 1);
            Debug.Assert(result.Z is 1);
        }

        [Fact]
        public void CeilingOperator()
        {
            Vector3<byte> result = Vector3<byte>.Abs(new Vector3<byte>(1));

            Debug.Assert(result.X is 1);
            Debug.Assert(result.Y is 1);
            Debug.Assert(result.Z is 1);
        }

        [Fact]
        public void EqualsOperator()
        {
            Vector3<bool> result = _A == _B;

            Debug.Assert(result.X is true);
            Debug.Assert(result.Y is false);
            Debug.Assert(result.Z is false);
        }

        [Fact]
        public void NotEqualOperator()
        {
            Vector3<bool> result = _A != _B;

            Debug.Assert(result.X is false);
            Debug.Assert(result.Y is true);
            Debug.Assert(result.Z is true);
        }

        [Fact]
        public void GreaterThanOperator()
        {
            Vector3<bool> result = _A > _B;

            Debug.Assert(result.X is false);
            Debug.Assert(result.Y is true);
            Debug.Assert(result.Z is false);
        }

        [Fact]
        public void LessThanOperator()
        {
            Vector3<bool> result = _A < _B;

            Debug.Assert(result.X is false);
            Debug.Assert(result.Y is false);
            Debug.Assert(result.Z is true);
        }

        [Fact]
        public void GreaterThanOrEqualOperator()
        {
            Vector3<bool> result = _A >= _B;

            Debug.Assert(result.X is true);
            Debug.Assert(result.Y is true);
            Debug.Assert(result.Z is false);
        }

        [Fact]
        public void LessThanOrEqualOperator()
        {
            Vector3<bool> result = _A <= _B;

            Debug.Assert(result.X is true);
            Debug.Assert(result.Y is false);
            Debug.Assert(result.Z is true);
        }
    }
}

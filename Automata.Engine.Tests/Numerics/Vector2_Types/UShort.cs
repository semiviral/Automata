using System;
using System.Diagnostics;
using Automata.Engine.Numerics;
using Xunit;

namespace Automata.Engine.Tests.Numerics.Vector2_Types
{
    public class UShort
    {
        private static readonly Vector2<ushort> _A = new Vector2<ushort>(0, 10);
        private static readonly Vector2<ushort> _B = new Vector2<ushort>(0, 0);

        [Fact]
        public void AddOperator()
        {
            Vector2<ushort> result = _A + _B;

            Debug.Assert(result.X is 0);
            Debug.Assert(result.Y is 10);
        }

        [Fact]
        public void SubtractOperator()
        {
            Vector2<ushort> result = _A - _B;

            Debug.Assert(result.X is 0);
            Debug.Assert(result.Y is 10);
        }

        [Fact]
        public void MultiplyOperator()
        {
            Vector2<ushort> result = _A * _B;

            Debug.Assert(result.X is 0);
            Debug.Assert(result.Y is 0);
        }

        [Fact]
        public void DivideOperator()
        {
            Vector2<ushort> result = default;

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
            }
        }

        [Fact]
        public void AbsOperator()
        {
            Vector2<ushort> result = Vector2<ushort>.Abs(new Vector2<ushort>(1));

            Debug.Assert(result.X is 1);
            Debug.Assert(result.Y is 1);
        }

        [Fact]
        public void FloorOperator()
        {
            Vector2<ushort> result = Vector2<ushort>.Abs(new Vector2<ushort>(1));

            Debug.Assert(result.X is 1);
            Debug.Assert(result.Y is 1);
        }

        [Fact]
        public void CeilingOperator()
        {
            Vector2<ushort> result = Vector2<ushort>.Abs(new Vector2<ushort>(1));

            Debug.Assert(result.X is 1);
            Debug.Assert(result.Y is 1);
        }

        [Fact]
        public void EqualsOperator()
        {
            Vector2<bool> result = _A == _B;

            Debug.Assert(result.X is true);
            Debug.Assert(result.Y is false);
        }

        [Fact]
        public void NotEqualOperator()
        {
            Vector2<bool> result = _A != _B;

            Debug.Assert(result.X is false);
            Debug.Assert(result.Y is true);
        }

        [Fact]
        public void GreaterThanOperator()
        {
            Vector2<bool> result = _A > _B;

            Debug.Assert(result.X is false);
            Debug.Assert(result.Y is true);
        }

        [Fact]
        public void LessThanOperator()
        {
            Vector2<bool> result = _A < _B;

            Debug.Assert(result.X is false);
            Debug.Assert(result.Y is false);
        }

        [Fact]
        public void GreaterThanOrEqualOperator()
        {
            Vector2<bool> result = _A >= _B;

            Debug.Assert(result.X is true);
            Debug.Assert(result.Y is true);
        }

        [Fact]
        public void LessThanOrEqualOperator()
        {
            Vector2<bool> result = _A <= _B;

            Debug.Assert(result.X is true);
            Debug.Assert(result.Y is false);
        }
    }
}

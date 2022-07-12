using System.Diagnostics;
using Automata.Engine.Numerics;
using Xunit;

namespace Automata.Engine.Tests.Numerics.Vector3_Types
{
    public class Double
    {
        private static readonly Vector3<double> _A = new Vector3<double>(0, 10, 10);
        private static readonly Vector3<double> _B = new Vector3<double>(0, 0, 20);

        [Fact]
        public void AddOperator()
        {
            Vector3<double> result = _A + _B;

            Debug.Assert(result.X is 0);
            Debug.Assert(result.Y is 10);
            Debug.Assert(result.Z is 30);
        }

        [Fact]
        public void SubtractOperator()
        {
            Vector3<double> result = _A - _B;

            Debug.Assert(result.X is 0);
            Debug.Assert(result.Y is 10);
            Debug.Assert(result.Z is -10);
        }

        [Fact]
        public void MultiplyOperator()
        {
            Vector3<double> result = _A * _B;

            Debug.Assert(result.X is 0);
            Debug.Assert(result.Y is 0);
            Debug.Assert(result.Z is 200);
        }

        [Fact]
        public void DivideOperator()
        {
            Vector3<double> result = _A / _B;

            Debug.Assert(result.X is double.NaN);
            Debug.Assert(result.Y is double.PositiveInfinity);
            Debug.Assert(result.Z is 0.5);
        }

        [Fact]
        public void AbsOperator()
        {
            Vector3<double> result = Vector3<double>.Abs(new Vector3<double>(-0.5d));

            Debug.Assert(result.X is 0.5);
            Debug.Assert(result.Y is 0.5);
            Debug.Assert(result.Z is 0.5);
        }

        [Fact]
        public void FloorOperator()
        {
            Vector3<double> result = Vector3<double>.Ceiling(new Vector3<double>(-0.5d));

            Debug.Assert(result.X is 0);
            Debug.Assert(result.Y is 0);
            Debug.Assert(result.Z is 0);
        }

        [Fact]
        public void CeilingOperator()
        {
            Vector3<double> result = Vector3<double>.Floor(new Vector3<double>(-0.5d));

            Debug.Assert(result.X is -1);
            Debug.Assert(result.Y is -1);
            Debug.Assert(result.Z is -1);
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

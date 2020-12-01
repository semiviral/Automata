using System.Diagnostics;
using Automata.Engine.Numerics;
using Xunit;

namespace Automata.Engine.Tests.Numerics.Primitive
{
    public class Convert
    {
        [Theory]
        [InlineData(-5)]
        public void SByteToSByte(sbyte a)
        {
            Debug.Assert(Primitive<sbyte>.Convert<sbyte>(a) == a);
        }

        [Theory]
        [InlineData(-5f)]
        public void FloatToDouble(float a)
        {
            Debug.Assert(Primitive<float>.Convert<double>(a) == a);
        }

        [Theory]
        [InlineData(-5f)]
        public void FloatToDecimal(float a)
        {
            Debug.Assert(Primitive<float>.Convert<decimal>(a)== -5m);
        }
    }
}

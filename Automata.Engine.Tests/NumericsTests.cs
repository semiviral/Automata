using System.Diagnostics;
using Automata.Engine.Numerics;
using Xunit;

namespace Automata.Engine.Tests
{
    public class NumericsTests
    {
        [Fact]
        public void Vector2IntMultiply()
        {
            Vector2<int> a = new Vector2<int>(2, 2);
            Vector2<int> b = new Vector2<int>(2, 2);
            Vector2<int> r = a * b;

            Debug.Assert(r.X is 4);
            Debug.Assert(r.Y is 4);
        }
    }
}

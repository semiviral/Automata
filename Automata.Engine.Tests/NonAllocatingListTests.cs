using System.Diagnostics;
using Automata.Engine.Collections;
using Xunit;

namespace Automata.Engine.Tests
{
    public class NonAllocatingListTests
    {
        [Fact]
        public void TestAddRange()
        {
            using NonAllocatingList<uint> list = new NonAllocatingList<uint>();

            list.AddRange(stackalloc[]
            {
                0u,
                1u
            });

            Debug.Assert(list.Count is 2);
            Debug.Assert(list[0] is 0u);
            Debug.Assert(list[1] is 1u);

            list.AddRange(stackalloc[]
            {
                2u,
                3u
            });

            Debug.Assert(list.Count is 4);
            Debug.Assert(list[0] is 0u);
            Debug.Assert(list[1] is 1u);
            Debug.Assert(list[2] is 2u);
            Debug.Assert(list[3] is 3u);
        }

        [Fact]
        public void TestInsertRange()
        {
            using NonAllocatingList<uint> list = new NonAllocatingList<uint>();

            list.InsertRange(0, stackalloc[]
            {
                0u,
                3u
            });

            Debug.Assert(list.Count is 2);
            Debug.Assert(list[0] is 0u);
            Debug.Assert(list[1] is 3u);

            list.InsertRange(1, stackalloc[]
            {
                1u,
                2u
            });

            Debug.Assert(list.Count is 4);
            Debug.Assert(list[0] is 0u);
            Debug.Assert(list[1] is 1u);
            Debug.Assert(list[2] is 2u);
            Debug.Assert(list[3] is 3u);
        }
    }
}

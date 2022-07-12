using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Automata.Engine.Collections;
using Xunit;

namespace Automata.Engine.Tests
{
    public class NonAllocatingListTests
    {
        [Fact]
        public void Add()
        {
            static void add_and_verify_impl_impl<T>(IList<T> list, T item, int length) where T : IEquatable<T>
            {
                Debug.Assert(list.Count == (length - 1));

                list.Add(item);

                Debug.Assert(list.Count == length);
                Debug.Assert(list[length - 1].Equals(item));
            }

            using NonAllocatingList<uint> list = new NonAllocatingList<uint>();

            add_and_verify_impl_impl(list, 0u, 1);
            add_and_verify_impl_impl(list, 1u, 2);
            add_and_verify_impl_impl(list, 2u, 3);
            add_and_verify_impl_impl(list, 3u, 4);
            add_and_verify_impl_impl(list, 4u, 5);
            add_and_verify_impl_impl(list, 5u, 6);
            add_and_verify_impl_impl(list, 6u, 7);
            add_and_verify_impl_impl(list, 7u, 8);
        }

        [Fact]
        public void Remove()
        {
            using NonAllocatingList<uint> list = new NonAllocatingList<uint>
            {
                0u,
                1u,
                2u,
                3u,
                4u,
                5u,
                6u,
                7u,
                8u,
                9u,
                10u
            };

            Debug.Assert(list.Count is 11);
            Debug.Assert(list[4u] is 4u);

            list.Remove(4u);

            Debug.Assert(list.Count is 10);
            Debug.Assert(list[4u] is 5u);

            list.Remove(0u);

            Debug.Assert(list.Count is 9);
            Debug.Assert(list[0u] is 1u);

            list.Remove(10u);

            Debug.Assert(list.Count is 8);
            Debug.Assert(list[7u] is 9u);
        }

        [Fact]
        public void RemoveAt()
        {
            using NonAllocatingList<uint> list = new NonAllocatingList<uint>
            {
                0u,
                1u,
                2u,
                3u,
                4u,
                5u,
                6u,
                7u,
                8u,
                9u,
                10u
            };

            Debug.Assert(list.Count is 11);
            Debug.Assert(list[4u] is 4u);

            list.RemoveAt(4);

            Debug.Assert(list.Count is 10);
            Debug.Assert(list[4u] is 5u);

            list.RemoveAt(0);

            Debug.Assert(list.Count is 9);
            Debug.Assert(list[0u] is 1u);

            list.RemoveAt(8);

            Debug.Assert(list.Count is 8);
            Debug.Assert(list[7u] is 9u);
        }

        [Fact]
        public void IndexOf()
        {
            using NonAllocatingList<uint> list = new NonAllocatingList<uint>
            {
                0u,
                1u,
                2u,
                3u,
                4u,
                5u,
                6u,
                7u,
                8u,
                9u,
                10u
            };

            Debug.Assert(list.IndexOf(0u) is 0);
            Debug.Assert(list.IndexOf(4u) is 4);
            Debug.Assert(list.IndexOf(7u) is 7);
            Debug.Assert(list.IndexOf(10u) is 10);
        }

        [Fact]
        public void Clear()
        {
            using NonAllocatingList<uint> list = new NonAllocatingList<uint>
            {
                0u,
                1u,
                2u,
                3u,
                4u,
                5u,
                6u,
                7u,
                8u,
                9u,
                10u
            };

            list.Clear();

            Debug.Assert(list.Count is 0);
        }

        [Fact]
        public void Fill()
        {
            using NonAllocatingList<uint> list = new NonAllocatingList<uint>
            {
                0u,
                1u,
                2u,
                3u,
                4u,
                5u,
                6u,
                7u,
                8u,
                9u,
                10u
            };

            list.Fill(1u);

            Debug.Assert(Enumerable.Repeat(1u, 11).SequenceEqual(list));
        }

        [Fact]
        public void Contains()
        {
            using NonAllocatingList<uint> list = new NonAllocatingList<uint>
            {
                0u,
                1u,
                2u,
                3u,
                4u,
                5u,
                6u,
                7u,
                8u,
                9u,
                10u
            };

            Debug.Assert(list.Contains(0u));
            Debug.Assert(list.Contains(1u));
            Debug.Assert(list.Contains(2u));
            Debug.Assert(list.Contains(3u));
            Debug.Assert(list.Contains(4u));
            Debug.Assert(list.Contains(5u));
            Debug.Assert(list.Contains(6u));
            Debug.Assert(list.Contains(7u));
            Debug.Assert(list.Contains(8u));
            Debug.Assert(list.Contains(9u));
            Debug.Assert(list.Contains(10u));
        }

        [Fact]
        public void AddRange()
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
        public void InsertRange()
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

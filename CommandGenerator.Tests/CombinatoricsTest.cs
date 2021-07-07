using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace RoboCup.AtHome.CommandGenerator.Tests
{
    public class CombinatoricsTest
    {
        private class Counter : IEnumerable<int>
        {
            public int Start { get; set; }
            public int Count { get; set; }
            public Counter(int start, int count) {
                Start = start;
                Count = count;
            }

            public IEnumerator<int> GetEnumerator()
            {
                return new CounterEnumerator(Start, Count);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private class CounterEnumerator : IEnumerator<int>
        {
            public int Current { get; set; }
            public int Start { get; set; }
            public int Count { get; set; }

            public CounterEnumerator(int start, int count, int? current = null) {
                Start = start;
                Count = count;
                Current = current ?? start - 1;
            }

            object IEnumerator.Current => Current;

            public void Dispose() { }

            public bool MoveNext() {
                if (Current < Start + Count - 1) {
                    Current += 1;
                    return true;
                } else {
                    return false;
                }
            }

            public void Reset()
            {
                throw new System.NotImplementedException();
            }
        }

        [Fact] 
        public void TestFinalOverflowIncrement() {
            var sequences = new List<IEnumerable<int>>() { 
                new Counter(0, 10),
                new Counter(0, 10),
                new Counter(0, 10),
                new Counter(0, 10),
                new Counter(0, 10),
            };
            var enumerators = new IEnumerator<int>[] {
                new CounterEnumerator(0, 10, 9),
                new CounterEnumerator(0, 10, 9),
                new CounterEnumerator(0, 10, 9),
                new CounterEnumerator(0, 10, 9),
                new CounterEnumerator(0, 10, 9)
            };
            var index = 4;
            var result = Combinatorics.IncrementIndices(sequences, enumerators, ref index);
            Assert.True(result);
            Assert.Equal(0, index);
            for (int i = 0; i < enumerators.Length; i += 1) {
                Assert.Equal(0, enumerators[i].Current);
            }
        }

        [Fact]
        public void TestOverflowIncrement() {
            var sequences = new List<IEnumerable<int>>() { 
                new Counter(0, 10),
                new Counter(0, 10),
                new Counter(0, 10),
                new Counter(0, 10),
                new Counter(0, 10),
            };
            var enumerators = new IEnumerator<int>[] {
                new CounterEnumerator(0, 10, 9),
                new CounterEnumerator(0, 10, 9),
                new CounterEnumerator(0, 10, 9),
                new CounterEnumerator(0, 10, 0),
                new CounterEnumerator(0, 10, 9)
            };
            var index = 4;
            var result = Combinatorics.IncrementIndices(sequences, enumerators, ref index);
            Assert.False(result);
            Assert.Equal(3, index);
            Assert.Equal(1, enumerators[index].Current);
        }
    }
}
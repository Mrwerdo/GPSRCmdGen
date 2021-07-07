using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace RoboCup.AtHome.CommandGenerator
{
	public static class Combinatorics {
		/*
		Suppose there is a fixed length list L that has N elements, and for any given i less than N there
		corresponds a sequence S[i] (S subscript i, read as "S i") such that element L[i] can be any one of the
		elements in S[i]. This function enumerates all lists that meet the constraints for L.

		The length N is deduced by the length of the argument.
		*/
		public static IEnumerable<T[]> EnumerateReplacementsOfOrderedList<T>(List<IEnumerable<T>> sequences)
		{
			int n = sequences.Count;
			IEnumerator<T>[] enumerators = sequences.Select(s => s.GetEnumerator()).ToArray();
			int[] indices = new int[n];
			T[] result = new T[n];
			int index = 0;

			foreach (IEnumerator<T> e in enumerators) {
				if (!e.MoveNext()) {
					throw new Exception("an empty sequence does not make sense!");
				}
			}

			while (true) {
				IEnumerator<T> currentEnumerator = enumerators[index];
				result[index] = currentEnumerator.Current;
				if (index + 1 == n) {
					yield return result;
					if (IncrementIndices(sequences, enumerators, ref index)) {
						break;
					}
				} else {
					index += 1;
				}
			}
		}
		
		public static bool IncrementIndices<T>(List<IEnumerable<T>> sequences, IEnumerator<T>[] enumerators, ref int index) {
			while (true) {
				if (!enumerators[index].MoveNext()) {
					// We have yielded the last result for this index.
					enumerators[index] = sequences.ElementAt(index).GetEnumerator();
					enumerators[index].MoveNext();
					if (index > 0) {
						index -= 1;
					} else {
						return true;
					}
				} else {
					return false;
				}
			}
		}
	}
}
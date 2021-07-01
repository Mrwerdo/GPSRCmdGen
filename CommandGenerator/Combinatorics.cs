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
		public static IEnumerable<T[]> EnumerateReplacementsOfOrderedList<T>(List<List<T>> sequences)
		{
			int n = sequences.Count;
			int[] lengths = sequences.Select(s => s.Count).ToArray();
			int[] indices = new int[n];
			T[] result = new T[n];
			int index = 0;

			while (true) {
				result[index] = sequences.ElementAt(index).ElementAt(indices[index]);
				if (index + 1 == n) {
					yield return result;
					if (IncrementIndices(lengths, indices, ref index)) {
						break;
					}
				} else {
					index += 1;
				}
			}
		}
		
		public static bool IncrementIndices(int[] lengths, int[] indices, ref int index) {
			while (true) {
				if (indices[index] + 1 == lengths[index]) {
					// We have yielded the last result for this index.
					indices[index] = 0;
					if (index > 0) {
						index -= 1;
					} else {
						return true;
					}
				} else {
					indices[index] += 1;
					return false;
				}
			}
		}
	}
}
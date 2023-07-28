// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Patterns.Aggregators
{
    /// <summary>
    /// Aggregates objects into groups of patterns where elements are evenly spaced rhythmically.
    /// In edges where an interval change occurs, the child will be added to the group with a smaller interval.
    /// </summary>
    public class RhythmAggregator
    {
        public double MarginOfError;

        public RhythmAggregator(double marginOfError = 3)
        {
            MarginOfError = marginOfError;
        }

        private bool isFlat(IHasInterval current, IHasInterval previous, double marginOfError)
        {
            return Math.Abs(current.Interval - previous.Interval) <= marginOfError;
        }

        /// <summary>
        /// Create a new output group and add all elements from <paramref name="data"/> until an interval change occurs.
        /// </summary>
        private OutType nextGroup<InType, OutType>(OutType? previous, List<InType> data, ref int i, double marginOfError)
            where InType : IHasInterval
            where OutType : FlatRhythm<InType>, new()
        {
            OutType result = new OutType
            {
                Previous = previous,
            };

            result.Children.Add(data[i]);
            i++;

            for (; i < data.Count - 1; i++)
            {
                // An interval change occured, add the current data if the next interval is larger.
                if (!isFlat(data[i], data[i + 1], marginOfError))
                {
                    if (data[i + 1].Interval > data[i].Interval + marginOfError)
                    {
                        result.Children.Add(data[i]);
                        i++;
                    }

                    return result;
                }

                // No interval change occured
                result.Children.Add(data[i]);
            }

            // Handle final data
            if (data.Count > 2 && isFlat(data[^1], data[^2], marginOfError))
            {
                result.Children.Add(data[i]);
                i++;
            }

            return result;
        }

        /// <summary>
        /// Group <paramref name="input"/> into a list of <typeparamref name="OutType"/>s.
        /// </summary>
        public List<OutType> Group<InType, OutType>(List<InType> input)
            where InType : IHasInterval
            where OutType : FlatRhythm<InType>, new()
        {
            List<OutType> result = new List<OutType>();

            // Index does not need to be incremented, as it is handled within EvenRhythm's constructor.
            for (int i = 0; i < input.Count;)
            {
                OutType? previous = result.Count > 0 ? result[^1] : null;
                result.Add(nextGroup(previous, input, ref i, MarginOfError));
            }

            return result;
        }
    }
}

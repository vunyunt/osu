// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Patterns.Aggregators
{
    /// <summary>
    /// Aggregates <see cref="IRepeatable{OtherType}"/>s by repetition
    /// </summary>
    public class RepetitionAggregator
    {
        public RepetitionAggregator() { }

        public List<OutType> Group<InType, OutType>(List<InType> input)
            where InType : class, IHasInterval, IRepeatable<InType>
            where OutType : DifficultyPattern<InType>, new()
        {
            List<OutType> result = new();
            OutType? currentGroup = null;
            InType? previous = null;

            foreach (InType current in input)
            {
                if (previous == null || !current.IsRepetitionOf(previous))
                {
                    if (currentGroup != null)
                    {
                        result.Add(currentGroup);
                    }

                    currentGroup = new OutType();
                }

                currentGroup!.Children.Add(current);
                previous = current;
            }

            return result;
        }
    }
}

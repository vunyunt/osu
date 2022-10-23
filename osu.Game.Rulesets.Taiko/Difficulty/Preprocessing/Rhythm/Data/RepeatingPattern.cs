using System;
using System.Linq;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Rhythm.Data
{
    /// <summary>
    /// Repeating rhythm pattern that contains either one repeating or two alternating <see cref="FlatPattern"/>s.
    /// </summary>
    public class RepeatingPattern
    {
        /// <summary>
        /// Maximum number of notes to look back when checking for repetition.
        /// </summary>
        private const int max_repetition_interval = 32;

        private readonly List<RepeatingPattern> allRepeatingPatterns;

        /// <summary>
        /// The index of this <see cref="RepeatingPattern"/> in the list of all <see cref="RepeatingPattern"/>s.
        /// </summary>
        public readonly int Index;

        /// <summary>
        /// All <see cref="FlatPattern"/>s that are part of this <see cref="RepeatingPattern"/>.
        /// </summary>
        public List<FlatPattern> FlatPatterns { get; private set; } = new List<FlatPattern>();

        public TaikoDifficultyHitObject FirstHitObject => FlatPatterns.First().FirstHitObject;

        public RepeatingPattern? Previous(int backwardsIndex) => allRepeatingPatterns.ElementAtOrDefault(Index - (backwardsIndex + 1));

        /// <summary>
        /// The amount of hit objects that are part of this <see cref="RepeatingPattern"/>.
        /// </summary>
        public int Length = 0;

        /// <summary>
        /// The amount of <see cref="FlatPattern"/>s that form a repetition in this <see cref="RepeatingPattern"/>. This
        /// should either have a value of 1 (single repeating pattern) or 2 (two alternating pattern).
        /// </summary>
        public int RepetitionLength = 1;

        /// <summary>
        /// The amount of hit objects that separates this <see cref="RepeatingPattern"/> from the previous one that 
        /// is a repetition. This will have a maximum value of <see cref="max_repetition_interval"/>.
        /// </summary>
        public int RepetitionInterval;

        public RepeatingPattern(List<RepeatingPattern> allRepeatingPatterns, int index)
        {
            this.allRepeatingPatterns = allRepeatingPatterns;
            Index = index;
        }

        /// <summary>
        /// Repetition here is defined as a <see cref="RepeatingPattern"/> that contains the same <see cref="FlatPattern"/>s.
        /// The amount of <see cref="FlatPattern"/>s contained is not taken into consideration. However, the order of them
        /// is (i.e. if two <see cref="RepeatingPattern"/>s contains the same alternating <see cref="FlatPattern"/>s but
        /// "out of phase", they are not considered repetitions).
        /// </summary>
        public bool IsRepetitionOf(RepeatingPattern other)
        {
            int patternsToCheck = Math.Min(2, Math.Min(FlatPatterns.Count, other.FlatPatterns.Count));

            for (int i = 0; i < patternsToCheck; i++)
            {
                if (!FlatPatterns[i].IsRepetitionOf(other.FlatPatterns[i]))
                    return false;
            }

            return true;
        }

        public void FindRepetitionInterval()
        {
            RepeatingPattern? current = Previous(0);
            int interval = 1;

            while (interval < max_repetition_interval && current != null)
            {
                interval += current.Length;

                if (current.IsRepetitionOf(this))
                {
                    RepetitionInterval = Math.Min(interval, max_repetition_interval);
                    return;
                }

                current = current.Previous(0);
            }

            RepetitionInterval = max_repetition_interval;
        }
    }
}
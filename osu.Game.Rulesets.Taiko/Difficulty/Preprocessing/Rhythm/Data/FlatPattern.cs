using System;
using System.Linq;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Rhythm.Data
{
    /// <summary>
    /// A flat pattern is defined as a sequence of hit objects that has effectively no variation in rhythm, i.e. all
    /// hit object within will have rhythm ratios of 1, with the exception of the first hit object.
    /// </summary>
    public class FlatPattern
    {
        private const int max_repetition_interval = 16;

        public List<TaikoDifficultyHitObject> HitObjects { get; private set; } = new List<TaikoDifficultyHitObject>();

        public ContinuousPattern Parent = null!;

        public int Index;

        /// <summary>
        /// The previous <see cref="FlatPattern"/> within the same <see cref="ContinuousPattern"/>.
        /// </summary>
        public FlatPattern? Previous => Index == 0 ? Parent.FlatPatterns[Index - 1] : null;

        /// <summary>
        /// The <see cref="TaikoDifficultyHitObjectRhythm.Ratio"/> of the first hit object in this pattern.
        /// </summary>
        public double Ratio => HitObjects[0].Rhythm.Ratio;

        public bool IsRepetitionOf(FlatPattern other)
        {
            return Math.Abs(this.Ratio / other.Ratio - 1) < 0.05 && HitObjects.Count == other.HitObjects.Count;
        }

        public int FindRepetitionInterval()
        {
            FlatPattern? current = Previous;
            int interval = 1;
            while (current != null)
            {
                interval += current.HitObjects.Count;

                if (IsRepetitionOf(current))
                {
                    return Math.Min(interval, max_repetition_interval);
                }

                current = current.Previous;
            }

            return max_repetition_interval;
        }
    }
}
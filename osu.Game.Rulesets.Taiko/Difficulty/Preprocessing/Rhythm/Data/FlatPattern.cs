using System;
using System.Linq;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Rhythm.Data
{
    /// <summary>
    /// A flat pattern is defined as a sequence of hit objects that has effectively no variation in rhythm, i.e. all
    /// hit object within will have rhythm ratios of almost 1, with the exception of the first two hit objects.
    /// </summary>
    public class FlatPattern
    {
        private readonly List<FlatPattern> allPatterns;

        /// <summary>
        /// The index of this <see cref="FlatPattern"/> in the list of all <see cref="FlatPattern"/>s.
        /// </summary>
        public readonly int Index;

        /// <summary>
        /// The parent <see cref="RepeatingPattern"/> of this <see cref="FlatPattern"/>.
        /// </summary>
        public RepeatingPattern Parent = null!;

        /// <summary>
        /// The amount of times this <see cref="FlatPattern"/> is repeated in the parent <see cref="RepeatingPattern"/>.
        /// </summary>
        public int RepetitionIndex;

        /// <summary>
        /// All <see cref="TaikoDifficultyHitObject"/>s that are part of this <see cref="FlatPattern"/>.
        /// </summary>
        public List<TaikoDifficultyHitObject> HitObjects { get; private set; } = new List<TaikoDifficultyHitObject>();

        public TaikoDifficultyHitObject FirstHitObject => HitObjects.First();

        public FlatPattern? Previous(int backwardsIndex) => allPatterns.ElementAtOrDefault(Index - (backwardsIndex + 1));

        public FlatPattern? Next(int forwardsIndex) => allPatterns.ElementAtOrDefault(Index + (forwardsIndex + 1));

        /// <summary>
        /// The <see cref="TaikoDifficultyHitObjectRhythm.Ratio"/> of the first hit object in this pattern.
        /// </summary>
        public double Ratio => HitObjects[0].Rhythm.Ratio;

        public FlatPattern(List<FlatPattern> allPatterns, int index)
        {
            this.allPatterns = allPatterns;
            Index = index;
        }

        /// <summary>
        /// Two <see cref="FlatPattern"/>s are considered repetitions if they have the same amount of hit objects and
        /// have the same interval between the first two hit objects. Only the first two hit objects are taken into
        /// account due to <see cref="FlatPattern"/>s are defined as having no variation in rhythm.
        ///
        /// If there is only one hit object in the <see cref="FlatPattern"/>s, they are considered repetitions if their
        /// first (and only) hit objects have the same interval.
        /// </summary>
        public bool IsRepetitionOf(FlatPattern? other)
        {
            if (other == null || HitObjects.Count != other.HitObjects.Count)
                return false;

            if (HitObjects.Count <= 1)
                return Math.Abs(HitObjects[0].DeltaTime - other.HitObjects[0].DeltaTime) < 3;

            return Math.Abs(HitObjects[1].DeltaTime - other.HitObjects[1].DeltaTime) < 3;
        }
    }
}
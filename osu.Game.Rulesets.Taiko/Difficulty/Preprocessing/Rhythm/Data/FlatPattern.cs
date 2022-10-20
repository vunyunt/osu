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
        public List<TaikoDifficultyHitObject> HitObjects { get; private set; } = new List<TaikoDifficultyHitObject>();

        public TaikoDifficultyHitObject FirstHitObject => HitObjects.First();

        public RepeatingRhythmPattern Parent = null!;

        public int Index;

        /// <summary>
        /// The previous <see cref="FlatPattern"/> within the same <see cref="RepeatingRhythmPattern"/>.
        /// </summary>
        public FlatPattern? Previous;

        /// <summary>
        /// The <see cref="TaikoDifficultyHitObjectRhythm.Ratio"/> of the first hit object in this pattern.
        /// </summary>
        public double Ratio => HitObjects[0].Rhythm.Ratio;

        public bool IsRepetitionOf(FlatPattern other)
        {
            if (HitObjects.Count != other.HitObjects.Count)
                return false;

            if (HitObjects.Count <= 1)
                return Math.Abs(HitObjects[0].DeltaTime - other.HitObjects[0].DeltaTime) < 3;

            return Math.Abs(HitObjects[1].DeltaTime - other.HitObjects[1].DeltaTime) < 3;
        }
    }
}
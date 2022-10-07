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
        public List<TaikoDifficultyHitObject> HitObjects { get; private set; } = new List<TaikoDifficultyHitObject>();

        public TaikoDifficultyHitObject FirstHitObject => HitObjects.First();

        public ContinuousPattern Parent = null!;

        public int Index;

        public int AlternatingIndex;

        /// <summary>
        /// The previous <see cref="FlatPattern"/> within the same <see cref="ContinuousPattern"/>.
        /// </summary>
        public FlatPattern? Previous => Index > 0 ? Parent.FlatPatterns[Index - 1] : null;

        /// <summary>
        /// The <see cref="TaikoDifficultyHitObjectRhythm.Ratio"/> of the first hit object in this pattern.
        /// </summary>
        public double Ratio => HitObjects[0].Rhythm.Ratio;

        public bool IsRepetitionOf(FlatPattern other)
        {
            return Math.Abs(Ratio / other.Ratio - 1) < 0.05 && HitObjects.Count == other.HitObjects.Count;
        }

        /// <summary>
        /// This should be called after the <see cref="Parent"/> is set, as alternating patterns only count within the
        /// same parent pattern.
        /// </summary>
        public void FindAlternatingIndex()
        {
            FlatPattern? alt = Previous?.Previous;
            FlatPattern? previousAlt = alt?.Previous;

            if (
                alt != null && alt.IsRepetitionOf(this) &&
                (previousAlt == null || Previous!.IsRepetitionOf(previousAlt)))
            {
                AlternatingIndex = alt.AlternatingIndex + 1;
            }
        }
    }
}
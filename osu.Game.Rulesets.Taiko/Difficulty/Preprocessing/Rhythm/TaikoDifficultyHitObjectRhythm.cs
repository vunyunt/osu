// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Rhythm.Data;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Rhythm
{
    /// <summary>
    /// Represents a rhythm change in a taiko map.
    /// </summary>
    public class TaikoDifficultyHitObjectRhythm
    {
        public FlatPattern? FlatPattern;

        public ContinuousPattern? ContinuousPattern;

        public RepeatingRhythmPattern? RepeatingRhythmPattern;

        /// <summary>
        /// The ratio of current <see cref="osu.Game.Rulesets.Difficulty.Preprocessing.DifficultyHitObject.DeltaTime"/>
        /// to previous <see cref="osu.Game.Rulesets.Difficulty.Preprocessing.DifficultyHitObject.DeltaTime"/> for the rhythm change.
        /// A <see cref="Ratio"/> above 1 indicates a slow-down; a <see cref="Ratio"/> below 1 indicates a speed-up.
        /// </summary>
        public readonly double Ratio;

        /// <summary>
        /// The ratio of hit window to the interval of the note.
        /// </summary>
        public readonly double Leniency;

        public TaikoDifficultyHitObjectRhythm(double hitWindow, TaikoDifficultyHitObject current)
        {
            var previous = current.Previous(0);

            if (previous == null)
            {
                Ratio = 1;
                Leniency = 1;

                return;
            }

            Ratio = current.DeltaTime / previous.DeltaTime;
            Leniency = hitWindow / current.DeltaTime;
        }
    }
}

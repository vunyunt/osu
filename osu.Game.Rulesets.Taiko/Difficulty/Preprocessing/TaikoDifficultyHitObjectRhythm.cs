using System;
// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing
{
    /// <summary>
    /// Represents a rhythm change in a taiko map.
    /// </summary>
    public class TaikoDifficultyHitObjectRhythm
    {
        /// <summary>
        /// The difficulty multiplier associated with this rhythm change.
        /// </summary>
        public readonly double Difficulty;

        /// <summary>
        /// The ratio of current <see cref="osu.Game.Rulesets.Difficulty.Preprocessing.DifficultyHitObject.DeltaTime"/>
        /// to previous <see cref="osu.Game.Rulesets.Difficulty.Preprocessing.DifficultyHitObject.DeltaTime"/> for the rhythm change.
        /// A <see cref="Ratio"/> above 1 indicates a slow-down; a <see cref="Ratio"/> below 1 indicates a speed-up.
        /// </summary>
        public readonly double Ratio;

        /// <summary>
        /// Creates an object representing a rhythm change.
        /// </summary>
        /// <param name="numerator">The numerator for <see cref="Ratio"/>.</param>
        /// <param name="denominator">The denominator for <see cref="Ratio"/></param>
        /// <param name="difficulty">The difficulty multiplier associated with this rhythm change.</param>
        public TaikoDifficultyHitObjectRhythm(int numerator, int denominator, double difficulty)
        {
            Ratio = numerator / (double)denominator;
            Difficulty = difficulty;
        }

        /// <summary>
        /// Creates an object representing a rhythm change. Difficulty is calculated from the ratio.
        /// </summary>
        public TaikoDifficultyHitObjectRhythm(double ratio) {
            Ratio = ratio;
            Difficulty = difficultyFromRatio(ratio);
        }

        /// <summary>
        /// Calculate difficulty from ratio
        /// </summary>
        private double difficultyFromRatio(double ratio)
        {
            // Sum of n = 8 terms of periodic penalty. A more common denominator will be penalized multiple time, hence
            // simpler rhythm change will be penalized more.
            // Note that to penalize 1/4 properly, a power-of-two n is required.

            // For offsetting the penalty so that a positive difficulty is given.
            double multiplierSum = 0;
            double difficulty = 0;
            for(int i = 0; i < 8; ++i) {
                double currentTermMultiplier = termMultiplier(i);
                difficulty += termPenalty(ratio, i, 8, currentTermMultiplier);
                multiplierSum += currentTermMultiplier;
            }

            return difficulty + multiplierSum;
        }

        /// <summary>
        /// Multiplier for a given denominator term.
        /// </summary>
        private double termMultiplier(int denominator) {
            return 1;
        }

        /// <summary>
        /// Calculate the penalty of a single denominator for the given ratio. The sum of the result of this method
        /// for multiple denominators is the total penalty for the given ratio.
        /// </summary>
        /// <param name="ratio">The ratio of current and previous <see cref="osu.Game.Rulesets.Difficulty.Preprocessing.DifficultyHitObject.DeltaTime" /></param>
        /// <param name="denominator">The denominator term to calculate the penalty for. A denominator of 3 means 1/3, 2/3, 3/3, 4/3 etc will be penalized</param>
        /// <param name="power">Power to raise to, higher power will result in a narrower penalization range.</param>
        /// <param name="multiplier">Multiplier for the denominator term.</param>
        private double termPenalty(double ratio, int denominator, double power, double multiplier) 
        {
            return multiplier * Math.Pow(Math.Cos(denominator * Math.PI * ratio), power);
        }
    }
}

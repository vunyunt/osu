// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Rhythm;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Rhythm.Data;

namespace osu.Game.Rulesets.Taiko.Difficulty.Evaluators
{
    public class RhythmEvaluator
    {
        // TODO: Share this sigmoid as it's used in colour evaluator. Should be done after tl tapping pr is merged.
        private static double invertedSigmoid(double val, double center, double width, double middle, double height)
        {
            double inverted = Math.Tanh(Math.E * -(val - center) / width);
            return inverted * (height / 2) + middle;
        }

        private static double sigmoid(double val, double center, double width, double middle, double height)
        {
            double inverted = invertedSigmoid(val, center, width, middle, height);
            return height - inverted;
        }

        /// <summary>
        /// Multiplier for a given denominator term.
        /// </summary>
        private static double termPenalty(double ratio, int denominator, double power, double multiplier)
        {
            return -multiplier * Math.Pow(Math.Cos(denominator * Math.PI * ratio), power);
        }

        /// <summary>
        /// Gives a bonus for target ratio using a bell-shaped function.
        /// </summary>
        private static double targetedBonus(double ratio, double targetRatio, double width, double multiplier)
        {
            // Gaussian function
            return multiplier * Math.Exp(Math.E * -(Math.Pow(ratio - targetRatio, 2) / Math.Pow(width, 2)));
        }

        private static double ratioDifficulty(double ratio)
        {
            // Sum of n = 8 terms of periodic penalty. A more common denominator will be penalized multiple time, hence
            // simpler rhythm change will be penalized more.
            // Note that to penalize 1/4 properly, a power-of-two n is required.

            // For offsetting the penalty so that a positive difficulty is given.
            double multiplierSum = 0;
            double difficulty = 0;

            for (int i = 1; i < 8; ++i)
            {
                double currentTermMultiplier = 2;
                difficulty += termPenalty(ratio, i, 2, currentTermMultiplier);
                multiplierSum += currentTermMultiplier;
            }

            difficulty += multiplierSum;

            // Give bonus to near-1 ratios
            difficulty += targetedBonus(ratio, 1, 0.5, 1);

            // Penalize ratios that are VERY near 1
            difficulty -= targetedBonus(ratio, 1, 0.3, 1);

            return difficulty;
        }

        private static double evaluateDifficultyOf(EvenHitObjects evenHitObjects, double hitWindow)
        {
            // Treat the pattern as a single note rhythm wise if it's shorter than one hit window.
            if (evenHitObjects.Duration < hitWindow)
                return 0;

            double intervalDifficulty = ratioDifficulty(evenHitObjects.HitObjectIntervalRatio);

            // Penalize patterns that can be played with the same interval as the previous pattern.
            double? previousInterval = evenHitObjects.Previous?.HitObjectInterval;
            if (previousInterval != null && evenHitObjects.Children.Count > 1)
            {
                double expectedDurationFromPrevious = (double)previousInterval * evenHitObjects.Children.Count;
                double durationDifference = Math.Abs(evenHitObjects.Duration - expectedDurationFromPrevious);
                intervalDifficulty *= 1 - sigmoid(durationDifference / hitWindow, 0.5, 1.5, 0.5, 1);
            }

            return intervalDifficulty;
        }

        private static double evaluateDifficultyOf(EvenPatterns evenPatterns)
        {
            return ratioDifficulty(evenPatterns.IntervalRatio);
        }

        public static double EvaluateDifficultyOf(DifficultyHitObject hitObject, double greatHitWindow)
        {
            TaikoDifficultyHitObjectRhythm rhythm = ((TaikoDifficultyHitObject)hitObject).Rhythm;
            double difficulty = 0.0d;

            if (rhythm.EvenHitObjects?.FirstHitObject == hitObject) // Difficulty for EvenHitObjects
                difficulty += evaluateDifficultyOf(rhythm.EvenHitObjects, greatHitWindow);
            if (rhythm.EvenPatterns?.FirstHitObject == hitObject) // Difficulty for EvenPatterns
                difficulty += evaluateDifficultyOf(rhythm.EvenPatterns);

            return difficulty;
        }
    }
}

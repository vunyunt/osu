// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Patterns;


namespace osu.Game.Rulesets.Taiko.Difficulty.Evaluators
{
    public class PatternEvaluator
    {
        private readonly double hitWindow;

        public PatternEvaluator(double hitWindow)
        {
            this.hitWindow = hitWindow;
        }

        /// <summary>
        /// Multiplier for a given denominator term.
        /// </summary>
        private double termPenalty(double ratio, int denominator, double power, double multiplier)
        {
            return -multiplier * Math.Pow(Math.Cos(denominator * Math.PI * ratio), power);
        }

        /// <summary>
        /// Gives a bonus for target ratio using a bell-shaped function.
        /// </summary>
        private double targetedBonus(double ratio, double targetRatio, double width, double multiplier)
        {
            return multiplier * Math.Exp(Math.E * -(Math.Pow(ratio - targetRatio, 2) / Math.Pow(width, 2)));
        }

        private double ratioDifficulty(double ratio, int terms = 8)
        {
            // Sum of n = 8 terms of periodic penalty. A more common denominator will be penalized multiple time, hence
            // simpler rhythm change will be penalized more.
            // Note that to penalize 1/4 properly, a power-of-two n is required.
            double difficulty = 0;

            for (int i = 1; i <= terms; ++i)
            {
                difficulty += termPenalty(ratio, i, 2, 1);
            }

            difficulty += terms;

            // Give bonus to near-1 ratios
            difficulty += targetedBonus(ratio, 1, 0.5, 1);

            // Penalize ratios that are VERY near 1
            difficulty -= targetedBonus(ratio, 1, 0.3, 1);

            // Penalize 1/2s specifically
            difficulty -= targetedBonus(ratio, 0.5, 0.1, 0.2);

            return difficulty / Math.Sqrt(8);
        }

        private double childrenIntervalPenalty<InnerChildrenType>(FlatRhythm<InnerChildrenType> pattern)
        where InnerChildrenType : IHasInterval
        {
            double? previousInterval = pattern.Previous?.Interval;

            if (previousInterval != null && !double.IsNaN(previousInterval.Value) && pattern.Children.Count > 1)
            {
                // The expected duration for the pattern if it has the same interval as the previous pattern.
                double expectedDurationFromPrevious = (double)previousInterval * (pattern.Children.Count - 1);

                // Calculate penalty multiplier based on the difference between the expected duration, actual duration, and hit window.
                double durationDifference = Math.Abs(pattern.Duration - expectedDurationFromPrevious);
                return MathEvaluator.Sigmoid(durationDifference / hitWindow, 1.5, 0.5, 0.5, 1);
            }

            return 1;
        }

        private double burstPenalty<InnerChildrenType>(FlatRhythm<InnerChildrenType> pattern)
        where InnerChildrenType : IHasInterval
        {
            // Penalize patterns that can be hit within a single hit window.
            return MathEvaluator.Sigmoid(pattern.Duration / hitWindow, 1, 0.5, 0.5, 1);
        }

        private double childrenIntervalRatio<InnerChildrenType>(FlatRhythm<InnerChildrenType> pattern)
        where InnerChildrenType : IHasInterval
        {
            FlatRhythm<InnerChildrenType>? previous = pattern.Previous as FlatRhythm<InnerChildrenType>;
            double childrenInterval = pattern.ChildrenInterval;
            double previousChildrenInterval = previous?.ChildrenInterval ?? double.NaN;

            if (double.IsNaN(childrenInterval) || double.IsNaN(previousChildrenInterval))
            {
                return 1;
            }

            return childrenInterval / previousChildrenInterval;
        }

        private double intervalRatio<InnerChildrenType>(FlatRhythm<InnerChildrenType> pattern)
        where InnerChildrenType : IHasInterval
        {
            FlatRhythm<InnerChildrenType>? previous = pattern.Previous as FlatRhythm<InnerChildrenType>;
            double interval = pattern.Interval;
            double previousInterval = previous?.Interval ?? double.NaN;

            if (double.IsNaN(interval) || double.IsNaN(previousInterval))
            {
                return 1;
            }

            return interval / previousInterval;
        }

        public double Evaluate<InnerChildrenType>(FlatRhythm<InnerChildrenType> pattern)
        where InnerChildrenType : IHasInterval
        {
            double childrenIntervalRatio = this.childrenIntervalRatio(pattern);
            double intervalRatio = this.intervalRatio(pattern);

            double intervalStrain = ratioDifficulty(intervalRatio);
            double childrenIntervalStrain = ratioDifficulty(childrenIntervalRatio);

            childrenIntervalStrain *= childrenIntervalPenalty(pattern);
            childrenIntervalStrain *= burstPenalty(pattern);

            return ratioDifficulty(intervalStrain + childrenIntervalStrain);
        }

        /// <summary>
        /// Calcaultes the number of previous mono patterns needed to have a even
        /// total number of notes.
        /// </summary>
        ///
        /// Note: A problem with this approach is that a single odd-numbered
        ///       mono with a bunch of even numbered monos will result in a very
        ///       long chain. Hence a limit of 3 patterns is applied
        private int patternsToEven(MonoPattern monoPattern)
        {
            int noteCount = 0;
            int patternCount = 0;
            MonoPattern? current = monoPattern;

            while (current != null && (noteCount == 0 || noteCount % 2 == 0))
            {
                patternCount += 1;

                // Restrict to 3 patterns max
                if (patternCount >= 3)
                {
                    return 3;
                }

                noteCount += current.Children.Count;
                current = current.Previous as MonoPattern;
            }

            return Math.Max(patternCount, 1);
        }

        public double Evaluate(TaikoDifficultyHitObject hitObject)
        {
            double total = 0;

            var pattern = hitObject.Pattern;
            if (pattern.FlatRhythmPattern != null)
            {
                total += 0.25 * Evaluate(pattern.FlatRhythmPattern);
            }

            if (pattern.SecondPassRhythmPattern != null)
            {
                total += 0.5 * Evaluate(pattern.SecondPassRhythmPattern);
            }

            if (pattern.ThirdPassRhythmPattern != null)
            {
                total += Evaluate(pattern.ThirdPassRhythmPattern);
            }

            if (pattern.MonoPattern != null && pattern.FlatRhythmPattern == null)
            {
                total += 0.2d * patternsToEven(pattern.MonoPattern);
            }

            if (pattern.SecondPassColourPattern != null)
            {
                total += 0.5 * Evaluate(pattern.SecondPassColourPattern);
            }

            if (pattern.ThirdPassRhythmPattern != null)
            {
                total += Evaluate(pattern.ThirdPassRhythmPattern);
            }

            return total;
        }
    }
}

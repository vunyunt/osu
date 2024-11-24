// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty.Evaluators
{
    public static class PatternEvaluator
    {
        public static double EvaluateDifficultyOf(
            TaikoDifficultyHitObject hitObject,
            TaikoPatternFields fields,
            double errorStandardDeviation,
            double hitWindowStandardDeviation)
        {
            if (hitObject.BaseObject is not Hit) return 0;

            double errorAmplitude = fields.RhythmField.GetAmplitude(hitObject.StartTime, errorStandardDeviation);
            double hitAmplitude = fields.RhythmField.GetAmplitude(hitObject.StartTime, hitWindowStandardDeviation);

            // System.Console.WriteLine("errorAmplitude: " + errorAmplitude + ", hitAmplitude: " + hitAmplitude);

            hitAmplitude = Math.Max(hitAmplitude, 0.1);
            return errorAmplitude / hitAmplitude;
        }
    }
}

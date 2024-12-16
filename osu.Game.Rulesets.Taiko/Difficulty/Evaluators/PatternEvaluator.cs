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

            double rhythmHitAmplitude = fields.RhythmField.GetAmplitude(hitObject.StartTime, hitWindowStandardDeviation);
            double centreHitAmplitude = fields.CentreField.GetAmplitude(hitObject.StartTime, hitWindowStandardDeviation);
            double rimHitAmplitude = fields.RimField.GetAmplitude(hitObject.StartTime, hitWindowStandardDeviation);
            double colourChangeAmplitude = fields.ColourChangeField.GetAmplitude(hitObject.StartTime, hitWindowStandardDeviation);

            double rhythmErrorAmplitude = fields.RhythmField.GetAmplitude(hitObject.StartTime, errorStandardDeviation);
            double colourChangeErrorAmplitude = fields.ColourChangeField.GetAmplitude(hitObject.StartTime, errorStandardDeviation);

            double hitAmplitude = Math.Sqrt(
                Math.Pow(centreHitAmplitude, 2) +
                Math.Pow(rimHitAmplitude, 2) +
                // Math.Pow(colourChangeAmplitude * 0.5, 2) +
                Math.Pow(rhythmHitAmplitude, 2));

            double errorAmplitude = Math.Sqrt(
                // Math.Pow(colourChangeErrorAmplitude * 0.5, 2) +
                Math.Pow(rhythmErrorAmplitude, 2));

            hitAmplitude = Math.Max(hitAmplitude, 0.1);
            return errorAmplitude / hitAmplitude;
        }
    }
}

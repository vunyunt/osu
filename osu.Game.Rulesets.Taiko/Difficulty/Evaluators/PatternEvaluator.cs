// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty.Evaluators
{
    public static class PatternEvaluator
    {
        private static double evaluateDifficultyForField(
            TaikoDifficultyHitObject hitObject,
            TaikoTimeField field,
            double errorStandardDeviation,
            double hitWindowStandardDeviation)
        {
            double hitAmplitude = field.GetAmplitude(hitObject.StartTime, hitWindowStandardDeviation);
            double errorAmplitude = field.GetAmplitude(hitObject.StartTime, errorStandardDeviation);

            hitAmplitude = Math.Max(0.1, hitAmplitude);
            double difficulty = errorAmplitude / hitAmplitude;
            // difficulty = Math.Max(0, difficulty);

            return difficulty;
        }

        public static double EvaluateDifficultyOf(
            TaikoDifficultyHitObject hitObject,
            TaikoPatternFields fields,
            double errorStandardDeviation,
            double hitWindowStandardDeviation)
        {
            if (hitObject.BaseObject is not Hit) return 0;

            // double rhythmErrorStandardDeviation = Math.Min(errorStandardDeviation, hitObject.DeltaTime);
            // rhythmErrorStandardDeviation = Math.Max(hitWindowStandardDeviation, rhythmErrorStandardDeviation);
            double rhythmDifficulty = evaluateDifficultyForField(hitObject, fields.RhythmField, errorStandardDeviation, hitWindowStandardDeviation);
            double colourChangeDifficulty = 0;

            if (hitObject.IsColourChange)
            {
                colourChangeDifficulty = evaluateDifficultyForField(hitObject, fields.ColourChangeField, errorStandardDeviation, hitWindowStandardDeviation);
                // colourChangeDifficulty *= 0.1;
            }
            else
            {
            }

            double centreDifficulty = evaluateDifficultyForField(hitObject, fields.CentreField, errorStandardDeviation, hitWindowStandardDeviation);
            double rimDifficulty = evaluateDifficultyForField(hitObject, fields.RimField, errorStandardDeviation, hitWindowStandardDeviation);

            // centreDifficulty *= 0.1;
            // rimDifficulty *= 0.1;

            // System.Console.WriteLine("{0},{1},{2},{3}", rhythmDifficulty, colourChangeDifficulty, centreDifficulty, rimDifficulty);

            return Math.Sqrt(rhythmDifficulty * rhythmDifficulty + colourChangeDifficulty * colourChangeDifficulty) + centreDifficulty + rimDifficulty;

            // return rhythmDifficulty;

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

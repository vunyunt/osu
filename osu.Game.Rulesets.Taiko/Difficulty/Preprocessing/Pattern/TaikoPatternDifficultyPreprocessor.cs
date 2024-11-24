// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Pattern
{
    public static class TaikoPatternDifficultyPreprocessor
    {
        private const int harmonics = 8;

        private const double cycles_count = 4;

        private const double decay_base = Math.E;

        private static void createAlignmentPoints(double time, double interval, TaikoTimeField timeField)
        {
            for (int i = 0; i < harmonics; i++)
            {
                double harmonicInterval = interval * Math.Pow(2, i);
                double harmonicAmplitude = Math.Pow(0.5, i);

                for (int j = 0; j < cycles_count * (i + 1); j++)
                {
                    double t = time + harmonicInterval * j;
                    double amplitude = harmonicAmplitude * Math.Pow(decay_base, j / (i + 1));
                    timeField.AddNode(t, amplitude);
                }
            }
        }

        private static void createRhythmAlignmentPoints(
            IEnumerable<TaikoDifficultyHitObject> hitObjects, TaikoTimeField timeField)
        {
            hitObjects
                .Where(hitObject => hitObject.DeltaTime > 0)
                .ForEach(hitObject => createAlignmentPoints(hitObject.StartTime, hitObject.DeltaTime, timeField));
        }

        private static void createColourAlignmentPoints(
            IEnumerable<TaikoDifficultyHitObject> hitObject, HitType type, TaikoTimeField timeField)
        {
            hitObject
                .Where(hitObject => (hitObject.BaseObject as Hit)?.Type == type)
                .SelectPair((previous, current) =>
                    (interval: current.StartTime - previous.StartTime, hitObject: current))
                .ForEach(x => createAlignmentPoints(x.hitObject.StartTime, x.interval, timeField));
        }


        private static void createCentreAlignmentPoints(
            IEnumerable<TaikoDifficultyHitObject> hitObject, TaikoPatternFields fields
        ) => createColourAlignmentPoints(hitObject, HitType.Centre, fields.CentreField);

        private static void createRimAlignmentPoints(
            IEnumerable<TaikoDifficultyHitObject> hitObject, TaikoPatternFields fields
        ) => createColourAlignmentPoints(hitObject, HitType.Rim, fields.RimField);

        private static void createColourChangeAlignmentPoints(
            IEnumerable<TaikoDifficultyHitObject> hitObject, TaikoPatternFields fields
        )
        {
            hitObject
            .Where(hitObject => hitObject.BaseObject is Hit)
            .SelectPair((previous, current) =>
            {
                var previousHit = (Hit)previous.BaseObject;
                var currentHit = (Hit)current.BaseObject;
                return previousHit.Type == currentHit.Type ? current : null;
            })
            .OfType<TaikoDifficultyHitObject>()
            .SelectPair(
                (previous, current) => (interval: current.StartTime - previous.StartTime, hitObject: current))
            .ForEach(x => createAlignmentPoints(x.hitObject.StartTime, x.interval, fields.ColourChangeField));
        }

        public static TaikoPatternFields ComputeFields(IEnumerable<TaikoDifficultyHitObject> hitObjects)
        {
            TaikoPatternFields fields = new TaikoPatternFields();

            createRhythmAlignmentPoints(hitObjects, fields.RhythmField);
            createCentreAlignmentPoints(hitObjects, fields);
            createRimAlignmentPoints(hitObjects, fields);
            createColourChangeAlignmentPoints(hitObjects, fields);

            return fields;
        }
    }
}

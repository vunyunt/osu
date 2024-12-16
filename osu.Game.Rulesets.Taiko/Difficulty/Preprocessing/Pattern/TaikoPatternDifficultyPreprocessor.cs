// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Pattern
{
    public class TaikoPatternDifficultyPreprocessor
    {
        private const int harmonics_count = 4;

        private const double harmonics_decay_base = 0.5;

        private const int cycles_count = 6;

        private const double cycles_decay_base = 0.5;

        private Dictionary<double, double> precomputedNodes = new Dictionary<double, double>();

        public TaikoPatternDifficultyPreprocessor()
        {
            const double base_interval = 1;

            for (int i = 0; i < harmonics_count; i++)
            {
                double harmonicInterval = base_interval / Math.Pow(2, i);
                double harmonicAmplitude = Math.Pow(harmonics_decay_base, i);

                for (int j = 0; j < cycles_count * (i + 1); j++)
                {
                    double t = harmonicInterval * j;
                    double amplitude = harmonicAmplitude * Math.Pow(cycles_decay_base, t);
                    amplitude /= cycles_count;
                    if (precomputedNodes.ContainsKey(t))
                    {
                        precomputedNodes[t] += amplitude;
                    }
                    else
                    {
                        precomputedNodes.TryAdd(t, amplitude);
                    }
                }
            }
        }

        private void createAlignmentPoints(double time, double interval, TaikoTimeField timeField)
        {
            foreach (var (t, amplitude) in precomputedNodes)
            {
                double scaledT = t * interval;
                timeField.AddNode(scaledT + time, amplitude);
            }
        }

        private void createRhythmAlignmentPoints(
            IEnumerable<TaikoDifficultyHitObject> hitObjects, TaikoTimeField timeField)
        {
            hitObjects
                .Where(hitObject => hitObject.DeltaTime > 0)
                .ForEach(hitObject => createAlignmentPoints(hitObject.StartTime, hitObject.DeltaTime, timeField));
        }

        private void createColourAlignmentPoints(
            IEnumerable<TaikoDifficultyHitObject> hitObject, HitType type, TaikoTimeField timeField)
        {
            hitObject
                .Where(hitObject => (hitObject.BaseObject as Hit)?.Type == type)
                .SelectPair((previous, current) =>
                    (interval: current.StartTime - previous.StartTime, hitObject: current))
                .ForEach(x => createAlignmentPoints(x.hitObject.StartTime, x.interval, timeField));
        }


        private void createCentreAlignmentPoints(
            IEnumerable<TaikoDifficultyHitObject> hitObject, TaikoPatternFields fields
        ) => createColourAlignmentPoints(hitObject, HitType.Centre, fields.CentreField);

        private void createRimAlignmentPoints(
            IEnumerable<TaikoDifficultyHitObject> hitObject, TaikoPatternFields fields
        ) => createColourAlignmentPoints(hitObject, HitType.Rim, fields.RimField);

        private void createColourChangeAlignmentPoints(
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

        public TaikoPatternFields ComputeFields(IEnumerable<TaikoDifficultyHitObject> hitObjects)
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

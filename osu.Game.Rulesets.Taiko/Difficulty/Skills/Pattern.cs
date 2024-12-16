// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.


using System;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Difficulty.Evaluators;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Scoring;

namespace osu.Game.Rulesets.Taiko.Difficulty.Skills
{
    public class Pattern : StrainDecaySkill
    {
        protected override double SkillMultiplier => 0.04;

        protected override double StrainDecayBase => 0;

        private TaikoPatternFields? fields;

        private double? rhythmErrorStandardDeviation;

        private double? hitWindowStandardDeviation;

        public Pattern(Mod[] mods) : base(mods) { }

        public void Initialize(IBeatmap beatmap, TaikoPatternFields fields)
        {
            this.fields = fields;

            HitWindows hitWindows = new TaikoHitWindows();
            hitWindows.SetDifficulty(beatmap.Difficulty.OverallDifficulty);
            hitWindowStandardDeviation = hitWindows.WindowFor(HitResult.Great);
            rhythmErrorStandardDeviation = 1000 + hitWindowStandardDeviation;
        }

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            if (fields == null || hitWindowStandardDeviation == null || rhythmErrorStandardDeviation == null)
            {
                throw new InvalidOperationException("Pattern skill class has not been initialized. Please call Initialize first.");
            }

            return PatternEvaluator.EvaluateDifficultyOf(
                (TaikoDifficultyHitObject)current,
                fields,
                (double)rhythmErrorStandardDeviation,
                (double)hitWindowStandardDeviation);
        }
    }
}

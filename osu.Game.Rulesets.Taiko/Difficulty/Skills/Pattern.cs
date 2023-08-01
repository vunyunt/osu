// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Difficulty.Evaluators;

namespace osu.Game.Rulesets.Taiko.Difficulty.Skills
{
    public class Pattern : StrainDecaySkill
    {
        private PatternEvaluator evaluator;

        public Pattern(Mod[] mods, double hitWindow) : base(mods)
        {
            evaluator = new(hitWindow);
        }

        protected override double SkillMultiplier => 0.35;

        protected override double StrainDecayBase => 0.4;

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            return evaluator.Evaluate((TaikoDifficultyHitObject)current);
        }
    }
}

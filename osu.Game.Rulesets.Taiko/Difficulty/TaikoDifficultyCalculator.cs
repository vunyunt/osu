// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Colour;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Patterns;
using osu.Game.Rulesets.Taiko.Difficulty.Skills;
using osu.Game.Rulesets.Taiko.Mods;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Scoring;

namespace osu.Game.Rulesets.Taiko.Difficulty
{
    public class TaikoDifficultyCalculator : DifficultyCalculator
    {
        public override int Version => 20220902;

        private readonly IWorkingBeatmap workingBeatmap;

        public TaikoDifficultyCalculator(IRulesetInfo ruleset, IWorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
            workingBeatmap = beatmap;
        }

        protected override Skill[] CreateSkills(IBeatmap beatmap, Mod[] mods, double clockRate)
        {
            HitWindows hitWindows = new HitWindows();
            hitWindows.SetDifficulty(beatmap.Difficulty.OverallDifficulty);

            return new Skill[]
            {
                new Peaks(mods, hitWindows.WindowFor(HitResult.Great) / clockRate)
            };
        }

        protected override Mod[] DifficultyAdjustmentMods => new Mod[]
        {
            new TaikoModDoubleTime(),
            new TaikoModHalfTime(),
            new TaikoModEasy(),
            new TaikoModHardRock(),
        };

        protected override IEnumerable<DifficultyHitObject> CreateDifficultyHitObjects(IBeatmap beatmap, double clockRate)
        {
            HitWindows hitWindows = new HitWindows();
            hitWindows.SetDifficulty(beatmap.Difficulty.OverallDifficulty);

            // List<DifficultyHitObject> is required for HitObject's constructor
            List<DifficultyHitObject> difficultyHitObjects = new List<DifficultyHitObject>();
            // This should contain identical items to difficultyHitObjects. This is to avoid repeated casting.
            List<TaikoDifficultyHitObject> taikoDifficultyHitObjects = new List<TaikoDifficultyHitObject>();
            List<TaikoDifficultyHitObject> centreObjects = new List<TaikoDifficultyHitObject>();
            List<TaikoDifficultyHitObject> rimObjects = new List<TaikoDifficultyHitObject>();
            List<TaikoDifficultyHitObject> noteObjects = new List<TaikoDifficultyHitObject>();

            for (int i = 2; i < beatmap.HitObjects.Count; i++)
            {
                TaikoDifficultyHitObject hitObject = new TaikoDifficultyHitObject(
                        beatmap.HitObjects[i], beatmap.HitObjects[i - 1], beatmap.HitObjects[i - 2], clockRate,
                        difficultyHitObjects, centreObjects, rimObjects, noteObjects, difficultyHitObjects.Count);
                difficultyHitObjects.Add(hitObject);
                taikoDifficultyHitObjects.Add(hitObject);
            }

            TaikoColourDifficultyPreprocessor.ProcessAndAssign(difficultyHitObjects);
            new TaikoPatternPreprocessor(hitWindows).ProcessAndAssign(taikoDifficultyHitObjects);

            return difficultyHitObjects;
        }

        protected override DifficultyAttributes CreateDifficultyAttributes(IBeatmap beatmap, Mod[] mods, Skill[] skills, double clockRate)
        {
            if (beatmap.HitObjects.Count == 0)
                return new TaikoDifficultyAttributes { Mods = mods };

            var combined = (Peaks)skills[0];

            double combinedRating = combined.DifficultyValue();
            double starRating = rescale(combinedRating);

            // These have to be read after combined.DifficultyValue() is set
            double patternRating = combined.PatternStat;
            double rhythmRating = combined.RhythmStat;
            double staminaRating = combined.StaminaStat;

            HitWindows hitWindows = new TaikoHitWindows();
            hitWindows.SetDifficulty(beatmap.Difficulty.OverallDifficulty);

            TaikoDifficultyAttributes attributes = new TaikoDifficultyAttributes
            {
                StarRating = starRating,
                Mods = mods,
                StaminaDifficulty = staminaRating,
                PatternDifficulty = patternRating,
                PeakDifficulty = combinedRating,
                GreatHitWindow = hitWindows.WindowFor(HitResult.Great) / clockRate,
                MaxCombo = beatmap.HitObjects.Count(h => h is Hit),
            };

            if (ComputeLegacyScoringValues)
            {
                TaikoLegacyScoreSimulator sv1Simulator = new TaikoLegacyScoreSimulator();
                sv1Simulator.Simulate(workingBeatmap, beatmap, mods);
                attributes.LegacyAccuracyScore = sv1Simulator.AccuracyScore;
                attributes.LegacyComboScore = sv1Simulator.ComboScore;
                attributes.LegacyBonusScoreRatio = sv1Simulator.BonusScoreRatio;
            }

            return attributes;
        }

        /// <summary>
        /// Applies a final re-scaling of the star rating.
        /// </summary>
        /// <param name="sr">The raw star rating value before re-scaling.</param>
        private double rescale(double sr)
        {
            if (sr < 0) return sr;

            return 10.43 * Math.Log(sr / 8 + 1);
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Difficulty.Evaluators;

namespace osu.Game.Rulesets.Taiko.Difficulty.Skills
{
    // TODO: Not sure where this should go, but it's only used in Peak so I'm
    //       placing it here for now
    public class TaikoStrain : IComparable<TaikoStrain>
    {
        private const double final_multiplier = 0.177;
        private const double pattern_skill_multiplier = 0.55 * final_multiplier;
        private const double stamina_skill_multiplier = 0.45 * final_multiplier;

        public readonly double Pattern;
        public readonly double Stamina;
        public readonly double Combined;

        public TaikoStrain(double colour, double stamina)
        {
            Pattern = colour * pattern_skill_multiplier;
            Stamina = stamina * stamina_skill_multiplier;
            Combined = MathEvaluator.Norm(3, Pattern, Stamina);
        }

        int IComparable<TaikoStrain>.CompareTo(TaikoStrain? other)
        {
            if (other == null)
                return 1;

            return Combined.CompareTo(other.Combined);
        }
    }

    public class Peaks : Skill
    {
        private readonly Pattern pattern;
        private readonly Stamina stamina;

        // These stats are only defined after DifficultyValue() is called
        public double RhythmStat { get; private set; }
        public double PatternStat { get; private set; }
        public double StaminaStat { get; private set; }

        public Peaks(Mod[] mods, double greatHitWindow)
            : base(mods)
        {
            pattern = new Pattern(mods, greatHitWindow);
            stamina = new Stamina(mods);
        }

        public override void Process(DifficultyHitObject current)
        {
            pattern.Process(current);
            stamina.Process(current);
        }

        /// <summary>
        /// Returns the combined star rating of the beatmap, calculated using peak strains from all sections of the map.
        /// </summary>
        /// <remarks>
        /// For each section, the peak strains of all separate skills are combined into a single peak strain for the section.
        /// The resulting partial rating of the beatmap is a weighted sum of the combined peaks (higher peaks are weighted more).
        /// </remarks>
        public override double DifficultyValue()
        {
            List<TaikoStrain> peaks = new List<TaikoStrain>();

            var patternPeaks = pattern.GetCurrentStrainPeaks().ToList();
            var staminaPeaks = stamina.GetCurrentStrainPeaks().ToList();

            for (int i = 0; i < patternPeaks.Count; i++)
            {
                TaikoStrain peak = new TaikoStrain(patternPeaks[i], staminaPeaks[i]);

                // Sections with 0 strain are excluded to avoid worst-case time complexity of the following sort (e.g. /b/2351871).
                // These sections will not contribute to the difficulty.
                if (peak.Combined > 0)
                    peaks.Add(peak);
            }

            double combinedDifficulty = 0;
            double patternDifficulty = 0;
            double staminaDifficulty = 0;
            double weight = 1;

            foreach (TaikoStrain strain in peaks.OrderByDescending(d => d))
            {
                combinedDifficulty += strain.Combined * weight;
                patternDifficulty += strain.Pattern * weight;
                staminaDifficulty += strain.Stamina * weight;
                weight *= 0.9;
            }

            PatternStat = patternDifficulty;
            StaminaStat = staminaDifficulty;

            return combinedDifficulty;
        }
    }
}

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
        private const double final_multiplier = 0.15;
        private const double rhythm_skill_multiplier = 0.33 * final_multiplier;
        private const double colour_skill_multiplier = 0.33 * final_multiplier;
        private const double stamina_skill_multiplier = 0.42 * final_multiplier;

        public readonly double Colour;
        public readonly double Rhythm;
        public readonly double Stamina;
        public readonly double Combined;

        public TaikoStrain(double colour, double rhythm, double stamina)
        {
            Colour = colour * colour_skill_multiplier;
            Rhythm = rhythm * rhythm_skill_multiplier;
            Stamina = stamina * stamina_skill_multiplier;
            Combined = MathEvaluator.Norm(2, Colour, Rhythm, Stamina);
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
        private readonly Rhythm rhythm;
        private readonly Colour colour;
        private readonly Stamina stamina;

        // These stats are only defined after DifficultyValue() is called
        public double RhythmStat { get; private set; }
        public double ColourStat { get; private set; }
        public double StaminaStat { get; private set; }

        public Peaks(Mod[] mods, double greatHitWindow)
            : base(mods)
        {
            rhythm = new Rhythm(mods, greatHitWindow);
            colour = new Colour(mods);
            stamina = new Stamina(mods);
        }

        public override void Process(DifficultyHitObject current)
        {
            rhythm.Process(current);
            colour.Process(current);
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

            var colourPeaks = colour.GetCurrentStrainPeaks().ToList();
            var rhythmPeaks = rhythm.GetCurrentStrainPeaks().ToList();
            var staminaPeaks = stamina.GetCurrentStrainPeaks().ToList();

            for (int i = 0; i < colourPeaks.Count; i++)
            {
                TaikoStrain peak = new TaikoStrain(colourPeaks[i], rhythmPeaks[i], staminaPeaks[i]);

                // Sections with 0 strain are excluded to avoid worst-case time complexity of the following sort (e.g. /b/2351871).
                // These sections will not contribute to the difficulty.
                if (peak.Combined > 0)
                    peaks.Add(peak);
            }

            double combinedDifficulty = 0;
            double colourDifficulty = 0;
            double rhythmDifficulty = 0;
            double staminaDifficulty = 0;
            double weight = 1;

            foreach (TaikoStrain strain in peaks.OrderByDescending(d => d))
            {
                combinedDifficulty += strain.Combined * weight;
                colourDifficulty += strain.Colour * weight;
                rhythmDifficulty += strain.Rhythm * weight;
                staminaDifficulty += strain.Stamina * weight;
                weight *= 0.9;
            }

            RhythmStat = rhythmDifficulty;
            ColourStat = colourDifficulty;
            StaminaStat = staminaDifficulty;

            return combinedDifficulty;
        }
    }
}

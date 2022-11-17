using System.Collections.Generic;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Rhythm.Data;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Rhythm
{
    public class TaikoRhythmDifficultyPreprocessor
    {
        // TODO: With the Encode functions moved to the respective data classes, this class now seems somewhat redundant.
        //       If we can find a way to move the Encode calls to somewhere else (perhaps just in TaikoDifficultyCalculator?)
        //       we can remove this class.
        public static void ProcessAndAssign(List<TaikoDifficultyHitObject> hitObjects)
        {
            List<EvenHitObjects> flatPatterns = EvenHitObjects.GroupHitObjects(hitObjects);
            List<EvenPatterns> evenPatterns = EvenPatterns.GroupEvenPatterns(flatPatterns);
        }
    }
}
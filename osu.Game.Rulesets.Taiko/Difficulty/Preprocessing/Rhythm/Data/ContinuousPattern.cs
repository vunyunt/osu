using System.Linq;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Rhythm.Data
{
    /// <summary>
    /// A continuous pattern is defined as a sequence of continuous hit object that doesn't contain any rhythm ratio of
    /// 2 or above (representing a gap of a 2x slowdown).
    /// </summary>
    public class ContinuousPattern
    {
        public List<FlatPattern> FlatPatterns { get; private set; } = new List<FlatPattern>();

        public RepeatingRhythmPattern Parent = null!;

        public int Index;

        public int Length;

        public IEnumerable<TaikoDifficultyHitObject> HitObjects => FlatPatterns.SelectMany(pattern => pattern.HitObjects);

        public bool IsRepetitionOf(ContinuousPattern other)
        {
            if (FlatPatterns.Count != other.FlatPatterns.Count)
                return false;

            for (int i = 0; i < FlatPatterns.Count; i++)
            {
                if (!FlatPatterns[i].IsRepetitionOf(other.FlatPatterns[i]))
                    return false;
            }

            return true;
        }
    }
}
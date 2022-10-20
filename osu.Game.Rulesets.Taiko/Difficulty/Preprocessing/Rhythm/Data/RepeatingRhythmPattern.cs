using System;
using System.Linq;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Rhythm.Data
{
    public class RepeatingRhythmPattern
    {
        private const int max_repetition_interval = 32;

        public List<FlatPattern> FlatPatterns { get; private set; } = new List<FlatPattern>();

        public TaikoDifficultyHitObject FirstHitObject => FlatPatterns.First().FirstHitObject;

        public RepeatingRhythmPattern? Previous;

        public int Length = 0;

        public int RepetitionInterval;

        public bool IsRepetitionOf(RepeatingRhythmPattern other)
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

        public void FindRepetitionInterval()
        {
            RepeatingRhythmPattern? current = Previous;
            int interval = 1;

            while (interval < max_repetition_interval && current != null)
            {
                interval += current.Length;

                if (current.IsRepetitionOf(this))
                {
                    RepetitionInterval = Math.Min(interval, max_repetition_interval);
                    return;
                }

                current = current.Previous;
            }

            RepetitionInterval = max_repetition_interval;
        }
    }
}
using System;
using System.Linq;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Rhythm.Data
{
    public class RepeatingRhythmPattern
    {
        private const int max_repetition_interval = 32;

        public List<ContinuousPattern> ContinuousPatterns { get; private set; } = new List<ContinuousPattern>();

        public TaikoDifficultyHitObject FirstHitObject => ContinuousPatterns.First().FirstHitObject;

        public RepeatingRhythmPattern? Previous;

        public int Length => ContinuousPatterns[0].Length * ContinuousPatterns.Count;

        public int RepetitionInterval;

        public bool IsRepetitionOf(RepeatingRhythmPattern other)
        {
            if (ContinuousPatterns.Count != other.ContinuousPatterns.Count)
                return false;

            for (int i = 0; i < ContinuousPatterns.Count; i++)
            {
                if (!ContinuousPatterns[i].IsRepetitionOf(other.ContinuousPatterns[i]))
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
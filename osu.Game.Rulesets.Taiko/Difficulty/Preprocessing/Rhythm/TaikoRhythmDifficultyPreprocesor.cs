using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Rhythm.Data;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Rhythm
{
    public class TaikoRhythmDifficultyPreprocessor
    {
        public static void ProcessAndAssign(List<DifficultyHitObject> hitObjects)
        {
            List<FlatPattern> flatPatterns = encodeFlatPattern(hitObjects);
            List<ContinuousPattern> continuousPatterns = encodeContinuousPattern(flatPatterns);
            List<RepeatingRhythmPattern> repeatingRhythmPatterns = encodeRepeatingRhythmPattern(continuousPatterns);

            Console.WriteLine(flatPatterns.Count);
        }

        private static List<FlatPattern> encodeFlatPattern(List<DifficultyHitObject> data)
        {
            List<FlatPattern> flatPatterns = new List<FlatPattern>();
            FlatPattern? currentPattern = null;

            data.ForEach(hitObject =>
            {
                TaikoDifficultyHitObject taikoHitObject = (TaikoDifficultyHitObject)hitObject;

                if (currentPattern == null || Math.Abs(taikoHitObject.Rhythm.Ratio - 1) > 0.1)
                {
                    currentPattern = new FlatPattern();
                    flatPatterns.Add(currentPattern);
                }

                taikoHitObject.Rhythm.FlatPattern = currentPattern;
                currentPattern.HitObjects.Add(taikoHitObject);
            });

            return flatPatterns;
        }

        private static List<ContinuousPattern> encodeContinuousPattern(List<FlatPattern> data)
        {
            List<ContinuousPattern> continuousPatterns = new List<ContinuousPattern>();
            ContinuousPattern? currentPattern = null;

            data.ForEach(flatPattern =>
            {
                if (currentPattern == null || flatPattern.Ratio < 1.9)
                {
                    currentPattern = new ContinuousPattern();
                    continuousPatterns.Add(currentPattern);
                }

                flatPattern.Parent = currentPattern;
                flatPattern.Index = currentPattern.FlatPatterns.Count;
                flatPattern.HitObjects.ForEach(hitObject => hitObject.Rhythm.ContinuousPattern = currentPattern);
                currentPattern.FlatPatterns.Add(flatPattern);
                currentPattern.Length += flatPattern.HitObjects.Count;
            });

            return continuousPatterns;
        }

        private static List<RepeatingRhythmPattern> encodeRepeatingRhythmPattern(List<ContinuousPattern> data)
        {
            List<RepeatingRhythmPattern> repeatingRhythmPatterns = new List<RepeatingRhythmPattern>();
            RepeatingRhythmPattern? currentPattern = null;

            data.ForEach(continuousPattern =>
            {
                if (currentPattern == null || !continuousPattern.IsRepetitionOf(currentPattern.ContinuousPatterns[0]))
                {
                    currentPattern = new RepeatingRhythmPattern();
                    repeatingRhythmPatterns.Add(currentPattern);
                }

                continuousPattern.Parent = currentPattern;
                continuousPattern.Index = currentPattern.ContinuousPatterns.Count;
                foreach (TaikoDifficultyHitObject hitObject in continuousPattern.HitObjects)
                {
                    hitObject.Rhythm.RepeatingRhythmPattern = currentPattern;
                }
                currentPattern.ContinuousPatterns.Add(continuousPattern);
            });

            return repeatingRhythmPatterns;
        }
    }
}
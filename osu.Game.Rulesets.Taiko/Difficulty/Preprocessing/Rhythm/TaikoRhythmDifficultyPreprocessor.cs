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
            List<RepeatingRhythmPattern> repeatingRhythmPatterns = encodeRepeatingRhythmPattern(flatPatterns);

            // repeatingRhythmPatterns.ForEach(item =>
            // {
            //     Console.WriteLine("NEW REPEATING RHYTHM PATTERN");
            //     item.FlatPatterns.ForEach(item2 =>
            //     {
            //         Console.WriteLine("NEW FLAT PATTERN");
            //         item2.HitObjects.ForEach(item3 =>
            //         {
            //             Console.WriteLine($"{item3.StartTime}, {item3.DeltaTime}");
            //         });
            //     });
            // });
        }

        private static void bind(TaikoDifficultyHitObject hitObject, FlatPattern pattern)
        {
            hitObject.Rhythm.FlatPattern = pattern;
            pattern.HitObjects.Add(hitObject);
        }

        // Detect if the next note will have an interval lower than the current one. We are allowing a margin of error
        // of 3ms for good measure.
        private static bool willSpeedUp(TaikoDifficultyHitObject hitObject)
        {
            return (hitObject.NextNote(0)?.DeltaTime ?? double.MaxValue) < (hitObject.DeltaTime - 3);
        }

        private static List<FlatPattern> encodeFlatPattern(List<DifficultyHitObject> data)
        {
            List<FlatPattern> flatPatterns = new List<FlatPattern>();
            FlatPattern? currentPattern = null;
            var enumerator = data.GetEnumerator();

            while (enumerator.MoveNext())
            {
                TaikoDifficultyHitObject taikoHitObject = (TaikoDifficultyHitObject)enumerator.Current!;

                // A rhythm change is considered to have occured if the delta time difference between the current object
                // and the previous object is greater than 3ms. This is to account for the fact that note timing is
                // stored in ms, hence will have a margin of error of up to 2ms (1ms either way for each note). We are
                // adding a little bit more for good measure.
                bool rhythmChanged = Math.Abs(taikoHitObject.DeltaTime - taikoHitObject.Previous(0)?.DeltaTime ?? 0) > 3;

                if (currentPattern == null || rhythmChanged || willSpeedUp(taikoHitObject))
                {
                    currentPattern = new FlatPattern
                    {
                        Previous = currentPattern
                    };
                    flatPatterns.Add(currentPattern);

                    TaikoDifficultyHitObject? nextNote = taikoHitObject.NextNote(0);

                    // If the next next note is not a speed up, skip the check and add it to the current pattern. This
                    // is because we want to always group notes to the faster pattern.
                    if (nextNote != null && !willSpeedUp(nextNote))
                    {
                        // Bind the current object. Binding of the next object will be handled outside of the conditionals
                        // after moving the enumerator.
                        bind(taikoHitObject, currentPattern);

                        if (!enumerator.MoveNext()) break;

                        taikoHitObject = (TaikoDifficultyHitObject)enumerator.Current!;
                    }
                }

                bind(taikoHitObject, currentPattern);
            }

            enumerator.Dispose();
            return flatPatterns;
        }

        private static void bind(FlatPattern pattern, RepeatingRhythmPattern parent)
        {
            pattern.Parent = parent;
            pattern.Index = parent.FlatPatterns.Count;
            parent.FlatPatterns.Add(pattern);
            parent.Length = pattern.HitObjects.Count;

            foreach (TaikoDifficultyHitObject hitObject in pattern.HitObjects)
            {
                hitObject.Rhythm.RepeatingRhythmPattern = parent;
            }
        }

        private static List<RepeatingRhythmPattern> encodeRepeatingRhythmPattern(List<FlatPattern> data)
        {
            List<RepeatingRhythmPattern> repeatingRhythmPatterns = new List<RepeatingRhythmPattern>();
            RepeatingRhythmPattern? currentPattern = null;

            data.ForEach(flatPattern =>
            {
                if (currentPattern == null || !flatPattern.IsRepetitionOf(currentPattern.FlatPatterns[0]))
                {
                    currentPattern = new RepeatingRhythmPattern
                    {
                        Previous = currentPattern
                    };
                    currentPattern.Previous?.FindRepetitionInterval();
                    repeatingRhythmPatterns.Add(currentPattern);
                }

                bind(flatPattern, currentPattern);
            });

            // Find repetition interval for the final pattern
            currentPattern?.FindRepetitionInterval();

            return repeatingRhythmPatterns;
        }
    }
}
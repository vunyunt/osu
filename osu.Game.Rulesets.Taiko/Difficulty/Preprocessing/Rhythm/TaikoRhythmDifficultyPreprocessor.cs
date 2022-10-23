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
            List<RepeatingPattern> repeatingRhythmPatterns = encodeRepeatingRhythmPattern(flatPatterns);

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
            var enumerator = data.GetEnumerator();

            while (enumerator.MoveNext())
            {
                TaikoDifficultyHitObject taikoHitObject = (TaikoDifficultyHitObject)enumerator.Current!;

                // A rhythm change is considered to have occured if the delta time difference between the current object
                // and the previous object is greater than 3ms. This is to account for the fact that note timing is
                // stored in ms, hence will have a margin of error of up to 2ms (1ms either way for each note). We are
                // adding a little bit more for good measure.
                bool rhythmChanged = Math.Abs(taikoHitObject.DeltaTime - taikoHitObject.Previous(0)?.DeltaTime ?? 0) > 3;

                if (flatPatterns.Count == 0 || rhythmChanged || willSpeedUp(taikoHitObject))
                {
                    flatPatterns.Add(new FlatPattern(flatPatterns, flatPatterns.Count));

                    TaikoDifficultyHitObject? nextNote = taikoHitObject.NextNote(0);

                    // If the next next note is not a speed up, skip the check and add it to the current pattern. This
                    // is because we want to always group notes to the faster pattern.
                    if (nextNote != null && !willSpeedUp(nextNote))
                    {
                        // Bind the current object. Binding of the next object will be handled outside of the conditionals
                        // after moving the enumerator.
                        bind(taikoHitObject, flatPatterns[^1]);

                        if (!enumerator.MoveNext()) break;

                        taikoHitObject = (TaikoDifficultyHitObject)enumerator.Current!;
                    }
                }

                bind(taikoHitObject, flatPatterns[^1]);
            }

            enumerator.Dispose();
            return flatPatterns;
        }

        private static void bind(FlatPattern pattern, RepeatingPattern parent)
        {
            pattern.Parent = parent;
            pattern.RepetitionIndex = parent.FlatPatterns.Count / parent.RepetitionLength;
            parent.FlatPatterns.Add(pattern);
            parent.Length = pattern.HitObjects.Count;

            foreach (TaikoDifficultyHitObject hitObject in pattern.HitObjects)
            {
                hitObject.Rhythm.RepeatingPattern = parent;
            }
        }

        /// <summary>
        /// Determines if the given <see cref="FlatPattern"/> should be appended to the given <see cref="RepeatingPattern"/>.
        /// </summary>
        private static bool shouldAppend(FlatPattern flatPattern, RepeatingPattern repeatingRhythmPattern)
        {
            // Currently this should never be the case, but we are adding this check just in case.
            if (repeatingRhythmPattern.FlatPatterns.Count == 0)
            {
                return true;
            }

            if (repeatingRhythmPattern.FlatPatterns.Count == 1)
            {
                if (repeatingRhythmPattern.FlatPatterns[0].IsRepetitionOf(flatPattern))
                {
                    repeatingRhythmPattern.RepetitionLength = 1;
                    return true;
                }

                if (repeatingRhythmPattern.FlatPatterns[0].IsRepetitionOf(flatPattern.Next(0)))
                {
                    repeatingRhythmPattern.RepetitionLength = 2;
                    return true;
                }

                return false;
            }

            // We only check the second final pattern because in both the cases of single repeating flat patterns and
            // two alternating flat patterns, the second final pattern will be the same as the one passed in.
            return repeatingRhythmPattern.FlatPatterns[^2].IsRepetitionOf(flatPattern);
        }

        private static List<RepeatingPattern> encodeRepeatingRhythmPattern(List<FlatPattern> data)
        {
            List<RepeatingPattern> repeatingRhythmPatterns = new List<RepeatingPattern>();

            data.ForEach(flatPattern =>
            {
                if (repeatingRhythmPatterns.Count == 0 || !shouldAppend(flatPattern, repeatingRhythmPatterns[^1]))
                {
                    repeatingRhythmPatterns.Add(new RepeatingPattern(repeatingRhythmPatterns, repeatingRhythmPatterns.Count));
                    repeatingRhythmPatterns[^1].Previous(0)?.FindRepetitionInterval();
                }

                bind(flatPattern, repeatingRhythmPatterns[^1]);
            });

            // Find repetition interval for the final pattern
            if (repeatingRhythmPatterns.Count != 0)
            {
                repeatingRhythmPatterns[^1].FindRepetitionInterval();
            }

            return repeatingRhythmPatterns;
        }
    }
}
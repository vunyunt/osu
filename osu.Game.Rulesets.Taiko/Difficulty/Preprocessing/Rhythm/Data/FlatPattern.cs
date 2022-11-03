using System;
using System.Linq;
using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Rhythm.Data
{
    /// <summary>
    /// A flat pattern is defined as a sequence of hit objects that has effectively no variation in rhythm, i.e. all
    /// hit object within will have rhythm ratios of almost 1, with the exception of the first two hit objects.
    /// </summary>
    public class FlatPattern
    {
        private readonly List<FlatPattern> allPatterns;

        /// <summary>
        /// The index of this <see cref="FlatPattern"/> in the list of all <see cref="FlatPattern"/>s.
        /// </summary>
        public readonly int Index;

        /// <summary>
        /// The parent <see cref="RepeatingPattern"/> of this <see cref="FlatPattern"/>.
        /// </summary>
        public RepeatingPattern Parent = null!;

        /// <summary>
        /// The amount of times this <see cref="FlatPattern"/> is repeated in the parent <see cref="RepeatingPattern"/>.
        /// </summary>
        public int RepetitionIndex;

        /// <summary>
        /// All <see cref="TaikoDifficultyHitObject"/>s that are part of this <see cref="FlatPattern"/>.
        /// </summary>
        public List<TaikoDifficultyHitObject> HitObjects { get; private set; } = new List<TaikoDifficultyHitObject>();

        public TaikoDifficultyHitObject FirstHitObject => HitObjects.First();

        /// <summary>
        /// Start time of the first hit object.
        /// </summary>
        public double StartTime => HitObjects.First().StartTime;

        /// <summary>
        /// The interval between the first and last hit object
        /// </summary>
        public double Duration => HitObjects.Last().StartTime - HitObjects.First().StartTime;

        public FlatPattern? Previous(int backwardsIndex) => allPatterns.ElementAtOrDefault(Index - (backwardsIndex + 1));

        public FlatPattern? Next(int forwardsIndex) => allPatterns.ElementAtOrDefault(Index + (forwardsIndex + 1));

        /// <summary>
        /// The <see cref="TaikoDifficultyHitObjectRhythm.Ratio"/> of the first hit object in this pattern.
        /// </summary>
        public double Ratio => HitObjects[0].Rhythm.Ratio;

        /// <summary>
        /// The interval in ms of each hit object in this <see cref="FlatPattern"/>. This is only defined if there is
        /// more than two hit objects in this <see cref="FlatPattern"/>.
        /// </summary>
        public double? HitObjectInterval = null;

        /// <summary>
        /// The ratio of <see cref="HitObjectInterval"/> between this and the previous <see cref="FlatPattern"/>. In the
        /// case where one or both of the <see cref="HitObjectInterval"/> is undefined, this will have a value of 1.
        /// </summary>
        public double HitObjectIntervalRatio = 1;

        /// <summary>
        /// The interval between the <see cref="StartTime"/> of this and the previous <see cref="FlatPattern"/>. This is
        /// only defined if there is a previous <see cref="FlatPattern"/>.
        /// </summary>
        public double? StartTimeInterval = null;

        /// <summary>
        /// The ratio of <see cref="StartTimeInterval"/> between this and the previous <see cref="FlatPattern"/>. In the
        /// case where one or both of the <see cref="StartTimeInterval"/> is undefined, this will have a value of 1.
        /// </summary>
        public double StartTimeIntervalRatio = 1;

        /// <summary>
        /// The index of this <see cref="FlatPattern"/> in a list of evenly spaced <see cref="FlatPattern"/>s, defined 
        /// as having even <see cref="StartTimeInterval"/>s.
        /// </summary>
        /// TODO: Need to be named better
        public int EvenStartTimeIndex;

        public FlatPattern(List<FlatPattern> allPatterns, int index)
        {
            this.allPatterns = allPatterns;
            Index = index;
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

        public static List<FlatPattern> Encode(List<DifficultyHitObject> data)
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

            flatPatterns.ForEach(pattern => pattern.CalculateIntervals());

            return flatPatterns;
        }


        /// <summary>
        /// Computes <see cref="HitObjectInterval"/>, <see cref="HitObjectIntervalRatio"/>, <see cref="StartTimeInterval"/>,
        /// and <see cref="StartTimeIntervalRatio"/> for this <see cref="FlatPattern"/>.
        /// </summary>
        public void CalculateIntervals()
        {
            HitObjectInterval = HitObjects.Count < 2 ? null : HitObjects[1].StartTime - HitObjects[0].StartTime;
            FlatPattern? previous = Previous(0);

            if (previous?.HitObjectInterval != null && HitObjectInterval != null)
            {
                HitObjectIntervalRatio = HitObjectInterval.Value / previous.HitObjectInterval.Value;
            }

            if (previous == null)
            {
                return;
            }

            StartTimeInterval = StartTime - previous.StartTime;
            StartTimeIntervalRatio = (double)(StartTimeInterval / (previous.StartTimeInterval ?? StartTimeInterval));
        }

        /// <summary>
        /// Two <see cref="FlatPattern"/>s are considered repetitions if they have the same amount of hit objects and
        /// have the same interval between the first two hit objects. Only the first two hit objects are taken into
        /// account due to <see cref="FlatPattern"/>s are defined as having no variation in rhythm.
        ///
        /// If there is only one hit object in the <see cref="FlatPattern"/>s, they are considered repetitions if their
        /// first (and only) hit objects have the same interval.
        /// </summary>
        public bool IsRepetitionOf(FlatPattern? other)
        {
            if (other == null || HitObjects.Count != other.HitObjects.Count)
                return false;

            if (HitObjects.Count <= 1)
                return Math.Abs(HitObjects[0].DeltaTime - other.HitObjects[0].DeltaTime) < 3;

            return Math.Abs(HitObjects[1].DeltaTime - other.HitObjects[1].DeltaTime) < 3;
        }
    }
}
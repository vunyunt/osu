using System;
using System.Linq;
using System.Collections.Generic;

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
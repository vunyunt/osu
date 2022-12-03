// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Rhythm.Data
{
    public class EvenPatterns : EvenRhythm<EvenHitObjects>
    {
        public EvenPatterns? Previous { get; private set; }

        public double ChildrenInterval => Children.Count > 1 ? Children[1].Interval : Children[0].Interval;

        public double IntervalRatio => ChildrenInterval / Previous?.ChildrenInterval ?? double.PositiveInfinity;

        public TaikoDifficultyHitObject FirstHitObject => Children[0].FirstHitObject;

        public IEnumerable<TaikoDifficultyHitObject> AllHitObjects => Children.SelectMany(child => child.Children);

        private EvenPatterns(EvenPatterns? previous, List<EvenHitObjects> data, ref int i)
            : base(data, ref i, 3)
        {
            Previous = previous;

            foreach (TaikoDifficultyHitObject hitObject in AllHitObjects)
            {
                hitObject.Rhythm.EvenPatterns = this;
            }
        }

        public static List<EvenPatterns> GroupPatterns(List<EvenHitObjects> data)
        {
            List<EvenPatterns> evenPatterns = new List<EvenPatterns>();

            // Index does not need to be incremented, as it is handled within EvenRhythm's constructor.
            for (int i = 0; i < data.Count;)
            {
                EvenPatterns? previous = evenPatterns.Count > 0 ? evenPatterns[^1] : null;
                evenPatterns.Add(new EvenPatterns(previous, data, ref i));
            }

            return evenPatterns;
        }
    }
}

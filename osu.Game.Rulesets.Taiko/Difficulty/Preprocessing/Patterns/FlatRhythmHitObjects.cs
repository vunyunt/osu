// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Patterns
{
    /// <summary>
    /// A pattern containing a list of <see cref="TaikoDifficultyHitObject"/> having the same interval.
    /// </summary>
    public class FlatRhythmHitObjects : FlatRhythm<TaikoDifficultyHitObject>
    {
        public FlatRhythmHitObjects() { }

        public override TaikoDifficultyHitObject FirstHitObject => Children[0];
    }
}

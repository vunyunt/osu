// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Patterns
{
    /// <summary>
    /// A group of single-coloured notes.
    /// </summary>
    public class MonoPattern : DifficultyPattern<TaikoDifficultyHitObject>
    {
        public MonoPattern() { }

        public override TaikoDifficultyHitObject FirstHitObject => Children[0];
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Patterns
{
    public class MonoPattern : DifficultyPattern, IRepeatable<MonoPattern>
    {
        public MonoPattern() { }

        bool IRepeatable<MonoPattern>.IsRepetitionOf(MonoPattern other)
        {
            return other.Children.Count == Children.Count;
        }
    }

    /// <summary>
    /// A second order colour pattern where notes are group by repetition of
    /// same-numbered mono notes.
    /// </summary>
    /// Note: even though this and Repetition Aggregator is kept and computed,
    /// it's currently unused in the evaluator.
    public class ColourSequence : DifficultyPattern<MonoPattern>
    {
        public ColourSequence() { }

        public override TaikoDifficultyHitObject FirstHitObject => Children[0].FirstHitObject;
    }
}

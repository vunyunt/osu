// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Patterns
{
    /// <summary>
    /// A pattern that can be repeated
    /// </summary>
    public interface IRepeatable<OtherType>
    {
        public bool IsRepetitionOf(OtherType other);
    }
}

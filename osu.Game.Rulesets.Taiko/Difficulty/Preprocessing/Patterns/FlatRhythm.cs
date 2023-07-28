// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Patterns
{
    /// <summary>
    /// A pattern containing a list of <typeparamref name="ChildrenType"/> where each <typeparamref name="ChildrenType"/> has the same interval.
    /// </summary>
    public abstract class FlatRhythm<ChildrenType> : DifficultyPattern<ChildrenType>
        where ChildrenType : IHasInterval
    {
        /// <summary>
        /// The interval between each of the children in this <see cref="FlatRhythm{ChildrenType}"/>.
        /// </summary>
        public double ChildrenInterval => Children.Count > 1 ? Children[1].Interval : double.NaN;
    }
}

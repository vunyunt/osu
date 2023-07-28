// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Patterns
{
    /// <summary>
    /// A pattern that consists of a list of <typeparamref name="ChildrenType"/>. It should generally only contain
    /// consecutive objects.
    /// </summary>
    public abstract class DifficultyPattern<ChildrenType> : IHasInterval
    where ChildrenType : IHasInterval
    {
        /// <summary>
        /// The previous <see cref="DifficultyPattern{ChildrenType}"/>  />.
        /// </summary>
        public DifficultyPattern<ChildrenType>? Previous;

        public List<ChildrenType> Children { get; protected set; } = new List<ChildrenType>();

        public abstract TaikoDifficultyHitObject FirstHitObject { get; }

        /// <summary>
        /// The start time of the first <see cref="DifficultyPattern{ChildrenType}.Children"/> in this <see cref="DifficultyPattern{ChildrenType}"/>.
        /// </summary>
        public double StartTime => Children[0].StartTime;

        /// <summary>
        /// The interval between <see cref="StartTime" /> of this and the previous <see cref="DifficultyPattern{ChildrenType}"/>.
        /// </summary>
        public double Interval => Previous != null ? StartTime - Previous.StartTime : double.NaN;

        /// <summary>
        /// The duration of this <see cref="DifficultyPattern{ChildrenType}"/>.
        /// </summary>
        public double Duration => Children[^1].StartTime - StartTime;
    }
}

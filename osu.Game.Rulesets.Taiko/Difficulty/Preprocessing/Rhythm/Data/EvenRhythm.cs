// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Rhythm.Data
{
    /// <summary>
    /// A base class for grouping <see cref="IHasInterval"/>s by their interval. In edges where an interval change
    /// occurs, the <see cref="IHasInterval"/> is added to the group with the smaller interval.
    /// </summary>
    public abstract class EvenRhythm<ChildType>
        where ChildType : class, IHasInterval
    {
        public IReadOnlyList<ChildType> Children { get; private set; }

        private double totalInterval;

        public double AverageInterval => totalInterval / (Children.Count - 1);

        private bool isFlat(ChildType current, ChildType next, double marginOfError)
        {
            return Math.Abs(current.Interval - next.Interval) <= marginOfError;
        }

        private void add(List<ChildType> children, ChildType child)
        {
            children.Add(child);

            if (children.Count > 1)
            {
                totalInterval += child.Interval;
            }
        }

        /// <summary>
        /// Process a child when it's at the "edge" of two groups. The child is added to the group with the smaller interval.
        /// </summary>
        private void processEdge(List<ChildType> children, List<ChildType> data, ref int i)
        {
            if (data[i + 1].Interval > data[i].Interval)
            {
                add(children, data[i]);
                i++;
            }
        }

        /// <summary>
        /// Create a new <see cref="EvenRhythm{ChildType}"/> from a list of <see cref="IHasInterval"/>s, and add
        /// them to the <see cref="Children"/> list until the end of the group.
        /// </summary>
        ///
        /// <param name="data">
        /// The list of <see cref="IHasInterval"/>s.
        /// </param>
        ///
        /// <param name="i">
        /// Index in <paramref name="data"/> to start adding children. This will be modified and should be passed into
        /// the next <see cref="EvenRhythm{ChildType}"/>'s constructor.
        /// </param>
        ///
        /// <param name="marginOfError">
        /// The margin of error for the interval, within of which no interval change is considered to have occured.
        /// </param>
        ///
        /// <param name="hitWindow">
        /// The hit window for the <see cref="IHasInterval"/>s. This is used to determine if a given pattern, while
        /// having interval changes, can be hit by playing evenly. This is determined by checking all future notes that
        /// fall within marginOfError of each other whenever a rhythm change occurs. If they can be all hit within
        /// hitWindow while playing with the current interval, then these notes are added to the current group.
        /// </param>
        protected EvenRhythm(List<ChildType> data, ref int i, double marginOfError, double hitWindow)
        {
            List<ChildType> children = new List<ChildType>();
            Children = children;
            add(children, data[i++]);

            if (i < data.Count - 1)
            {
                if (!isFlat(data[i], data[i + 1], hitWindow))
                {
                    processEdge(children, data, ref i);
                    return;
                }

                add(children, data[i++]);
            }

            while (i < data.Count - 1)
            {
                double expectedInterval = 0;
                double actualInterval = 0;
                int j = i;

                do
                {
                    expectedInterval += AverageInterval;
                    actualInterval += data[j].Interval;

                    if (Math.Abs(expectedInterval - actualInterval) > hitWindow)
                    {
                        processEdge(children, data, ref i);
                        return;
                    }

                    j++;
                } while (j < data.Count - 1 && isFlat(data[j], data[j + 1], marginOfError));

                for (; i < j; i++)
                {
                    add(children, data[i]);
                }
            }

            // Handle final data
            if (data.Count > 2 && i < data.Count && isFlat(data[^2], data[^1], marginOfError))
            {
                add(children, data[i]);
                i++;
            }
        }
    }
}

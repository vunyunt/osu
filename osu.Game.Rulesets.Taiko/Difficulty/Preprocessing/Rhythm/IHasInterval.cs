// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Rhythm
{
    // This was originally meant to be a generic, so that we could have interval based on different concepts (for
    // example, time-based vs note-based intervals). However it seems like INumber is only supported in C# 7, and
    // the current rhythm system only requires double intervals.
    public interface IHasInterval
    {
        double Interval { get; }
    }
}

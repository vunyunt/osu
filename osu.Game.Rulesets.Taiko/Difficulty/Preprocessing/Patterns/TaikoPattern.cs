// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Patterns
{
    public class TaikoPattern
    {
        public FlatRhythmHitObjects? FlatRhythmPattern;

        public SecondPassRhythmPattern? SecondPassRhythmPattern;

        public ThirdPassRhythmPattern? ThirdPassRhythmPattern;

        public MonoPattern? MonoPattern;

        public ColourRhythm? FirstPassColourPattern;

        public SecondPassColourRhythm? SecondPassColourPattern;

        public ColourSequence? ColourSequence;
    }
}

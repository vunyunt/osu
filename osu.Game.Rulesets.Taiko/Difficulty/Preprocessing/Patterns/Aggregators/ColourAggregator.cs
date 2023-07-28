// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Patterns.Aggregators
{
    /// <summary>
    /// Aggregates notes into groups where elements are of the same colour (hit type).
    /// </summary>
    public class ColourAggregator
    {
        public List<MonoPattern> Group(List<TaikoDifficultyHitObject> input)
        {
            List<MonoPattern> result = new List<MonoPattern>();
            MonoPattern? currentPattern = null;

            for (int i = 0; i < input.Count; i++)
            {
                TaikoDifficultyHitObject currentObject = input[i];

                // Get the previous note, ignoring all non-note objects
                TaikoDifficultyHitObject? previousObject = currentObject.PreviousNote(0);

                // If this is the first object in the list or the colour changed, create a new mono pattern
                if (
                    currentPattern == null ||
                    previousObject == null ||
                    (currentObject.BaseObject as Hit)?.Type != (previousObject.BaseObject as Hit)?.Type)
                {
                    currentPattern = new MonoPattern()
                    {
                        Previous = currentPattern
                    };
                    result.Add(currentPattern);
                }

                // Add the current object to the result.
                currentPattern.Children.Add(currentObject);
            }

            return result;
        }
    }
}

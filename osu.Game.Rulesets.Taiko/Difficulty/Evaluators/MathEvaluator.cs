// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Rulesets.Taiko.Difficulty.Evaluators
{
    /// <summary>
    /// Utility math functinos used in taiko difficulty calculation.
    /// </summary>
    public class MathEvaluator
    {

        /// <summary>
        /// An interted sigmoid function. It gives a value between (middle - height/2) and (middle + height/2). Output
        /// will have a negative correlaation with the input value.
        /// </summary>
        /// <param name="val">The input value.</param>
        /// <param name="center">The center of the sigmoid, where the largest gradient occurs and output is equal to middle.</param>
        /// <param name="width">The radius of the sigmoid, outside of which output are near the minimum/maximum.</param>
        /// <param name="middle">The middle of the sigmoid output.</param>
        /// <param name="height">The height of the sigmoid output. This will be equal to max output - min output.</param>
        public static double InvertedSigmoid(double val, double center, double width, double middle, double height)
        {
            double inverted = Math.Tanh(Math.E * -(val - center) / width);
            return inverted * (height / 2) + middle;
        }

        /// <summary>
        /// A sigmoid function. It gives a value between (middle - height/2) and (middle + height/2). Output will have a
        /// positive correlaation with the input value.
        /// </summary>
        /// <param name="val">The input value.</param>
        /// <param name="center">The center of the sigmoid, where the largest gradient occurs and output is equal to middle.</param>
        /// <param name="width">The radius of the sigmoid, outside of which output are near the minimum/maximum.</param>
        /// <param name="middle">The middle of the sigmoid output.</param>
        /// <param name="height">The height of the sigmoid output. This will be equal to max output - min output.</param>
        public static double Sigmoid(double val, double center, double width, double middle, double height)
        {
            double inverted = InvertedSigmoid(val, center, width, middle, height);
            return 2 * middle - inverted;
        }
    }
}

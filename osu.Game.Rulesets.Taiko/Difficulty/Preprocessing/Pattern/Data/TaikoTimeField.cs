// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Stores amplitude points data by time. Getting amplitude at a specific time is defined as the sum of amplitudes
/// within a range scaled by a normal distribution.
/// </summary>
public class TaikoTimeField
{
    private readonly SortedList<double, double> amplitudesByTime = new SortedList<double, double>();

    public void AddNode(double time, double amplitude)
    {
        double resultAmplitude = amplitudesByTime.GetValueOrDefault(time, 0) + amplitude;
        amplitudesByTime[time] = resultAmplitude;
    }

    private double bellCurve(double x, double standardDeviation)
    {
        double scaledX = x / standardDeviation;
        double numerator = Math.Pow(Math.E, (-(scaledX * scaledX)) / 2);
        return numerator;
    }

    private int binarySearchGte(double startTime)
    {
        int left = 0;
        int right = amplitudesByTime.Count - 1;
        int startIndex = -1;

        while (left <= right)
        {
            int mid = left + (right - left) / 2;
            if (amplitudesByTime.Keys[mid] == startTime)
            {
                startIndex = mid;
                break;
            }
            else if (amplitudesByTime.Keys[mid] < startTime)
            {
                left = mid + 1;
            }
            else
            {
                startIndex = mid;
                right = mid - 1;
            }
        }

        return startIndex;
    }

    private List<(double time, double amplitude)> getNodesIn(double startTime, double endTime)
    {
        List<(double, double)> withinRangeAmplitudes = new List<(double, double)>();
        int startIndex = binarySearchGte(startTime);

        if (startIndex != -1)
        {
            for (int i = startIndex; i < amplitudesByTime.Count && amplitudesByTime.Keys[i] <= endTime; i++)
            {
                withinRangeAmplitudes.Add((amplitudesByTime.Keys[i], amplitudesByTime.Values[i]));
            }
        }

        return withinRangeAmplitudes;
    }

    /// <summary>
    /// Returns the sum of amplitudes within a range scaled by a normal distribution.
    /// </summary>
    ///
    /// <param name="time">The center time point</param>
    /// <param name="standardDeviation">
    /// Range of the normal distribution. Note that only amplitudes within 3 standard
    /// deviations are considered
    /// </param>
    /// <returns></returns>
    public double GetAmplitude(double time, double standardDeviation)
    {
        List<(double time, double amplitude)> nodes = getNodesIn(
            time - standardDeviation * 3, time + standardDeviation * 3);
        return nodes
            .Sum(x =>
            {
                return bellCurve(time - x.time, standardDeviation) * x.amplitude;
            });
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;

public static class CollectionUtils
{
    public static IEnumerable<TargetType> SelectPair<SourceType, TargetType>(
        this IEnumerable<SourceType> source, Func<SourceType, SourceType, TargetType> operation)
    {
        return source
            .Zip(source.Skip(1), source.SkipLast(1))
            .Select(x => operation(x.First, x.Second));
    }

    public static void ForEachPair<SourceType>(
        this IEnumerable<SourceType> source, Action<SourceType, SourceType> action)
    {
        source.Zip(source.Skip(1), source.SkipLast(1))
            .ForEach(x => action(x.First, x.Second));
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;

public static class CollectionUtils
{
    public static IEnumerable<TargetType> SelectPair<SourceType, TargetType>(
        this IEnumerable<SourceType> source, Func<SourceType, SourceType, TargetType> operation)
    {
        return source
            .Zip(source.Skip(1), source.SkipLast(1))
            .Select(x => operation(x.First, x.Second));
    }
}

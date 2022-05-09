namespace ValidCode.DependencyProperties;

using System;

public readonly struct Date
{
    public static DateTimeOffset Min(DateTimeOffset x, DateTimeOffset y) => x < y ? x : y;
}

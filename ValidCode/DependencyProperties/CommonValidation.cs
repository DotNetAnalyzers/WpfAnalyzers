namespace ValidCode.DependencyProperties;

internal static class CommonValidation
{
    public static bool ValidateDoubleIsGreaterThanZero(object? value)
    {
        if (value is double d)
        {
            return d > 0;
        }

        return false;
    }
}

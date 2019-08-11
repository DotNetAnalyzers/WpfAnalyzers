namespace ValidCode.DependencyProperties
{
    internal static class BooleanBoxes
    {
        internal static readonly object True = true;
        internal static readonly object False = false;

        internal static object Box(bool value)
        {
            return value
                ? True
                : False;
        }
    }
}

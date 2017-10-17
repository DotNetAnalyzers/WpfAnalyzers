namespace WpfAnalyzers
{
    internal enum AnalysisResult
    {
        /// <summary>
        /// Analysis determined that PropertyChanged was not invoked in this path.
        /// </summary>
        No,

        /// <summary>
        /// Analysis determined that PropertyChanged was invoked in this path.
        /// </summary>
        Yes,

        /// <summary>
        /// Analysis determined that PropertyChanged is potentially invoked in this path.
        /// </summary>
        Maybe
    }
}
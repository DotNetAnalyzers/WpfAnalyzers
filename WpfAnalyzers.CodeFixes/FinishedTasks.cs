namespace WpfAnalyzers
{
    using System.Threading.Tasks;

    internal static class FinishedTasks
    {
        internal static Task CompletedTask { get; } = Task.FromResult(default(VoidResult));

        private struct VoidResult
        {
        }
    }
}
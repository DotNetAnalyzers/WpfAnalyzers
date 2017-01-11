namespace WpfAnalyzers.Test
{
    using System.Threading.Tasks;

    public interface IHappyPathVerifier
    {
        Task VerifyHappyPathAsync(params string[] testCode);
    }
}
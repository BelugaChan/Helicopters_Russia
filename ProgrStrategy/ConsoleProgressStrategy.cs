using AbstractionsAndModels.Abstract;

namespace Algo.ProgrStrategy
{
    public class ConsoleProgressStrategy : ProgressStrategy
    {
        public override void LogProgress()
        {
            var progress = GetCurrentProgress();
            Console.WriteLine($"Step: {progress.Step}, Progress: {progress.CurrentProgress}%");
        }
    }
}

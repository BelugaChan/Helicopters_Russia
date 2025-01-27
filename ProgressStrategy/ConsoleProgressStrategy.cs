using AbstractionsAndModels.Interfaces.ProgressStrategy;
using AbstractionsAndModels.Models;

namespace Algo.ProgressStrategy
{
    public class ConsoleProgressStrategy : IProgressStrategy
    {
        private Progress _progress = new();
        private readonly object _lock = new(); 
        public Progress GetCurrentProgress()
        {
            lock (_lock)
            {
                return new Progress
                {
                    Step = _progress.Step,
                    CurrentProgress = _progress.CurrentProgress
                };
            }
        }

        public void UpdateProgress(Progress progress)
        {
            lock (_lock)
            {
                _progress = new Progress
                {
                    Step = progress.Step,
                    CurrentProgress = progress.CurrentProgress
                };
            }          
        }

        public void LogProgress()
        {
            Console.WriteLine($"Step: {_progress.Step}, Progress: {_progress.CurrentProgress}%");
        }
    }
}

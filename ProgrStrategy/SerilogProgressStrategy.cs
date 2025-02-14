using AbstractionsAndModels.Abstract;
using Serilog;

namespace Algo.ProgrStrategy
{
    public class SerilogProgressStrategy : ProgressStrategy
    {
        public override void LogProgress()
        {
            var progress = GetCurrentProgress();
            Log.Information($"Step: {progress.Step}, Progress: {progress.CurrentProgress}%");
        }
    }
}

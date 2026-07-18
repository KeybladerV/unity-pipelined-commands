using System;
using Cysharp.Threading.Tasks;

namespace PipelinedCommands.Utility
{
    /// <summary>
    /// Retains for N seconds (unscaled), then Releases.
    /// Only compiled when UniTask is present in the host project.
    /// Use <c>To1&lt;DelaySecondsCommand, float&gt;(seconds)</c>.
    /// </summary>
    [Poolable]
    public sealed class DelaySecondsCommand : Command<float>
    {
        public override void Execute(float seconds)
        {
            if (seconds <= 0f)
                return;

            Retain();
            DelayAsync(seconds).Forget();
        }

        private async UniTaskVoid DelayAsync(float seconds)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(seconds), DelayType.UnscaledDeltaTime);
                if (!IsResolved)
                    Release();
            }
            catch (Exception ex)
            {
                if (!IsResolved)
                    Fail(ex);
            }
        }
    }
}

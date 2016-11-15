using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ModMyFactory.Helpers
{
    static class ProcessExtensions
    {
        public static Task WaitForExitAsync(this Process process)
        {
            process.EnableRaisingEvents = true;

            var completionSource = new TaskCompletionSource<object>();

            EventHandler handler = null;
            handler = (sender, e) =>
            {
                process.Exited -= handler;
                completionSource.TrySetResult(null);
            };
            process.Exited += handler;

            return completionSource.Task;
        }
    }
}

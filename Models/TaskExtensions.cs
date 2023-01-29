using System;
using System.Threading.Tasks;

namespace Marmi.Models
{
    public static class TaskExtensions
    {
        public static void FireAndForget(this Task task, Action action = null)
        {
            task.ContinueWith(_ => action?.Invoke(), TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
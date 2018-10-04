using System.Threading.Tasks;

namespace ModMyFactory.Helpers
{
    static class TaskHelper
    {
        private static readonly Task[] emptyTaskArray = new Task[0];

        /// <summary>
        /// Returns a task object that is immediately completed.
        /// </summary>
        // Task.CompletedTask exists in .Net 4.6 but in 4.5.2 it is an internal member. Calling WhenAll with an empty array will return the internal CompletedTask object however.
        public static Task CompletedTask => Task.WhenAll(emptyTaskArray); 
    }
}

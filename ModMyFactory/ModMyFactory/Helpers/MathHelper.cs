using System.Linq;

namespace ModMyFactory.Helpers
{
    static class MathHelper
    {
        /// <summary>
        /// Finds the minimum of the given values.
        /// </summary>
        public static int Min(params int[] values)
        {
            return values.Min();
        }
    }
}

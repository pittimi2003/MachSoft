namespace Mss.WorkForce.Code.WMSSimulator.Helper
{
    public static class RandomSelector
    {
        #region Methods

        /// <summary>
        /// Selects an integer number between two options, both included
        /// </summary>
        /// <param name="min">Minimum number to pick</param>
        /// <param name="max">Maximum number to pick</param>
        /// <returns>Chosen number</returns>
        public static int Select(int min, int max)
        {
            Random rand = new Random();
            return rand.Next(min, max + 1);
        }

        /// <summary>
        /// Selects a random time span between to given limits
        /// </summary>
        /// <param name="start">Init time span</param>
        /// <param name="end">End time span</param>
        /// <returns>Chosen time span</returns>
        /// <exception cref="ArgumentException">Throws an exception if the start time is greater than the end time</exception>
        public static TimeSpan SelectRandomTime(TimeSpan start, TimeSpan end)
        {
            if (start > end)
            {
                throw new ArgumentException("Start time cannot be greater than the end time.");
            }

            Random rand = new Random();
            long range = (end - start).Ticks;
            long randomTicks = (long)(rand.NextDouble() * range);

            return start.Add(new TimeSpan(randomTicks));
        }

        #endregion
    }
}

namespace Mss.WorkForce.Code.Simulator.Helper
{
    public static class UniformDistribution
    {
        /// <summary>
        /// Creates an uniform distribution within a given hour limits.
        /// </summary>
        /// <param name="init">Init hour</param>
        /// <param name="end">End hour</param>
        /// <param name="count">Number of processes to distribute</param>
        /// <returns>List<double> giving the points of the distribution for each of the processes</returns>
        public static List<double> Distribute(double init, double end, int count)
        {
            if (count != 0)
            {
                if (count < 2)
                {
                    var meanPoint = Math.Round((init + end) / 2, 1);
                    return [meanPoint];
                }
                else
                {
                    List<double> distribution = Enumerable.Range(0, count).Select(i => init + i * (end - init) / (count - 1)).ToList();
                    return distribution;
                }
            }
            else
            {
                Console.WriteLine("No processes to distribute. Count to make an uniforme distribution cannot be zero.");
                throw new Exception("No processes to distribute. Count to make an uniforme distribution cannot be zero.");
            }
        }
    }
}

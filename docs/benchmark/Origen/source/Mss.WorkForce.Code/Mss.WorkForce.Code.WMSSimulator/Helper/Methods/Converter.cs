namespace Mss.WorkForce.Code.WMSSimulator.Helper
{
    public static class Converter
    {
        #region Methods

        /// <summary>
        /// Converts a double representing minutes to an int representing miliseconds.
        /// </summary>
        /// <param name="windowTime">Double representing minutes</param>
        /// <returns>Int representing miliseconds</returns>
        public static int ConvertMinutesToMiliseconds(double windowTime)
        {
            return Convert.ToInt32(windowTime * 60000);
        }

        /// <summary>
        /// Converts a double representing an hour to the actual hour and its minutes
        /// </summary>
        /// <param name="value">Double representing the hour in decimal format</param>
        /// <returns>Tuple with the hoour and the minutes</returns>
        public static (int Hours, int Minutes) ConvertFromDoubleToTime(double value)
        {
            int hours = (int)value;
            int minutes = (int)((value - hours) * 60);

            return (hours, minutes);
        }

        #endregion
    }
}

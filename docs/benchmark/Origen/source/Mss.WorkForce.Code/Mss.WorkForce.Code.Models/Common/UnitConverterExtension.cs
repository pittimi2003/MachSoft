namespace Mss.WorkForce.Code.Models.Common
{
    public static class UnitConverter
    {
        private const double MetersToFeet = 3.28084;

        /// <summary>
        /// Convierte metros a pies (usa el sistema métrico o imperial).
        /// </summary>
        public static double ConvertMetersToFeet(double valueInMeters, string unitSystem)
        {
            double result = unitSystem switch
            {
                "BritishImperial system" => valueInMeters * MetersToFeet,
                "International units" => valueInMeters,
                _ => valueInMeters
            };

            return Math.Round(result, 2);
        }

        /// <summary>
        /// Convierte pies a metros (usa el sistema métrico o imperial).
        /// </summary>
        public static double ConvertFeetToMeters(double valueInFeet, string unitSystem)
        {
            double result = unitSystem switch
            {
                "BritishImperial system" => valueInFeet / MetersToFeet,
                "International units" => valueInFeet,
                _ => valueInFeet
            };

            return Math.Round(result, 2);
        }
    }
}
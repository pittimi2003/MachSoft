using System.Text;

namespace Mss.WorkForce.Code.WMSSimulator.Generator
{
    public static class AppointmentsGenerator
    {
        /// <summary>
        /// Generates the Appointments for the YardAppointmentsNotifications table
        /// </summary>
        /// <param name="WFMOrders">Orders generated with Appointments</param>
        /// <param name="wfmData">Specific data from WFM context</param>
        /// <param name="isOut">If the orders are outbound or inbound</param>
        /// <param name="random">Object of the class Random</param>
        /// <param name="iteration">Number of the WorkForceTask being generated</param>
        /// <returns>List of Appointments</returns>
        public static List<Models.Models.YardAppointmentsNotifications> Generate(List<Models.Models.InputOrder> WFMOrders, DataBaseResponse wfmData, bool isOut,
            Random random, int iteration)
        {
            List<Models.Models.YardAppointmentsNotifications> yardAppointments = new List<Models.Models.YardAppointmentsNotifications>();

            var appointments = WFMOrders.GroupBy(x => (x.AppointmentDate, x.VehicleCode, x.IsOutbound, x.WarehouseId))
            .Select(x => new
            {
                AppointmentDate = x.Key.AppointmentDate,
                VehicleCode = x.Key.VehicleCode,
                IsOutbound = x.Key.IsOutbound,
                WarehouseId = x.Key.WarehouseId
            });

            var intro = isOut ? "APO" : "API";

            int i = 0;
            foreach (var a in appointments)
            {
                yardAppointments.Add(new Models.Models.YardAppointmentsNotifications()
                {
                    Id = Guid.NewGuid(),
                    NotificationId = Guid.NewGuid(),
                    AppointmentCode = $"{intro}_{(iteration + i).ToString("D5")}",
                    YardCode = "Back yard",
                    VehicleCode = a.VehicleCode,
                    VehicleType = "Truck",
                    AppointmentDate = a.AppointmentDate,
                    Customer = "Mecalux",
                    License = LicenseGenerator(random)
                });
                i++;
            }

            return yardAppointments;
        }

        /// <summary>
        /// Generates a random Spanish license plate
        /// </summary>
        /// <param name="random">Object of the random class</param>
        /// <returns>String of a license plate</returns>
        private static string LicenseGenerator(Random random)
        {
            char[] letrasPermitidas = "BCDFGHJKLMNPRSTVWXYZ".ToCharArray();

            // Generar 4 dígitos con ceros a la izquierda si es necesario
            int numeros = random.Next(0, 10000); // 0000 - 9999
            string parteNumerica = numeros.ToString("D4");

            // Generar 3 letras válidas
            StringBuilder parteLetras = new StringBuilder();
            for (int i = 0; i < 3; i++)
            {
                char letra = letrasPermitidas[random.Next(letrasPermitidas.Length)];
                parteLetras.Append(letra);
            }

            return $"{parteNumerica}{parteLetras}";
        }
    }
}

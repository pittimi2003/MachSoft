using Mss.WorkForce.Code.Web.Model;

namespace Mss.WorkForce.Code.Web.Services
{
    public class EnvironmentService
    {
        public EnvironmentEnum Environment { get; private set; } = EnvironmentEnum.DEVEL;

        public EnvironmentService(IConfiguration configuration)
        {
            var envString = configuration["DEPLOYENVIRONMENT"];

            if (!string.IsNullOrWhiteSpace(envString) &&
                Enum.TryParse<EnvironmentEnum>(envString, true, out var parsedEnvironment))
            {
                Environment = parsedEnvironment;
            }
            else
            {
                Environment = EnvironmentEnum.DEVEL; 
            }
        }

        public EnvironmentEnum GetEnvironment() => Environment;
    }
}

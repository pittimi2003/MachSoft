using Mss.WorkForce.Code.Models.DTO.Preview;

namespace Mss.WorkForce.Code.Models.DTO
{
    public class SimulationParametrizedDto
    {
        public List<ResourceDto> dataOperator {  get; set; }
        public List<LoadDto> inputList { get; set; }
        public List<LoadDto> outputList { get; set; }

        public List<ShiftDto> shiftDtos { get; set; }

        public List<StopDto> stops { get; set; }

    }
}

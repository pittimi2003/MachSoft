using Mss.WorkForce.Code.Models.DTO.Preview;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Models.ModelUpdate;

namespace Mss.WorkForce.Code.Models
{
    public class PreviewDto
    {
        public TemporalidadModel Temporalidad { get; set; }
        public List<LoadDto> Loadinput { get; set; }
        public List<LoadDto> LoadOutput { get; set; }
        public List<ShiftRolDto> ShiftRolDto { get; set; }
    }

    public class PreviewDtoData
    {
        public List<LoadDto> Loadinput { get; set; }
        public List<LoadDto> LoadOutput { get; set; }
        public List<ShiftRolDto> ShiftRolDto { get; set; }
    }
}

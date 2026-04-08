
namespace Mss.WorkForce.Code.Models.DTO
{
    public class PlanningSettingDto
    {
        public List<EquipmentDto> Equipments { get; set; } = [];
        public List<OrderScheduleDto> InputProfiles { get; set; } = [];
        public List<OrderScheduleDto> OutputProfiles { get; set; } = [];
        public List<WorkerScheduleDto> WorkerSchedules { get; set; } = [];
    }
}

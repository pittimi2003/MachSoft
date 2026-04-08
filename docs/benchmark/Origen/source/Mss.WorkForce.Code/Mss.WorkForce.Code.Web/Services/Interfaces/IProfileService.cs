using Mss.WorkForce.Code.Models;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Web.Model;

namespace Mss.WorkForce.Code.Web.Services
{
    public interface IProfileService
    {
        IEnumerable<LoadProfileDto> LoadProfiles(Guid warehouseId);

        IEnumerable<VehicleProfileDto> VehicleProfiles(Guid warehouseId);

        IEnumerable<PutawayProfileDto> PutawayProfiles(Guid warehouseId);

        IEnumerable<PostprocessProfileDto> PostprocessProfiles(Guid warehouseId);

        IEnumerable<PreprocessProfileDto> PreprocessProfiles(Guid warehouseId);
        IEnumerable<OrderProfilesDto> OrderSchedules(Guid warehouseId);
        IEnumerable<OrderLoadPropertiesDto> OrderLoadPropertiesProfile(Guid warehouseId);

        Task LoadProfileSaveChanges(IEnumerable<LoadProfileDto> loadProfileDtos, Guid warehouseId);
        Task VehicleProfileSaveChanges(IEnumerable<VehicleProfileDto> loadProfileDtos, Guid warehouseId);
        Task PutawayProfileSaveChanges(IEnumerable<PutawayProfileDto> loadProfileDtos, Guid warehouseId);
        Task PostprocessProfileSaveChanges(IEnumerable<PostprocessProfileDto> loadProfileDtos, Guid warehouseId);
        Task PreprocessProfileSaveChanges(IEnumerable<OrderProfilesDto> loadProfileDtos, Guid warehouseId);
        Task ProprocessProfileSaveChanges(IEnumerable<OrderLoadPropertiesDto> preprocessProfileDtos, Guid whId);
    }
}

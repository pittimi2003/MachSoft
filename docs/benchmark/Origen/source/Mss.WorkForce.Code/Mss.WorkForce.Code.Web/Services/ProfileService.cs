using Microsoft.AspNetCore.Components;
using Mss.WorkForce.Code.Models.ConvertedModel;
using Mss.WorkForce.Code.Models;
using Mss.WorkForce.Code.Models.DataAccess;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Models.DTO.ExtraConfiguration;

namespace Mss.WorkForce.Code.Web.Services
{
    public class ProfileService : IProfileService
    {
        #region Fields
        private readonly DataAccess _dataAccess;

        private readonly ISimulateService _simulateService;

        #endregion

        #region
        public ProfileService(DataAccess dataAccess, ISimulateService simulateService)
        {
            _dataAccess = dataAccess;
            _simulateService = simulateService;
        }

        #endregion

        #region Properties

        [Inject]
        private ILogger<SettingService> _logger { get; set; }

        #endregion

        #region GetDataBD

        public IEnumerable<LoadProfileDto> LoadProfiles(Guid warehouseId)
        {
            return MapperLoadProfile(_dataAccess.GetLoadProfiles(warehouseId));
        }

        public IEnumerable<VehicleProfileDto> VehicleProfiles(Guid warehouseId)
        {
            return MapperVehicleProfile(_dataAccess.GetVehicleProfiles(warehouseId));
        }

        public IEnumerable<PutawayProfileDto> PutawayProfiles(Guid warehouseId)
        {
            return MapperPutawayProfile(_dataAccess.GetPutawayProfiles(warehouseId));
        }

        public IEnumerable<PostprocessProfileDto> PostprocessProfiles(Guid warehouseId)
        {
            return MapperPostprocessProfile(_dataAccess.GetPostprocessProfiles(warehouseId));
        }

        public IEnumerable<PreprocessProfileDto> PreprocessProfiles(Guid warehouseId)
        {
            return MapperPreprocessProfile(_dataAccess.GetPreprocessProfiles(warehouseId));
        }

        public IEnumerable<OrderProfilesDto> OrderSchedules(Guid warehouseId)
        {
            return MapperOrderProfiles(_dataAccess.GetOrderSchedules(warehouseId));
        }

        public IEnumerable<OrderLoadPropertiesDto> OrderLoadPropertiesProfile(Guid warehouseId)
        {
            return MapperOrderLoadProfile(_dataAccess.GetOrderLoadRatios(warehouseId));
        }

        #endregion

        #region MapperEntityToDto

        private IEnumerable<LoadProfileDto> MapperLoadProfile(IEnumerable<LoadProfile> loadProfileList)
        {
            List<LoadProfileDto> DataProfile = new List<LoadProfileDto>();
            foreach (var loadProfile in loadProfileList)
            {
                DataProfile.Add(new LoadProfileDto
                {
                    Id = loadProfile.Id,
                    Name = loadProfile.Name,
                    Type = loadProfile.Type,
                    WarehouseId = loadProfile.WarehouseId,
                });
            }

            return DataProfile;

        }

        private IEnumerable<VehicleProfileDto> MapperVehicleProfile(IEnumerable<VehicleProfile> vehicleProfileList)
        {
            List<VehicleProfileDto> DataProfile = new List<VehicleProfileDto>();

            foreach (var vProfile in vehicleProfileList)
            {
                DataProfile.Add(new VehicleProfileDto
                {
                    Id = vProfile.Id,
                    Name = vProfile.Name,
                    Type = vProfile.Type,
                    WarehouseId = vProfile.WarehouseId,
                });
            }

            return DataProfile;
        }

        private IEnumerable<PutawayProfileDto> MapperPutawayProfile(IEnumerable<PutawayProfile> putawayProfileList)
        {
            List<PutawayProfileDto> DataProfile = new List<PutawayProfileDto>();

            foreach (var putawayProfile in putawayProfileList)
            {
                DataProfile.Add(new PutawayProfileDto
                {
                    Id = putawayProfile.Id,
                    Name = putawayProfile.Name,
                    Type = putawayProfile.Type,
                    WarehouseId = putawayProfile.WarehouseId,
                });
            }
            return DataProfile;
        }

        private IEnumerable<PostprocessProfileDto> MapperPostprocessProfile(IEnumerable<PostprocessProfile> postprocessProfileList)
        {
            List<PostprocessProfileDto> DataProfile = new List<PostprocessProfileDto>();

            foreach (var putawayProfile in postprocessProfileList)
            {
                DataProfile.Add(new PostprocessProfileDto
                {
                    Id = putawayProfile.Id,
                    Name = putawayProfile.Name,
                    Type = putawayProfile.Type,
                    WarehouseId = putawayProfile.WarehouseId
                });
            }
            return DataProfile;
        }

        private IEnumerable<PreprocessProfileDto> MapperPreprocessProfile(IEnumerable<PreprocessProfile> preprocessProfileList)
        {
            List<PreprocessProfileDto> DataProfile = new List<PreprocessProfileDto>();

            foreach (var putawayProfile in preprocessProfileList)
            {
                DataProfile.Add(new PreprocessProfileDto
                {
                    Id = putawayProfile.Id,
                    Name = putawayProfile.Name,
                    Type = putawayProfile.Type,
                    WarehouseId = putawayProfile.WarehouseId
                });
            }

            return DataProfile;
        }

        private IEnumerable<OrderProfilesDto> MapperOrderProfiles(IEnumerable<OrderSchedule> orderSchedules)
        {
            List<OrderProfilesDto> DataProfile = new List<OrderProfilesDto>();

            foreach (var item in orderSchedules)
            {
                OrderProfilesDto entity = new() 
                {
                    Id = item.Id,
                    NumberVehicles = item.NumberVehicles,
                    EndHour = item.EndHour,
                    InitHour = item.InitHour,
                    IsOut = item.IsOut,
                    WarehouseId =item.WarehouseId
                };
                if (item.Vehicle != null)
                {
                    entity.Vehicle.Id = item.Vehicle.Id;
                    entity.Vehicle.Name = item.Vehicle.Name;
                }
                if (item.Load != null)
                {
                    entity.Load.Id = item.Load.Id;
                    entity.Load.Name = item.Load.Name;
                }

                DataProfile.Add(entity);
            }

            return DataProfile.OrderBy(x => x.IsOut).OrderBy(x => x.InitHour);
        }

        private IEnumerable<OrderLoadPropertiesDto> MapperOrderLoadProfile(IEnumerable<OrderLoadRatio> orderLoadProfileList)
        {
            List<OrderLoadPropertiesDto> DataProfile = new List<OrderLoadPropertiesDto>();

            foreach (var orderProfile in orderLoadProfileList)
            {
                var profileCatalogue = new CatalogEntity(EntityNamesConst.VehicleProfile) { Id = orderProfile.Vehicle.Id, Name = orderProfile.Vehicle.Name };
                var loadCatalogue = new CatalogEntity(EntityNamesConst.LoadProfile) { Id = orderProfile.Load.Id, Name = orderProfile.Load.Name };
                DataProfile.Add(new OrderLoadPropertiesDto
                {
                    Id = orderProfile.Id,
                    Vehicle = profileCatalogue,
                    Load = loadCatalogue,
                    LoadInVehicle = orderProfile.LoadInVehicle,
                    OrderInLoad = orderProfile.OrderInLoad !=0? 1/orderProfile.OrderInLoad :0,
                    WarehouseId = orderProfile.Vehicle.WarehouseId,
                });
            }
            return DataProfile;
        }
        #endregion

        #region MapperDtoToEntity

        private LoadProfile MapperEntityLoadProfile(LoadProfileDto loadProfileDto)
        {
            LoadProfile entity = new()
            {
                Id = loadProfileDto.Id,
                Name = loadProfileDto.Name,
                Type = loadProfileDto.Type,
                WarehouseId = loadProfileDto.WarehouseId,
                Warehouse = null
            };

            return entity;
        }

        private VehicleProfile MapperEntityVehicleProfile(VehicleProfileDto vehicleProfileDto)
        {
            VehicleProfile entity = new()
            {
                Id = vehicleProfileDto.Id,
                Name = vehicleProfileDto.Name,
                Type = vehicleProfileDto.Type,
                WarehouseId = vehicleProfileDto.WarehouseId,
                Warehouse = null
            };

            return entity;
        }

        private PutawayProfile MapperEntityPutawayProfile(PutawayProfileDto putawayProfileDto)
        {
            PutawayProfile entity = new()
            {
                Id = putawayProfileDto.Id,
                Name = putawayProfileDto.Name,
                Type = putawayProfileDto.Type,
                WarehouseId = putawayProfileDto.WarehouseId,
                Warehouse = null
            };

            return entity;
        }

        private PostprocessProfile MapperEntityPostprocessProfile(PostprocessProfileDto postprocessProfileDto)
        {
            PostprocessProfile entity = new()
            {
                Id = postprocessProfileDto.Id,
                Name = postprocessProfileDto.Name,
                Type = postprocessProfileDto.Type,
                WarehouseId = postprocessProfileDto.WarehouseId,
                Warehouse = null
            };

            return entity;
        }

        private OrderSchedule MapperEntityPreprocessProfile(OrderProfilesDto item)
        {
            OrderSchedule entity = new()
            {
                Id = item.Id,
                Load = null,
                LoadId = item.Load.Id,
                Vehicle= null,
                VehicleId = item.Vehicle.Id,
                NumberVehicles = item.NumberVehicles,
                InitHour= item.InitHour,
                EndHour= item.EndHour,
                IsOut = item.IsOut,
                WarehouseId = item.WarehouseId,
                Warehouse = null
            };

            return entity;
        }

        private OrderLoadRatio MapperEntityOrderLoadProperties(OrderLoadPropertiesDto item)
        {
            OrderLoadRatio entity = new()
            {
                Id = item.Id,
                LoadId = item.Load.Id,
                Load = null,
                VehicleId = item.Vehicle.Id,
                Vehicle = null,
                LoadInVehicle =  item.LoadInVehicle,
                OrderInLoad = item.OrderInLoad !=0 ? 1/item.OrderInLoad :0,
            };

            return entity;
        }

        private SelectionLoadDto MapLoad(LoadProfile vp)
        {
            return new SelectionLoadDto
            {
                Id = vp.Id,
                Name = vp.Name,
            };
        }

        private SelectionVehicleDto MapVehicle(VehicleProfile vp)
        {
            return new SelectionVehicleDto
            {
                Id = vp.Id,
                Name = vp.Name,
            };
        }

        #endregion

        #region SaveChanges
        public async Task LoadProfileSaveChanges(IEnumerable<LoadProfileDto> loadProfileDtos, Guid whId)
        {
            OperationDB operation = new OperationDB();
            var warehouseId = whId;
            try
            {
                foreach (var profileDto in loadProfileDtos)
                {
                    if (profileDto.DataOperationType == OperationType.Insert)
                    {
                        profileDto.WarehouseId = warehouseId;
                        operation.AddNew(EntityNamesConst.LoadProfile, MapperEntityLoadProfile(profileDto));
                    }
                    else if (profileDto.DataOperationType == OperationType.Update)
                        operation.AddUpdate(EntityNamesConst.LoadProfile, MapperEntityLoadProfile(profileDto));
                    else if (profileDto.DataOperationType == OperationType.Delete)
                        operation.AddDelete(EntityNamesConst.LoadProfile, new EntityDto() { Id = profileDto.Id });
                }

                await _simulateService.SaveChangesInDataBase(operation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProfileService::LoadProfileSaveChanges => Error to save changes");
            }
        }

        public async Task VehicleProfileSaveChanges(IEnumerable<VehicleProfileDto> vehicleProfileDtos, Guid whId)
        {
            OperationDB operation = new OperationDB();
            var warehouseId = whId;
            try
            {
                foreach (var profileDto in vehicleProfileDtos)
                {
                    if (profileDto.DataOperationType == OperationType.Insert)
                    {
                        profileDto.WarehouseId = warehouseId;
                        operation.AddNew(EntityNamesConst.VehicleProfile, MapperEntityVehicleProfile(profileDto));
                    }
                    else if (profileDto.DataOperationType == OperationType.Update)
                        operation.AddUpdate(EntityNamesConst.VehicleProfile, MapperEntityVehicleProfile(profileDto));
                    else if (profileDto.DataOperationType == OperationType.Delete)
                        operation.AddDelete(EntityNamesConst.VehicleProfile, new EntityDto() { Id = profileDto.Id });
                }

                await _simulateService.SaveChangesInDataBase(operation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProfileService::VehicleProfileSaveChangesv => Error to save changes");
            }
        }

        public async Task PutawayProfileSaveChanges(IEnumerable<PutawayProfileDto> putawayProfileDtos, Guid whId)
        {
            OperationDB operation = new OperationDB();
            var warehouseId = whId;
            try
            {
                foreach (var profileDto in putawayProfileDtos)
                {
                    if (profileDto.DataOperationType == OperationType.Insert)
                    {
                        profileDto.WarehouseId = warehouseId;
                        operation.AddNew(EntityNamesConst.PutawayProfile, MapperEntityPutawayProfile(profileDto));
                    }
                    else if (profileDto.DataOperationType == OperationType.Update)
                        operation.AddUpdate(EntityNamesConst.PutawayProfile, MapperEntityPutawayProfile(profileDto));
                    else if (profileDto.DataOperationType == OperationType.Delete)
                        operation.AddDelete(EntityNamesConst.PutawayProfile, new EntityDto() { Id = profileDto.Id });
                }

                await _simulateService.SaveChangesInDataBase(operation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProfileService::PutawayProfileSaveChanges => Error to save changes");
            }
        }

        public async Task PostprocessProfileSaveChanges(IEnumerable<PostprocessProfileDto> postprocessProfileDtos, Guid whId)
        {
            OperationDB operation = new OperationDB();
            var warehouseId = whId;
            try
            {
                foreach (var profileDto in postprocessProfileDtos)
                {
                    if (profileDto.DataOperationType == OperationType.Insert)
                    {
                        profileDto.WarehouseId = warehouseId;
                        operation.AddNew(EntityNamesConst.PostprocessProfile, MapperEntityPostprocessProfile(profileDto));
                    }
                    else if (profileDto.DataOperationType == OperationType.Update)
                        operation.AddUpdate(EntityNamesConst.PostprocessProfile, MapperEntityPostprocessProfile(profileDto));
                    else if (profileDto.DataOperationType == OperationType.Delete)
                        operation.AddDelete(EntityNamesConst.PostprocessProfile, new EntityDto() { Id = profileDto.Id });
                }

                await _simulateService.SaveChangesInDataBase(operation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProfileService::PostprocessProfileSaveChanges => Error to save changes");
            }
        }

        public async Task PreprocessProfileSaveChanges(IEnumerable<OrderProfilesDto> preprocessProfileDtos, Guid whId)
        {
            OperationDB operation = new OperationDB();
            var warehouseId = whId;
            try
            {
                foreach (var profileDto in preprocessProfileDtos)
                {
                    if (profileDto.DataOperationType == OperationType.Insert)
                    {
                        profileDto.WarehouseId = warehouseId;
                        operation.AddNew(EntityNamesConst.OrderSchedule, MapperEntityPreprocessProfile(profileDto));
                    }
                    else if (profileDto.DataOperationType == OperationType.Update)
                        operation.AddUpdate(EntityNamesConst.OrderSchedule, MapperEntityPreprocessProfile(profileDto));
                    else if (profileDto.DataOperationType == OperationType.Delete)
                        operation.AddDelete(EntityNamesConst.OrderSchedule, new EntityDto() { Id = profileDto.Id });
                }

                await _simulateService.SaveChangesInDataBase(operation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProfileService::PreprocessProfileSaveChanges => Error to save changes");
            }
        }

        public async Task ProprocessProfileSaveChanges(IEnumerable<OrderLoadPropertiesDto> preprocessProfileDtos, Guid whId)
        {
            OperationDB operation = new OperationDB();
            var warehouseId = whId;
            try
            {
                foreach (var profileDto in preprocessProfileDtos)
                {
                    if (profileDto.DataOperationType == OperationType.Insert)
                    {
                        profileDto.WarehouseId = warehouseId;
                        operation.AddNew(EntityNamesConst.OrderLoadRatio, MapperEntityOrderLoadProperties(profileDto));
                    }
                    else if (profileDto.DataOperationType == OperationType.Update)
                        operation.AddUpdate(EntityNamesConst.OrderLoadRatio, MapperEntityOrderLoadProperties(profileDto));
                    else if (profileDto.DataOperationType == OperationType.Delete)
                        operation.AddDelete(EntityNamesConst.OrderLoadRatio, new EntityDto() { Id = profileDto.Id });
                }

                await _simulateService.SaveChangesInDataBase(operation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProfileService::PreprocessProfileSaveChanges => Error to save changes");
            }
        }

        #endregion
    }
}

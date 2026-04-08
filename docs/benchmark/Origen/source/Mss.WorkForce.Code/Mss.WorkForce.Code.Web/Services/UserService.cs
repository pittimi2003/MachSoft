using Mss.WorkForce.Code.Models.ConvertedModel;
using Mss.WorkForce.Code.Models.DataAccess;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Models.Models;

namespace Mss.WorkForce.Code.Web.Services
{
    public class UserService : IUserService
    {
        private readonly DataAccess _dataAccess;
        private readonly ILogger<UserService> _logger;
        private readonly ISimulateService _simulateService;

        public UserService(DataAccess dataAccess, ILogger<UserService> logger, ISimulateService simulateService)
        {
            _dataAccess = dataAccess;
            _logger = logger;
            _simulateService = simulateService;
        }

        public UserDto GetUsersById(Guid userId)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteUser(List<UserDto> lstUser)
        {
            try
            {
                OperationDB operation = new OperationDB();
                
                foreach (var item in lstUser)
                {
                    operation.AddDelete("Users", new EntityDto() { Id = item.Id });
                }
                
                await _simulateService.SaveChangesInDataBase(operation);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IEnumerable<UserDto> GetUsers(Guid OrganizationId)
        {
            List<UserDto> users = new();

            try
            {
                foreach (var item in _dataAccess.GetUser(OrganizationId))
                {
                    users.Add(MapperUser(item));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UserService::GetUsers => Error get list of users in database");
            }

            return users;
        }

        public async Task AddUser(UserDto dto)
        {
            try
            {
                OperationDB operation = new OperationDB();
                var user = MapDtoToUser(dto);
                user.OrganizationId = GetOnlyOrganization().Id;
               
                List<Guid> warehouseIds = user.Warehouses.Select(w => w.Id).ToList();
                user.Warehouses = null;
                
                operation.AddNew("Users", user);
                await _simulateService.SaveChangesInDataBase(operation);

                UpdateUserToWarehouse(user.Id, warehouseIds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UserService::AddUser => Error adding user to database");
                throw;
            }
        }

        public async Task UpdateUser(List<UserDto> lstUserDto)
        {
            try
            {
                OperationDB operation = new OperationDB();

                foreach (var item in lstUserDto)
                {
                    List<Guid> warehouseIds = item.Warehouses.Select(w => w.Id).ToList();
                    await UpdateUserToWarehouse(item.Id, warehouseIds);

                    var user = MapDtoToUser(item);
                    user.OrganizationId = GetOnlyOrganization().Id;
                    user.Warehouses = null;
                    operation.AddUpdate("Users", user);
                }               
                
                await _simulateService.SaveChangesInDataBase(operation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"UserService::UpdateUser => Error updating user");
            }
        }

        private User MapDtoToUser(UserDto dto)
        {
            return new User
            {
                Id = dto.Id,
                Code = dto.Code,
                Name = dto.Name,
                Lastname = dto.Lastname,
                Password = dto.Password,
                IsEnabled = dto.IsEnabled,
                IsActive = dto.IsActive,
                DecimalSeparatorId = dto.RegionalSettings.DecimalSeparator.Id,
                ThousandsSeparatorId = dto.RegionalSettings.ThousandsSeparator.Id,
                DateFormatId = dto.RegionalSettings.DateFormat.Id,
                HourFormatId = dto.RegionalSettings.HourFormat.Id,
                LanguageId = dto.RegionalSettings.Language.Id,
                LastAccessDate = dto.LastAccessDate,
                CreationDate = dto.CreationDate,
                OrganizationId = dto.OrganizationId,
                WarehouseDefaultId = dto.WarehouseDefaultId,
                Warehouses = dto.Warehouses,
            };
        }

        private UserDto MapperUser(User user)
        {
            UserDto userDto = new()
            {
                Id = user.Id,
                Code = user.Code,
                Name = user.Name,
                Lastname = user.Lastname,
                Password = user.Password,
                IsEnabled = user.IsEnabled,
                IsActive = user.IsActive,
                RegionalSettings = new RegionalSettings()
                {
                    DecimalSeparator = user.DecimalSeparator,
                    ThousandsSeparator = user.ThousandsSeparator,
                    DateFormat = user.DateFormat,
                    HourFormat = user.HourFormat,
                    Language = user.Language,
                },
                LastAccessDate = user.LastAccessDate,
                CreationDate = user.CreationDate,
                OrganizationId = user.OrganizationId,
                WarehouseDefaultId = user.WarehouseDefaultId,
                Warehouses = user.Warehouses,
            };

            return userDto;
        }

        public async Task UpdateUserToWarehouse(Guid userId, List<Guid> warehouseIds)
        {
            _dataAccess.UpdateUserToWarehouse(userId, warehouseIds);
        }

        public Organization GetOnlyOrganization()
        {
            return _dataAccess.GetOnlyOrganization();
        }

        public UserDto GetUserByCode(string code)
        {
            var user = _dataAccess.GetUserByCode(code);
            var userdto = MapperUser(user);
            return userdto;
        }
    }
}

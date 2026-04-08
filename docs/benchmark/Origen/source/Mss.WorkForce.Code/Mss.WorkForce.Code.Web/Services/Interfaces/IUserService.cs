using Mss.WorkForce.Code.Models.DTO;

namespace Mss.WorkForce.Code.Web.Services
{
    public interface IUserService
    {  
        Task DeleteUser(List<UserDto> lstUser);
        IEnumerable<UserDto> GetUsers(Guid OrganizationId);
        UserDto GetUsersById(Guid userId);
        Task UpdateUser(List<UserDto> lstUser);
        Task AddUser(UserDto user);
        Task UpdateUserToWarehouse(Guid userId, List<Guid> warehouseIds);
        UserDto GetUserByCode(string code);
    }
}

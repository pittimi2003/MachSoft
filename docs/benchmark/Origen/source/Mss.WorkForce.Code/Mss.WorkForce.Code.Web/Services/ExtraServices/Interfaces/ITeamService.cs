using Mss.WorkForce.Code.Models.DTO.ExtraConfiguration;

namespace Mss.WorkForce.Code.Web.Services.ExtraServices.Interfaces
{
    public interface ITeamService
    {
        #region Methods

        Task DeleteTeam(List<TeamDto> lstTeam);
        IEnumerable<TeamDto> GetTeam();
        TeamDto GetTeamById(Guid TeamId);
        Task UpdateTeam(List<TeamDto> lstTeam);
        Task AddTeam(TeamDto Team);

        #endregion
    }
}

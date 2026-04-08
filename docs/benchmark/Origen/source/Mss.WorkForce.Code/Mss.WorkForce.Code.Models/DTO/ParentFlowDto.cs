namespace Mss.WorkForce.Code.Models.DTO
{
    public class ParentFlowDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public string Type { get; set; }

        public static ParentFlowDto? NewDto()
        {
            return new ParentFlowDto
            {
                Id = Guid.NewGuid(),
                Name = string.Empty,
                Type = string.Empty,
            };
        }
    }
}

namespace Mss.WorkForce.Code.Models.DTO
{
    public class OrderPriorityDto
    {
        public Guid Id { get; set; }

        public string Code { get; set; }

        public int Priority { get; set; }

        public Guid WarehouseId { get; set; }

        public string IconClass { get; set; }

        public bool IsActive { get; set; } = false;

        //public bool IsActive { get; set; } = false;

        public static OrderPriorityDto NewDto()
        {
            return new OrderPriorityDto
            {
                Id = Guid.NewGuid(),
                Code = string.Empty,
                Priority = 0,
            };
        }
    }
}

namespace Reservation.SpacesService.DAL.Entities
{
    public class Space
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Location { get; set; }
        public string? Description { get; set; }
        public int Capacity { get; set; }
        public decimal PricePerHour { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
using RestaurantReservationSystem.Core.Enums;

namespace RestaurantReservationSystem.Core.Models {
    public class Reservation {

        // PROPERTIES:
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateOnly Date { get; set; }
        public TimeOnly Time { get; set; }
        public string? FirstName { get; set; } = null;
        public string? LastName { get; set; } = null;
        public string? PhoneNumber { get; set; } = null;
        public int Guests { get; set; }
        public string? Note { get; set; } = null;
        public ReservationStatus Status { get; set; } = ReservationStatus.Expected;


        // NAVIGATION PROPERTIES:
        public ICollection<Table> Tables { get; set; } = new List<Table>();
    }
}
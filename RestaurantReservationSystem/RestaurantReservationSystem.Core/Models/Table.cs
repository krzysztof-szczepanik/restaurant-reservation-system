namespace RestaurantReservationSystem.Core.Models {
    public class Table {

        // PROPERTIES:
        public int Id { get; set; }
        public int? Number { get; set; } = null;
        public string? Label { get; set; } = null;
        public string? Zone { get; set; } = null;
        public int Seats { get; set; }


        // NAVIGATION PROPERTIES:
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
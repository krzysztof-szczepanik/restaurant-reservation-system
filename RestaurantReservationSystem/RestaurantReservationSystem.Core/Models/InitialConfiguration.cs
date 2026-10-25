using RestaurantReservationSystem.Core.Enums;

namespace RestaurantReservationSystem.Core.Models {
    public class InitialConfiguration {
        public int Id { get; set; } = 1;
        public bool FirstNameVisible { get; set; } = true;
        public bool FirstNameRequired { get; set; } = false;
        public bool LastNameVisible { get; set; } = true;
        public bool LastNameRequired { get; set; } = true;
        public bool PhoneNumberVisible { get; set; } = true;
        public bool PhoneNumberRequired { get; set; } = false;
        public bool TableNumberVisible { get; set; } = true;
        public bool TableNumberRequired { get; set; } = true;
        public bool TableLabelVisible { get; set; } = true;
        public bool TableLabelRequired { get; set; } = false;
        public bool TableZoneVisible { get; set; } = true;
        public bool TableZoneRequired { get; set; } = false;
        public TableIdentifierOrder TableIdentifierOrder { get; set; } = TableIdentifierOrder.NumberThenLabel;
    }
}
namespace RestaurantReservationSystem.Application.OperationResults {
    public enum OperationErrorCode {
        TableHasReservations,
        ReservationNotFound,
        TableNotFound,
        InvalidStatusTransition,
        ConfigurationAlreadyExists
    }
}
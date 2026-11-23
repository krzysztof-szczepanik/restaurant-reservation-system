namespace RestaurantReservationSystem.Application.OperationResults {
    public sealed record ValidationError {

        // PROPERTIES:
        public string FieldName { get; }
        public string Message { get; }


        // CONSTRUCTORS:
        public ValidationError(string fieldName, string message) {
            if (string.IsNullOrWhiteSpace(fieldName)) throw new ArgumentException("FieldName cannot be empty.", nameof(fieldName));
            if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("Message cannot be empty.", nameof(message));

            FieldName = fieldName;
            Message = message;
        }
    }
}
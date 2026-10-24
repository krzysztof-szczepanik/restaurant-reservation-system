using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantReservationSystem.Application.OperationResults
{
    public enum OperationErrorCode
    {
        TableHasReservations,
        ReservationNotFound,
        TableNotFound,
        InvalidStatusTransition,
        ConfigurationAlreadyExists
    }
}

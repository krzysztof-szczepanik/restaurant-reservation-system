using System;
using System.Collections.Generic;
using System.Text;

namespace RestaurantReservationSystem.Core.Enums
{
    public enum ReservationStatus
    {
        Expected=0,
        Late,
        Arrived,
        Cancelled,
        NoShow
    }
}

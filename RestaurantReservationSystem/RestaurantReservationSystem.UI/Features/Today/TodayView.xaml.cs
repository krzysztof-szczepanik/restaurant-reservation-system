using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace RestaurantReservationSystem.UI.Features.Today {
    public partial class TodayView : UserControl {

        //  !!!!!!!!!! TEST DATA !!!!!!!!!!
        public ObservableCollection<TodayReservationRow> TodayReservations { get; } = new() {
            new("20.10.2026", "18:30", "Anna", "Kowalska", "501 234 567", 6, "Main Hall: Table 2...", "Birthday dinner...", "Expected"),
            new("20.10.2026", "20:00", "Ewa", "Wisniewska", "507 890 123", 5, "Main Hall: Table 7", "Nut allergy.", "Expected"),
            new("20.10.2026", "18:30", "James", "Wilson", "602 345 678", 4, "Terrace: Table 3", "", "Late"),
            new("20.10.2026", "12:15", "Marta", "Nowak", "703 456 789", 8, "Main Hall: Table 1, 2", "High chair requested.", "Arrived"),
            new("20.10.2026", "18:30", "Oliver", "Brown", "504 567 890", 2, "Terrace: Table 4", "Window seat.", "Arrived"),
            new("20.10.2026", "18:30", "Sophie", "Martin", "685 678 901", 4, "Table 5", "", "Cancelled"),
            new("20.10.2026", "18:30", "Piotr", "Zielinski", "606 789 012", 3, "Main Hall: Table 6", "", "No-show")
        };

        // CONSTRUCTORS:
        public TodayView() {
            InitializeComponent();
            DataContext = this;
        }
    }


    //  !!!!!!!!!! TEST STRUCT !!!!!!!!!!
    public class TodayReservationRow {
        public string Date { get; }
        public string Time { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public string PhoneNumber { get; }
        public int Guests { get; }
        public string Tables { get; }
        public string Note { get; }
        public string Status { get; }

        public string StatusBackground => Status switch {
            "Expected" => "#EFF6FF",
            "Late" => "#FEFCE8",
            "Arrived" => "#F0FDF4",
            "Cancelled" => "#F8FAFC",
            "No-show" => "#FEF2F2",
            _ => "#F8FAFC"
        };

        public string StatusBorder => Status switch {
            "Expected" => "#BFDBFE",
            "Late" => "#FEF08A",
            "Arrived" => "#BBF7D0",
            "Cancelled" => "#E2E8F0",
            "No-show" => "#FECACA",
            _ => "#E2E8F0"
        };

        public string StatusForeground => Status switch {
            "Expected" => "#1D4ED8",
            "Late" => "#A16207",
            "Arrived" => "#15803D",
            "Cancelled" => "#64748B",
            "No-show" => "#DC2626",
            _ => "#64748B"
        };

        public TodayReservationRow(string date, string time, string firstName, string lastName, string phoneNumber, int guests, string tables, string note, string status) {
            Date = date;
            Time = time;
            FirstName = firstName;
            LastName = lastName;
            PhoneNumber = phoneNumber;
            Guests = guests;
            Tables = tables;
            Note = note;
            Status = status;
        }
    }
}
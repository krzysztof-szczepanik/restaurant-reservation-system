namespace RestaurantReservationSystem.Infrastructure.Persistence;

public static class ConnectionStringProvider
{
    public static string GetConnectionString()
    {
        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string directoryPath = Path.Combine(localAppData, "RestaurantReservationSystem");

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string dbPath = Path.Combine(directoryPath, "restaurant.db");
        return $"Data Source={dbPath}";
    }
}
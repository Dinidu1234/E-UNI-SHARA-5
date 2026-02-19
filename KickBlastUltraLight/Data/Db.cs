using Microsoft.Data.Sqlite;
using KickBlastUltraLight.Models;

namespace KickBlastUltraLight.Data;

public static class Db
{
    private static readonly string DataFolder = Path.Combine(AppContext.BaseDirectory, "Data");
    private static readonly string DbPath = Path.Combine(DataFolder, "kickblast_ultra.db");
    private static readonly string ConnectionString = $"Data Source={DbPath}";

    public static void Init()
    {
        Directory.CreateDirectory(DataFolder);

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var createSql = @"
CREATE TABLE IF NOT EXISTS Users (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Username TEXT NOT NULL UNIQUE,
    Password TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Athletes (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    FullName TEXT NOT NULL,
    Age INTEGER NOT NULL,
    Plan TEXT NOT NULL,
    WeightKg REAL NOT NULL,
    TargetWeightKg REAL NOT NULL,
    Notes TEXT NULL
);

CREATE TABLE IF NOT EXISTS MonthlyCalculations (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    AthleteId INTEGER NOT NULL,
    Year INTEGER NOT NULL,
    Month INTEGER NOT NULL,
    Competitions INTEGER NOT NULL,
    CoachingHours INTEGER NOT NULL,
    TrainingCost REAL NOT NULL,
    CoachingCost REAL NOT NULL,
    CompetitionCost REAL NOT NULL,
    TotalCost REAL NOT NULL,
    WeightStatus TEXT NOT NULL,
    PaymentDueDate TEXT NOT NULL,
    CreatedAt TEXT NOT NULL,
    FOREIGN KEY(AthleteId) REFERENCES Athletes(Id)
);

CREATE TABLE IF NOT EXISTS Pricing (
    Id INTEGER PRIMARY KEY,
    BeginnerWeeklyFee REAL NOT NULL,
    IntermediateWeeklyFee REAL NOT NULL,
    AdvancedWeeklyFee REAL NOT NULL,
    CoachingHourlyRate REAL NOT NULL,
    CompetitionFee REAL NOT NULL
);";

        using (var command = new SqliteCommand(createSql, connection))
        {
            command.ExecuteNonQuery();
        }

        SeedData(connection);
    }

    private static void SeedData(SqliteConnection connection)
    {
        using var userCheck = new SqliteCommand("SELECT COUNT(*) FROM Users WHERE Username='dinidu'", connection);
        var userCount = Convert.ToInt32(userCheck.ExecuteScalar());
        if (userCount == 0)
        {
            using var insertUser = new SqliteCommand("INSERT INTO Users (Username, Password) VALUES ('dinidu', '123456')", connection);
            insertUser.ExecuteNonQuery();
        }

        using var pricingCheck = new SqliteCommand("SELECT COUNT(*) FROM Pricing WHERE Id=1", connection);
        var pricingCount = Convert.ToInt32(pricingCheck.ExecuteScalar());
        if (pricingCount == 0)
        {
            using var insertPricing = new SqliteCommand(@"INSERT INTO Pricing (Id, BeginnerWeeklyFee, IntermediateWeeklyFee, AdvancedWeeklyFee, CoachingHourlyRate, CompetitionFee)
VALUES (1, 2500, 3500, 4500, 1500, 2000)", connection);
            insertPricing.ExecuteNonQuery();
        }

        using var athletesCheck = new SqliteCommand("SELECT COUNT(*) FROM Athletes", connection);
        var athleteCount = Convert.ToInt32(athletesCheck.ExecuteScalar());
        if (athleteCount == 0)
        {
            var samples = new[]
            {
                new Athlete { FullName = "Nethmi Perera", Age = 14, Plan = "Beginner", WeightKg = 45, TargetWeightKg = 46, Notes = "Evening batch" },
                new Athlete { FullName = "Kavindu Silva", Age = 17, Plan = "Intermediate", WeightKg = 58, TargetWeightKg = 56, Notes = "State trials" },
                new Athlete { FullName = "Yasiru Fernando", Age = 19, Plan = "Advanced", WeightKg = 67, TargetWeightKg = 65, Notes = "National prep" },
                new Athlete { FullName = "Sethmi Karunarathna", Age = 13, Plan = "Beginner", WeightKg = 41, TargetWeightKg = 43, Notes = "New joiner" },
                new Athlete { FullName = "Malith Ekanayake", Age = 16, Plan = "Intermediate", WeightKg = 60, TargetWeightKg = 58, Notes = "Conditioning" },
                new Athlete { FullName = "Dinuka Jayasinghe", Age = 18, Plan = "Advanced", WeightKg = 70, TargetWeightKg = 68, Notes = "Peak cycle" }
            };

            foreach (var athlete in samples)
            {
                UpsertAthlete(athlete);
            }
        }
    }

    public static bool ValidateLogin(string username, string password)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        using var command = new SqliteCommand("SELECT COUNT(*) FROM Users WHERE Username=@u AND Password=@p", connection);
        command.Parameters.AddWithValue("@u", username);
        command.Parameters.AddWithValue("@p", password);
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    public static Pricing GetPricing()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        using var command = new SqliteCommand("SELECT * FROM Pricing WHERE Id=1", connection);
        using var reader = command.ExecuteReader();
        if (!reader.Read()) return new Pricing();

        return new Pricing
        {
            Id = reader.GetInt32(0),
            BeginnerWeeklyFee = reader.GetDecimal(1),
            IntermediateWeeklyFee = reader.GetDecimal(2),
            AdvancedWeeklyFee = reader.GetDecimal(3),
            CoachingHourlyRate = reader.GetDecimal(4),
            CompetitionFee = reader.GetDecimal(5)
        };
    }

    public static void SavePricing(Pricing pricing)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        using var command = new SqliteCommand(@"UPDATE Pricing SET
BeginnerWeeklyFee=@b,
IntermediateWeeklyFee=@i,
AdvancedWeeklyFee=@a,
CoachingHourlyRate=@c,
CompetitionFee=@f
WHERE Id=1", connection);
        command.Parameters.AddWithValue("@b", pricing.BeginnerWeeklyFee);
        command.Parameters.AddWithValue("@i", pricing.IntermediateWeeklyFee);
        command.Parameters.AddWithValue("@a", pricing.AdvancedWeeklyFee);
        command.Parameters.AddWithValue("@c", pricing.CoachingHourlyRate);
        command.Parameters.AddWithValue("@f", pricing.CompetitionFee);
        command.ExecuteNonQuery();
    }

    public static List<Athlete> GetAthletes(string? keyword = null, string? plan = null)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var sql = "SELECT Id, FullName, Age, Plan, WeightKg, TargetWeightKg, IFNULL(Notes, '') FROM Athletes WHERE 1=1";
        if (!string.IsNullOrWhiteSpace(keyword)) sql += " AND FullName LIKE @k";
        if (!string.IsNullOrWhiteSpace(plan) && plan != "All") sql += " AND Plan = @p";
        sql += " ORDER BY FullName";

        using var command = new SqliteCommand(sql, connection);
        if (!string.IsNullOrWhiteSpace(keyword)) command.Parameters.AddWithValue("@k", $"%{keyword}%");
        if (!string.IsNullOrWhiteSpace(plan) && plan != "All") command.Parameters.AddWithValue("@p", plan);

        var list = new List<Athlete>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new Athlete
            {
                Id = reader.GetInt32(0),
                FullName = reader.GetString(1),
                Age = reader.GetInt32(2),
                Plan = reader.GetString(3),
                WeightKg = reader.GetDecimal(4),
                TargetWeightKg = reader.GetDecimal(5),
                Notes = reader.GetString(6)
            });
        }

        return list;
    }

    public static void UpsertAthlete(Athlete athlete)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        SqliteCommand command;
        if (athlete.Id == 0)
        {
            command = new SqliteCommand(@"INSERT INTO Athletes (FullName, Age, Plan, WeightKg, TargetWeightKg, Notes)
VALUES (@n, @a, @p, @w, @t, @notes)", connection);
        }
        else
        {
            command = new SqliteCommand(@"UPDATE Athletes SET
FullName=@n, Age=@a, Plan=@p, WeightKg=@w, TargetWeightKg=@t, Notes=@notes
WHERE Id=@id", connection);
            command.Parameters.AddWithValue("@id", athlete.Id);
        }

        command.Parameters.AddWithValue("@n", athlete.FullName);
        command.Parameters.AddWithValue("@a", athlete.Age);
        command.Parameters.AddWithValue("@p", athlete.Plan);
        command.Parameters.AddWithValue("@w", athlete.WeightKg);
        command.Parameters.AddWithValue("@t", athlete.TargetWeightKg);
        command.Parameters.AddWithValue("@notes", athlete.Notes);
        command.ExecuteNonQuery();
    }

    public static void DeleteAthlete(int athleteId)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        using var command = new SqliteCommand("DELETE FROM Athletes WHERE Id=@id", connection);
        command.Parameters.AddWithValue("@id", athleteId);
        command.ExecuteNonQuery();
    }

    public static void SaveCalculation(MonthlyCalculation calculation)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        using var command = new SqliteCommand(@"INSERT INTO MonthlyCalculations
(AthleteId, Year, Month, Competitions, CoachingHours, TrainingCost, CoachingCost, CompetitionCost, TotalCost, WeightStatus, PaymentDueDate, CreatedAt)
VALUES
(@athleteId, @year, @month, @competitions, @hours, @training, @coaching, @competition, @total, @status, @due, @created)", connection);

        command.Parameters.AddWithValue("@athleteId", calculation.AthleteId);
        command.Parameters.AddWithValue("@year", calculation.Year);
        command.Parameters.AddWithValue("@month", calculation.Month);
        command.Parameters.AddWithValue("@competitions", calculation.Competitions);
        command.Parameters.AddWithValue("@hours", calculation.CoachingHours);
        command.Parameters.AddWithValue("@training", calculation.TrainingCost);
        command.Parameters.AddWithValue("@coaching", calculation.CoachingCost);
        command.Parameters.AddWithValue("@competition", calculation.CompetitionCost);
        command.Parameters.AddWithValue("@total", calculation.TotalCost);
        command.Parameters.AddWithValue("@status", calculation.WeightStatus);
        command.Parameters.AddWithValue("@due", calculation.PaymentDueDate);
        command.Parameters.AddWithValue("@created", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        command.ExecuteNonQuery();
    }

    public static List<MonthlyCalculation> GetHistory(int? athleteId = null, int? month = null, int? year = null)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var sql = @"SELECT m.Id, m.AthleteId, a.FullName, m.Year, m.Month, m.Competitions, m.CoachingHours,
m.TrainingCost, m.CoachingCost, m.CompetitionCost, m.TotalCost, m.WeightStatus, m.PaymentDueDate, m.CreatedAt
FROM MonthlyCalculations m
INNER JOIN Athletes a ON a.Id = m.AthleteId
WHERE 1=1";

        if (athleteId.HasValue && athleteId.Value > 0) sql += " AND m.AthleteId = @athleteId";
        if (month.HasValue && month.Value > 0) sql += " AND m.Month = @month";
        if (year.HasValue && year.Value > 0) sql += " AND m.Year = @year";
        sql += " ORDER BY m.Id DESC";

        using var command = new SqliteCommand(sql, connection);
        if (athleteId.HasValue && athleteId.Value > 0) command.Parameters.AddWithValue("@athleteId", athleteId.Value);
        if (month.HasValue && month.Value > 0) command.Parameters.AddWithValue("@month", month.Value);
        if (year.HasValue && year.Value > 0) command.Parameters.AddWithValue("@year", year.Value);

        var list = new List<MonthlyCalculation>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new MonthlyCalculation
            {
                Id = reader.GetInt32(0),
                AthleteId = reader.GetInt32(1),
                AthleteName = reader.GetString(2),
                Year = reader.GetInt32(3),
                Month = reader.GetInt32(4),
                Competitions = reader.GetInt32(5),
                CoachingHours = reader.GetInt32(6),
                TrainingCost = reader.GetDecimal(7),
                CoachingCost = reader.GetDecimal(8),
                CompetitionCost = reader.GetDecimal(9),
                TotalCost = reader.GetDecimal(10),
                WeightStatus = reader.GetString(11),
                PaymentDueDate = reader.GetString(12),
                CreatedAt = reader.GetString(13)
            });
        }

        return list;
    }

    public static (int Athletes, int Calculations, decimal Revenue) GetDashboardStats()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var athletesCommand = new SqliteCommand("SELECT COUNT(*) FROM Athletes", connection);
        using var calcCommand = new SqliteCommand("SELECT COUNT(*) FROM MonthlyCalculations", connection);
        using var revCommand = new SqliteCommand("SELECT IFNULL(SUM(TotalCost),0) FROM MonthlyCalculations", connection);

        return (
            Convert.ToInt32(athletesCommand.ExecuteScalar()),
            Convert.ToInt32(calcCommand.ExecuteScalar()),
            Convert.ToDecimal(revCommand.ExecuteScalar())
        );
    }
}

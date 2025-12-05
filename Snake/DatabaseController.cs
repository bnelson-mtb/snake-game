// <copyright file="DatabaseController.cs" company="UofU-CS3500">
// Copyright (c) 2025 UofU-CS3500. All rights reserved.
// </copyright>

using Microsoft.Data.SqlClient;

namespace CS3500.DatabaseController;

/// <summary>
/// Handles all database operations for tracking games and players.
/// This controller manages the Games and Players tables in the SQL database.
/// </summary>
public class DatabaseController
{
    /// <summary>
    /// The connection string for the database. (from secrets file)
    /// </summary>
    private string connectionString
    {
        get
        {
            var builder = new ConfigurationBuilder();

            builder.AddUserSecrets<DatabaseController>();
            IConfigurationRoot configuration = builder.Build();
            var selectedSecrets = configuration.GetSection("DbSecrets");


            return new SqlConnectionStringBuilder()
            {
                DataSource = selectedSecrets[ "Server" ],
                InitialCatalog = selectedSecrets[ "Database" ],
                UserID = selectedSecrets[ "User" ],
                Password = selectedSecrets[ "Password" ],
                ConnectTimeout = 15, // if the server doesn't connect in X seconds, give up
                Encrypt = false
            }.ConnectionString;
        }
    }

    /// <summary>
    /// Executes the given action with a connection to the database.
    /// </summary>
    /// <param name="action"></param>
    private void ExectuteWithConnection(Action<SqlConnection> action)
    {
        using SqlConnection connection = new(connectionString);
        connection.Open();
        action(connection);
    }

    /// <summary>
    /// Executes a query with a connection to the database and returns the result.
    /// </summary>
    /// <param name="query"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    private T QueryWithConnection<T>(Func<SqlConnection, T> query)
    {
    using SqlConnection connection = new(connectionString);
    connection.Open();
    return query(connection);
    }


    /// <summary>
    /// The ID of the current game session. Set when a new game is started.
    /// </summary>
    public int currentGameId { get; private set; } = -1;

    /// <summary>
    /// Tracks which snake IDs have already been recorded in the database for this game session.
    /// Key: snake ID, Value: the max score we've recorded for them.
    /// </summary>
    private Dictionary<int, int> recordedPlayers = new();

    /// <summary>
    /// Creates a new game session entry in the database and sets the CurrentGameId.
    /// Should be called when the client first connects to the server.
    /// </summary>
    /// <returns>The ID of the newly created game.</returns>
    public int StartNewGame()
    {
        string startTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        currentGameId = QueryWithConnection(connection =>
        {
            string insertQuery = "INSERT INTO Games (StartTime) OUTPUT INSERTED.Id VALUES (@StartTime)";
            using SqlCommand command = new(insertQuery, connection);
            command.Parameters.AddWithValue("@StartTime", startTime);
            return Convert.ToInt32(command.ExecuteScalar());
        });
        recordedPlayers.Clear();

        return currentGameId;
    }

    /// <summary>
    /// Updates the end time for the current game session.
    /// Should be called when the client disconnects from the server.
    /// </summary>
    public void EndCurrentGame()
    {
        string endTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        ExectuteWithConnection(connection =>
        {
            string updateQuery = "UPDATE Games SET EndTime = @EndTime WHERE Id = @Id";
            using SqlCommand command = new(updateQuery, connection);
            command.Parameters.AddWithValue("@EndTime", endTime);
            command.Parameters.AddWithValue("@Id", currentGameId);
            command.ExecuteNonQuery();
        });
    }

    /// <summary>
    /// Records a new player in the database, or updates their max score if they already exist.
    /// Should be called whenever a snake is received from the server.
    /// </summary>
    /// <param name="snakeId">The snake's unique ID from the server.</param>
    /// <param name="name">The snake's name.</param>
    /// <param name="score">The snake's current score.</param>
    public void RecordOrUpdatePlayer(Snake.Snake snake)
    {
        // Check if snakeId is already in the dictionary, if not, call InsertNewPlayer
        // If it is, call UpdatePlayerMaxScore
        if (!recordedPlayers.ContainsKey(snake.Id))
        {
            InsertNewPlayer(snake);
            return;
        }

        if (snake.Score > recordedPlayers[ snake.Id ])
        {
            UpdatePlayerMaxScore(snake.Id, snake.Score);
        }

        if (snake.Disconnected)
        {
            RecordPlayerDisconnect(snake.Id);
        }
    }

    /// <summary>
    /// Inserts a new player into the Players table.
    /// </summary>
    private void InsertNewPlayer(Snake.Snake snake)
    {
        string enterTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        ExectuteWithConnection(connection =>
        {
            string insertQuery =
                "INSERT INTO Players (PlayerId, Name, MaxScore, EnterTime, GameId) VALUES (@PlayerId, @Name, @MaxScore, @EnterTime, @GameId)";

            using SqlCommand command = new(insertQuery, connection);
            command.Parameters.AddWithValue("@PlayerId", snake.Id);
            command.Parameters.AddWithValue("@Name", snake.Name);
            command.Parameters.AddWithValue("@MaxScore", snake.Score);
            command.Parameters.AddWithValue("@EnterTime", enterTime);
            command.Parameters.AddWithValue("@GameId", currentGameId);
            command.ExecuteNonQuery();
        });

        // Add snakeId/score to the dictionary
        recordedPlayers[ snake.Id ] = snake.Score;
    }

    /// <summary>
    /// Updates the max score for an existing player.
    /// </summary>
    private void UpdatePlayerMaxScore(int snakeId, int newMaxScore)
    {
        ExectuteWithConnection(connection =>
            {
                string updateQuery = "UPDATE Players SET MaxScore = @MaxScore WHERE PlayerId = @PlayerId AND GameId = @GameId";
                using SqlCommand command = new(updateQuery, connection);
                command.Parameters.AddWithValue("@MaxScore", newMaxScore);
                command.Parameters.AddWithValue("@PlayerId", snakeId);
                command.Parameters.AddWithValue("@GameId", currentGameId);
                command.ExecuteNonQuery();

            });

        recordedPlayers[ snakeId ] = newMaxScore;
    }

    /// <summary>
    /// Records a player's disconnection by setting their leave time.
    /// Should be called when a snake's "dc" property is true.
    /// </summary>
    /// <param name="snakeId">The snake's unique ID.</param>
    public void RecordPlayerDisconnect(int snakeId)
    {
        string LeaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        ExectuteWithConnection(connection =>
        {
            string updateQuery = "UPDATE Players SET LeaveTime = @LeaveTime WHERE PlayerId = @PlayerId AND GameId = @GameId";

            using SqlCommand command = new(updateQuery, connection);
            command.Parameters.AddWithValue("@LeaveTime", LeaveTime);
            command.Parameters.AddWithValue("@PlayerId", snakeId);
            command.Parameters.AddWithValue("@GameId", currentGameId);

            command.ExecuteNonQuery();
        });
    }

    /// <summary>
    /// Gets all games from the database.
    /// </summary>
    /// <returns>List of tuples containing (Id, StartTime, EndTime).</returns>
    public List<(int Id, DateTime StartTime, DateTime? EndTime)> GetAllGames()
    {
        return QueryWithConnection(connection =>
        {
            var games = new List<(int Id, DateTime StartTime, DateTime? EndTime)>();
            string query = "SELECT Id, StartTime, EndTime FROM Games ORDER BY Id";

            using SqlCommand command = new(query, connection);
            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                int id = reader.GetInt32(0);
                DateTime startTime = reader.GetDateTime(1);
                DateTime? endTime = reader.IsDBNull(2) ? null : reader.GetDateTime(2);
                games.Add((id, startTime, endTime));
            }

            return games;
        });
    }

    /// <summary>
    /// Gets all players for a specific game.
    /// </summary>
    /// <param name="gameId">The game ID.</param>
    /// <returns>List of tuples containing (PlayerId, Name, MaxScore, EnterTime, LeaveTime).</returns>
    public List<(int PlayerId, string Name, int MaxScore, DateTime EnterTime, DateTime? LeaveTime)> GetPlayersForGame(int gameId)
    {
        return QueryWithConnection(connection =>
        {
            var players = new List<(int PlayerId, string Name, int MaxScore, DateTime EnterTime, DateTime? LeaveTime)>();
            string query = "SELECT PlayerId, Name, MaxScore, EnterTime, LeaveTime FROM Players WHERE GameId = @GameId ORDER BY MaxScore DESC, PlayerId";

            using SqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@GameId", gameId);
            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                int playerId = reader.GetInt32(0);
                string name = reader.GetString(1);
                int maxScore = reader.GetInt32(2);
                DateTime enterTime = reader.GetDateTime(3);
                DateTime? leaveTime = reader.IsDBNull(4) ? null : reader.GetDateTime(4);
                players.Add((playerId, name, maxScore, enterTime, leaveTime));
            }

            return players;
        });
    }

    /// <summary>
    /// Checks if a game with the given ID exists.
    /// </summary>
    /// <param name="gameId">The game ID to check.</param>
    /// <returns>True if the game exists, false otherwise.</returns>
    public bool GameExists(int gameId)
    {
        return QueryWithConnection(connection =>
        {
            string query = "SELECT COUNT(*) FROM Games WHERE Id = @Id";

            using SqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@Id", gameId);
            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        });
    }
}

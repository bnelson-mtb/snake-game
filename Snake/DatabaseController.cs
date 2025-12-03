// <copyright file="DatabaseController.cs" company="UofU-CS3500">
// Copyright (c) 2025 UofU-CS3500. All rights reserved.
// </copyright>

using Microsoft.Data.SqlClient;

namespace Snake;

/// <summary>
/// Handles all database operations for tracking games and players.
/// This controller manages the Games and Players tables in the SQL database.
/// </summary>
public class DatabaseController
{
    /// <summary>
    /// The connection string for the database. (from secrets file)
    /// </summary>
    private readonly string connectionString;

    /// <summary>
    /// The ID of the current game session. Set when a new game is started.
    /// </summary>
    public int currentGameId { get; private set; } = -1;

    /// <summary>
    /// Tracks which snake IDs have already been recorded in the database for this game session.
    /// Key: snake ID, Value: the max score we've recorded for them.
    /// </summary>
    private readonly Dictionary<int, int> recordedPlayers = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseController"/> class.
    /// </summary>
    /// <param name="connectionString">The SQL Server connection string.</param>
    public DatabaseController(string connectionString)
    {
        this.connectionString = connectionString;
    }

    /// <summary>
    /// Creates a new game session entry in the database and sets the CurrentGameId.
    /// Should be called when the client first connects to the server.
    /// </summary>
    /// <returns>The ID of the newly created game.</returns>
    public int StartNewGame()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Updates the end time for the current game session.
    /// Should be called when the client disconnects from the server.
    /// </summary>
    public void EndCurrentGame()
    {
        //
    }

    /// <summary>
    /// Records a new player in the database, or updates their max score if they already exist.
    /// Should be called whenever a snake is received from the server.
    /// </summary>
    /// <param name="snakeId">The snake's unique ID from the server.</param>
    /// <param name="name">The snake's name.</param>
    /// <param name="score">The snake's current score.</param>
    public void RecordOrUpdatePlayer(int snakeId, string name, int score)
    {
       //
    }

    /// <summary>
    /// Inserts a new player into the Players table.
    /// </summary>
    private void InsertNewPlayer(int snakeId, string name, int score)
    {
        //
    }

    /// <summary>
    /// Updates the max score for an existing player.
    /// </summary>
    private void UpdatePlayerMaxScore(int snakeId, int newMaxScore)
    {
       //
    }

    /// <summary>
    /// Records a player's disconnection by setting their leave time.
    /// Should be called when a snake's "dc" property is true.
    /// </summary>
    /// <param name="snakeId">The snake's unique ID.</param>
    public void RecordPlayerDisconnect(int snakeId)
    {
        //
    }
}

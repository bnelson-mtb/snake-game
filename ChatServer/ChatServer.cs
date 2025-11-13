// <copyright file="ChatServer.cs" company="PlaceholderCompany">
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// </copyright>

using CS3500.Networking;
using CS3500.Networking;
using Microsoft.Extensions.Logging;

namespace CS3500.ChatServer;

/// <summary>
///   A simple ChatServer that handles clients separately and replies with a static message.
/// </summary>
public partial class ChatServer
{
    private static readonly Dictionary<NetworkConnection, string> Names = new();
    private static object locker = new object();
    private static IList<NetworkConnection> connections = new List<NetworkConnection>();
    private static ILogger? logger;

    /// <summary>
    ///   The main program.
    /// </summary>
    private static void Main()
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
            builder.SetMinimumLevel(LogLevel.Trace);
        });
        logger = loggerFactory.CreateLogger("ChatServer");

        ServerConnection.WaitForConnections( HandleConnect, 11_000, logger );
        Console.Read(); // don't stop the program.
    }

    /// <summary>
    ///   <para>
    ///     Current (Wrong) Functionality: When a new connection is established,
    ///     enter a loop that receives from the client and sends "thanks" back
    ///     to the client for each message received.
    ///   </para>
    /// </summary>
    private static void HandleConnect( NetworkConnection connection )
    {
        // handle all messages until disconnect.
        try
        {
            string name = connection.ReceiveLine( );

            // Ensures thread safety
            lock (locker)
            {
                connections.Add(connection);
                Names[ connection ] = name;
            }

            logger?.LogInformation("Client named '{Name}' connected.", name);
            while ( true )
            {
                string msg = connection.ReceiveLine( );
                string line = $"{name}: {msg}";
                logger?.LogInformation("BROADCAST: {0}", line);
                lock (locker)
                {
                    foreach (NetworkConnection conn in connections)
                    {
                        conn.SendLine( line );
                    }
                }
            }
        }
        catch ( Exception e)
        {
            // Disconnect or read error — remove from lists.
            string who;
            lock (locker)
            {
                connections.Remove(connection);
                who = Names.TryGetValue(connection, out var nm) ? nm : "(unknown)";
                Names.Remove(connection);
            }

            LoggerFactory.Create(builder => builder.AddDebug())
                .CreateLogger("ClientHandler")
                .LogInformation(e, "Client '{Name}' disconnected.", who);
        }
    }
}

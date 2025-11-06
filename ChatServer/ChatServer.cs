// <copyright file="ChatServer.cs" company="UofU-CS3500">
// Copyright (c) 2024 UofU-CS3500. All rights reserved.
// </copyright>

using System.Data;
using System.Runtime.CompilerServices;
using CS3500.Networking;
using Microsoft.Extensions.Logging;

namespace CS3500.Chatting;

/// <summary>
///   A simple ChatServer that handles clients separately and replies with a static message.
/// </summary>
public partial class ChatServer
{
    private static Dictionary<NetworkConnection, string> names = new();
    private static object locker = new object();
    private static IList<NetworkConnection> connections = new List<NetworkConnection>();
    private static ILogger logger;
    /// <summary>
    ///   The main program.
    /// </summary>
    /// <param name="args"> ignored. </param>
    /// <returns> A Task. Not really used. </returns>
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
    ///   <para>
    ///     TODO: Expected functionality: When a new connection is established:
    ///   </para>
    ///   <list type="number">
    ///     <item>
    ///       Read the name of the connection and store it.  
    ///     </item>
    ///     <item>
    ///       Begin a loop that reads messages from the client.
    ///     </item>
    ///     <item>
    ///       For each message received, broadcast that message to all connected clients,
    ///     </item>
    ///     <item>
    ///       If the client disconnects (this will throw an exception), remove them from the list of connected clients.
    ///     </item>
    ///   </list>
    ///   <para>
    ///     All actions on the list of connected clients must be thread-safe!
    ///   </para>
    ///   <para> 
    ///     All important events (connections, disconnections, messages received, messages sent, errors, etc.)
    ///     must be logged using the logging system at the appropriate log level.
    ///   </para>
    /// </summary>
    private static void HandleConnect( NetworkConnection connection )
    {
        // handle all messages until disconnect.
        try
        {
            string name = connection.ReceiveLine( );
            // Ensures thread safety
            lock ( locker )
            {
                connections.Add( connection );
                names[connection] = name;
            }
            logger.LogInformation("Client named '{Name}' connected.", name);
            while ( true )
            {
                string msg = connection.ReceiveLine( );
                string line = $"{name}: {msg}";
                logger.LogInformation("BROADCAST: {0}", line);
                foreach (NetworkConnection conn in connections)
                {
                    conn.SendLine( line );
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
                who = names.TryGetValue(connection, out var nm) ? nm : "(unknown)";
                names.Remove(connection);
            }
            LoggerFactory.Create(builder => builder.AddDebug())
                .CreateLogger("ClientHandler")
                .LogInformation(e, "Client '{Name}' disconnected.", who);
        }
    }
}

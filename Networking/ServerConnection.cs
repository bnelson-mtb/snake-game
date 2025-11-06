// <copyright file="Server.cs" company="UofU-CS3500">
// Copyright (c) 2024 UofU-CS3500. All rights reserved.
// </copyright>

using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace CS3500.Networking;

/// <summary>
///   Represents a server task that waits for connections on a given
///   port and calls the provided delegate when a connection is made.
/// </summary>
public static class ServerConnection
{
    /// <summary>
    ///   Use on a TcpListener to handle new connections. Alert the calling program/function
    ///   via the handleConnect delegate.
    /// </summary>
    /// <param name="handleConnect">
    ///   Handler for what the user wants to do when a connection is made.
    ///   This should be run asynchronously via a new thread.
    /// </param>
    /// <param name="port"> The port (e.g., 11000) to listen on. </param>
    public static void WaitForConnections( Action<NetworkConnection> handleConnect, int port, ILogger logger )
    {
        TcpListener s = new TcpListener( IPAddress.Any, port );
        s.Start();
        logger.LogInformation( "Server listening on port {0}", port );
        while (true)
        {
            TcpClient client = s.AcceptTcpClient();
            logger.LogInformation( "Client connected." );

            NetworkConnection connection = new( client, logger );

            if ( !connection.IsConnected )
            {
                logger.LogWarning( "Connection failed to establish." );
                continue;
            }

            var thread = new Thread(() =>
            {
                try
                {
                    handleConnect( connection );
                }
                catch ( Exception e )
                {
                    logger.LogError( "Error in connection handler: {0}", e );
                }
                finally
                {
                    connection.Disconnect();
                    logger.LogInformation( "Client disconnect." );
                }
            } );

            thread.Start();
        }
    }
}

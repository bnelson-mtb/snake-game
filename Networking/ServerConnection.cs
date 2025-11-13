// <copyright file="ServerConnection.cs" company="PlaceholderCompany">
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// </copyright>

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
    /// <param name="logger">The logger.</param>
    public static void WaitForConnections(Action<NetworkConnection> handleConnect, int port, ILogger logger)
    {
        TcpListener s = new TcpListener(IPAddress.Any, port);
        s.Start();
        logger.LogInformation("Server listening on port {0}", port);
        while (true)
        {
            TcpClient client = s.AcceptTcpClient();
            logger.LogInformation("Client connected.");

            NetworkConnection connection = new(client, logger);

            if (!connection.IsConnected)
            {
                logger.LogWarning("Connection failed to establish.");
                continue;
            }

            var thread = new Thread(() =>
            {
                try
                {
                    handleConnect(connection);
                }
                catch (Exception e)
                {
                    logger.LogError("Error in connection handler: {0}", e);
                }
                finally
                {
                    connection.Disconnect();
                    logger.LogInformation("Client disconnect.");
                }
            });

            thread.Start();
        }
    }
}

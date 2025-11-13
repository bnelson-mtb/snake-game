// <copyright file="NetworkConnection.cs" company="PlaceholderCompany">
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// </copyright>

using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;

namespace CS3500.Networking;

/// <summary>
///   <para>
///     Wraps the StreamReader/Writer/TcpClient together so we
///     don't have to keep creating all three for network actions.
///   </para>
///   <para>
///     Note: In C#, the sealed keyword prevents further inheritance,
///     i.e., no class can derive from this one.  We do this because the
///     class is a stable, final abstraction around a TCP socket.
///   </para>
///   <para>
///     Implements IDisposable because we want to make sure that any given
///     network connection is "cleaned up" when we are done with it.
///   </para>
/// </summary>
public sealed class NetworkConnection : IDisposable
{
    /// <summary>
    ///   The connection/socket abstraction.
    /// </summary>
    private readonly TcpClient _tcpClient = new();

    /// <summary>
    ///   The logger.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    ///   Reading end of the connection.
    /// </summary>
    private StreamReader _reader = StreamReader.Null;

    /// <summary>
    ///   Writing end of the connection.
    /// </summary>
    private StreamWriter? _writer = StreamWriter.Null;

    /// <summary>
    ///   Initializes a new instance of the <see cref="NetworkConnection"/> class.
    ///   <para>
    ///     Create a network connection object.
    ///   </para>
    /// </summary>
    /// <param name="tcpClient">
    ///   An already existing TcpClient.
    /// </param>
    /// <param name="logger"> The logging element. </param>
    public NetworkConnection(TcpClient tcpClient, ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tcpClient = tcpClient ?? throw new ArgumentNullException(nameof(tcpClient));

        if (IsConnected)
        {
            // Only establish the reader/writer if the provided TcpClient is already connected.
            var stream = _tcpClient.GetStream();
            _reader = new StreamReader(stream, Encoding.UTF8);
            _writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
            _logger.LogDebug("Initialized NetworkConnection with pre-connected TcpClient.");
        }
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="NetworkConnection"/> class.
    ///   <para>
    ///     Create a network connection object.  The tcpClient will be unconnected at the start.
    ///   </para>
    /// <param name="logger">The logger.</param>>
    /// </summary>
    public NetworkConnection( ILogger logger )
        : this( new TcpClient(), logger )
    {
    }

    /// <summary>
    /// Gets a value indicating whether the socket is connected.
    /// </summary>
    public bool IsConnected
    {
        get
        {
            try
            {
                return _tcpClient.Connected;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    ///   Try to connect to the given host:port.
    /// </summary>
    /// <param name="host"> The URL or IP address, e.g., www.cs.utah.edu, or  127.0.0.1. </param>
    /// <param name="port"> The port, e.g., 11000. </param>
    public void Connect(string host, int port)
    {
        if (IsConnected)
        {
            _logger.LogDebug("Connect() called but already connected.");
            return;
        }

        _logger.LogInformation( "Connecting to {0}:{1}", host, port );
        _tcpClient.Connect(host, port);

        var stream = _tcpClient.GetStream();
        _reader = new StreamReader(stream, Encoding.UTF8);
        _writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
        _logger.LogInformation("Connected to {Host}:{Port}.", host, port);
    }

    /// <summary>
    ///   Send a message to the remote server.  If the <paramref name="message"/> contains
    ///   new lines, these will be treated on the receiving side as multiple messages.
    ///   This method should attach a newline to the end of the <paramref name="message"/>
    ///   (by using WriteLine).
    ///   If this operation can not be completed (e.g. because this NetworkConnection is not
    ///   connected), throw an InvalidOperationException.
    /// </summary>
    /// <param name="message"> The string of characters to send. </param>
    public void SendLine(string message)
    {
        if (!IsConnected || _writer is null)
        {
            _logger.LogError("Failed to write message \"{0}\". Client is not connected!", message);
            throw new InvalidOperationException("Not connected.");
        }

        _writer.WriteLine(message);
        _logger.LogTrace("Msg broadcasted -> {Message}", message);
    }

    /// <summary>
    ///   Read a message from the other side of the socket.  The message will contain
    ///   all characters up to the first new line. See <see cref="SendLineAsync"/>.
    /// </summary>
    /// <remarks>
    ///   <list type="bullet">
    ///     <item>
    ///       It is possible for this method to block indefinitely if the other side
    ///       doesn't send any data.
    ///     </item>
    ///     <item>
    ///       It is possible for this method to return an empty string if the other side
    ///       sends an "empty" message.
    ///     </item>
    ///   </list>
    /// </remarks>
    /// <returns> The contents of the message. </returns>
    /// <exception cref="InvalidOperationException">
    ///   An InvalidOperationException will be thrown if the connection is not established.
    /// </exception>
    /// <exception cref="IOException">
    ///   Thrown if an I/O error occurs while reading from the stream, for example:
    ///   <list type="bullet">
    ///     <item>The stream was closed (usually by the other side quitting).</item>
    ///     <item>The underlying network connection was lost.</item>
    ///   </list>
    ///   <remarks>
    ///     It is acceptable (in most cases) for your external code to catch the generic
    ///     (base type) "Exception" type when using this method, as regardless of which exception
    ///     is thrown, the connection is no longer usable.
    ///   </remarks>
    /// </exception>
    public string ReceiveLine()
    {
        if (!IsConnected || _reader == StreamReader.Null)
        {
            _logger.LogError("Failed to receive message. Client is not connected!");
            throw new InvalidOperationException( "Not connected" );
        }

        string? line = _reader.ReadLine();

        if (line is null)
        {
            _logger.LogInformation("Client closed connection.");
            throw new IOException("Client closed the connection.");
        }

        _logger.LogTrace("Msg received -> {Message}", line);
        return line;
    }

    /// <summary>
    ///   If connected, disconnect the connection and clean.
    ///   up (dispose) any streams.
    /// </summary>
    public void Disconnect()
    {
        if (!IsConnected)
        {
            _logger.LogDebug("Disconnect() called but client is not connected.");
            return;
        }

        _logger.LogInformation("Disconnecting TCP client...");

        try
        {
            _tcpClient.Client.Shutdown(SocketShutdown.Both);
            _logger.LogTrace("Socket shutdown successful.");
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Socket shutdown error.");
        }

        try
        {
            _logger.LogInformation("Disposing the network streams...");
            _writer?.Dispose();
            _reader.Dispose();
            _tcpClient.Dispose();
            _logger.LogInformation("Network streams and client disposed.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error while disposing streams or client.");
        }

        _logger.LogInformation("Disconnected.");
    }

    /// <summary>
    ///   Automatically called with a using statement (see IDisposable).
    /// </summary>
    public void Dispose( )
    {
        Disconnect();
    }
}

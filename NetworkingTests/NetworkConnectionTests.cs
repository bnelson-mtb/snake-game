// <copyright file="NetworkConnectionTests.cs" company="PlaceholderCompany">
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// </copyright>

using System.Net;
using System.Net.Sockets;
using CS3500.Networking;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Networking;
using Xunit;

namespace NetworkingTests;

/// <summary>
/// Minimal ILogger implementation that captures formatted log messages for assertion.
/// </summary>
internal sealed class CapturingLogger : ILogger
{
    private readonly List<string> _messages = [];

    public IReadOnlyList<string> Messages => _messages;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        _messages.Add(formatter(state, exception));
    }

    public bool Contains(string substring) =>
        _messages.Any(m => m.Contains(substring, StringComparison.OrdinalIgnoreCase));
}

/// <summary>
/// Unit and integration tests for <see cref="NetworkConnection"/>.
/// Live TCP pairs on loopback are used to exercise real network paths.
/// </summary>
public sealed class NetworkConnectionTests : IDisposable
{
    private readonly NullLogger<NetworkConnectionTests> _logger = NullLogger<NetworkConnectionTests>.Instance;
    private readonly List<IDisposable> _disposables = [];

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Starts a loopback listener, connects a client, and accepts the server side.
    /// All returned objects are registered for cleanup.
    /// </summary>
    private (TcpListener Listener, TcpClient ServerSide, TcpClient ClientSide) CreateConnectedPair()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint!).Port;

        var clientSide = new TcpClient();
        clientSide.Connect(IPAddress.Loopback, port);
        var serverSide = listener.AcceptTcpClient();

        _disposables.Add(clientSide);
        _disposables.Add(serverSide);

        return (listener, serverSide, clientSide);
    }

    public void Dispose()
    {
        foreach (var d in _disposables)
        {
            try { d.Dispose(); }
            catch { /* best-effort cleanup */ }
        }
    }

    // -----------------------------------------------------------------------
    // Constructor(TcpClient, ILogger) — B1–B4
    // -----------------------------------------------------------------------

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // B1: null logger guard
        Assert.Throws<ArgumentNullException>(() =>
            new NetworkConnection(new TcpClient(), null!));
    }

    [Fact]
    public void Constructor_NullTcpClient_ThrowsArgumentNullException()
    {
        // B2: null tcpClient guard
        Assert.Throws<ArgumentNullException>(() =>
            new NetworkConnection(null!, _logger));
    }

    [Fact]
    public void Constructor_WithPreConnectedTcpClient_IsConnected()
    {
        // B3: IsConnected == true branch — reader/writer are initialised
        var (listener, _, clientSide) = CreateConnectedPair();
        listener.Stop();

        using var conn = new NetworkConnection(clientSide, _logger);

        Assert.True(conn.IsConnected);
    }

    [Fact]
    public void Constructor_WithDisconnectedTcpClient_IsNotConnected()
    {
        // B4: IsConnected == false branch — reader/writer stay as Null
        using var conn = new NetworkConnection(new TcpClient(), _logger);

        Assert.False(conn.IsConnected);
    }

    // -----------------------------------------------------------------------
    // Constructor(ILogger) — B5
    // -----------------------------------------------------------------------

    [Fact]
    public void DefaultConstructor_CreatesDisconnectedConnection()
    {
        // B5: convenience ctor delegates to primary ctor with a fresh TcpClient
        using var conn = new NetworkConnection(_logger);

        Assert.False(conn.IsConnected);
    }

    // -----------------------------------------------------------------------
    // IsConnected — B6–B7
    // (B8 — exception path — is defensive code inside TcpClient and cannot be
    //  triggered without private reflection or a mocked socket layer)
    // -----------------------------------------------------------------------

    [Fact]
    public void IsConnected_DisconnectedClient_ReturnsFalse()
    {
        // B7: _tcpClient.Connected == false
        using var conn = new NetworkConnection(new TcpClient(), _logger);

        Assert.False(conn.IsConnected);
    }

    [Fact]
    public void IsConnected_ConnectedClient_ReturnsTrue()
    {
        // B6: _tcpClient.Connected == true
        var (listener, _, clientSide) = CreateConnectedPair();
        listener.Stop();

        using var conn = new NetworkConnection(clientSide, _logger);

        Assert.True(conn.IsConnected);
    }

    // -----------------------------------------------------------------------
    // Connect — B9–B10
    // -----------------------------------------------------------------------

    [Fact]
    public void Connect_WhenAlreadyConnected_ReturnsEarlyWithNoSideEffects()
    {
        // B9: early-return path — the connect call to a non-existent port is
        //     never reached because IsConnected gates it.
        var (listener, _, clientSide) = CreateConnectedPair();
        listener.Stop();

        using var conn = new NetworkConnection(clientSide, _logger);

        // Would throw SocketException if it actually attempted to connect.
        conn.Connect("127.0.0.1", 1);

        Assert.True(conn.IsConnected);
    }

    [Fact]
    public void Connect_WhenNotConnected_EstablishesConnection()
    {
        // B10: happy-path connect
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint!).Port;

        using var conn = new NetworkConnection(_logger);

        // Accept the incoming connection on a background thread.
        var acceptTask = Task.Run(() =>
        {
            var serverSide = listener.AcceptTcpClient();
            _disposables.Add(serverSide);
        });

        conn.Connect("127.0.0.1", port);
        acceptTask.Wait(TimeSpan.FromSeconds(5));
        listener.Stop();

        Assert.True(conn.IsConnected);
        _disposables.Add(conn);
    }

    // -----------------------------------------------------------------------
    // SendLine — B11–B12
    // -----------------------------------------------------------------------

    [Fact]
    public void SendLine_WhenNotConnected_ThrowsInvalidOperationException()
    {
        // B11: not connected — throws
        using var conn = new NetworkConnection(_logger);

        var ex = Assert.Throws<InvalidOperationException>(() => conn.SendLine("hello"));
        Assert.Equal("Not connected.", ex.Message);
    }

    [Fact]
    public void SendLine_WhenConnected_MessageArrievesOnOtherSide()
    {
        // B12: connected — writes line; verified by reading from server side
        var (listener, serverSide, clientSide) = CreateConnectedPair();
        listener.Stop();

        using var clientConn = new NetworkConnection(clientSide, _logger);
        using var serverConn = new NetworkConnection(serverSide, _logger);

        clientConn.SendLine("hello world");

        Assert.Equal("hello world", serverConn.ReceiveLine());
    }

    // -----------------------------------------------------------------------
    // ReceiveLine — B13–B15
    // -----------------------------------------------------------------------

    [Fact]
    public void ReceiveLine_WhenNotConnected_ThrowsInvalidOperationException()
    {
        // B13: not connected — throws
        using var conn = new NetworkConnection(_logger);

        var ex = Assert.Throws<InvalidOperationException>(() => conn.ReceiveLine());
        Assert.Equal("Not connected", ex.Message);
    }

    [Fact]
    public void ReceiveLine_WhenConnected_ReturnsMessage()
    {
        // B15: connected and data available — returns the line
        var (listener, serverSide, clientSide) = CreateConnectedPair();
        listener.Stop();

        using var clientConn = new NetworkConnection(clientSide, _logger);
        using var serverConn = new NetworkConnection(serverSide, _logger);

        serverConn.SendLine("test message");

        Assert.Equal("test message", clientConn.ReceiveLine());
    }

    [Fact]
    public void ReceiveLine_WhenRemoteCloses_ThrowsIOException()
    {
        // B14: ReadLine returns null (remote closed) — IOException thrown
        var (listener, serverSide, clientSide) = CreateConnectedPair();
        listener.Stop();

        using var clientConn = new NetworkConnection(clientSide, _logger);

        // Forcibly close the server side to simulate remote disconnect.
        serverSide.Close();

        var ex = Assert.Throws<IOException>(() => clientConn.ReceiveLine());
        Assert.Equal("Client closed the connection.", ex.Message);
    }

    // -----------------------------------------------------------------------
    // Disconnect — B16–B20
    // -----------------------------------------------------------------------

    [Fact]
    public void Disconnect_WhenNotConnected_DoesNotThrow()
    {
        // B16: early-return path — nothing to disconnect
        using var conn = new NetworkConnection(_logger);

        var ex = Record.Exception(() => conn.Disconnect());

        Assert.Null(ex);
    }

    [Fact]
    public void Disconnect_WhenConnected_ClientBecomesDisconnected()
    {
        // B17 + B19: connected, shutdown and dispose succeed
        var (listener, _, clientSide) = CreateConnectedPair();
        listener.Stop();

        var conn = new NetworkConnection(clientSide, _logger);
        Assert.True(conn.IsConnected);

        conn.Disconnect();

        Assert.False(conn.IsConnected);
    }

    [Fact]
    public void Disconnect_CalledTwice_DoesNotThrow()
    {
        // B16 after B17: second call hits the "not connected" early-return path
        var (listener, _, clientSide) = CreateConnectedPair();
        listener.Stop();

        var conn = new NetworkConnection(clientSide, _logger);
        conn.Disconnect();

        var ex = Record.Exception(() => conn.Disconnect());

        Assert.Null(ex);
    }

    // -----------------------------------------------------------------------
    // Dispose — B21
    // -----------------------------------------------------------------------

    [Fact]
    public void Dispose_WhenConnected_DisconnectsAndDoesNotThrow()
    {
        // B21: Dispose delegates to Disconnect
        var (listener, _, clientSide) = CreateConnectedPair();
        listener.Stop();

        var conn = new NetworkConnection(clientSide, _logger);
        Assert.True(conn.IsConnected);

        conn.Dispose();

        Assert.False(conn.IsConnected);
    }

    [Fact]
    public void Dispose_ViaUsingBlock_DisconnectsOnExit()
    {
        // B21 via language-level using statement
        var (listener, _, clientSide) = CreateConnectedPair();
        listener.Stop();

        NetworkConnection conn;
        bool wasConnectedInside;

        using (conn = new NetworkConnection(clientSide, _logger))
        {
            wasConnectedInside = conn.IsConnected;
        }

        Assert.True(wasConnectedInside);
        Assert.False(conn.IsConnected);
    }

    // -----------------------------------------------------------------------
    // Logging verification — kills Statement + String mutations on logger calls
    // -----------------------------------------------------------------------

    [Fact]
    public void Constructor_WithPreConnectedClient_LogsInitializedMessage()
    {
        var logger = new CapturingLogger();
        var (listener, _, clientSide) = CreateConnectedPair();
        listener.Stop();

        using var _ = new NetworkConnection(clientSide, logger);

        Assert.True(logger.Contains("Initialized NetworkConnection"));
    }

    [Fact]
    public void Connect_WhenAlreadyConnected_LogsAlreadyConnectedMessage()
    {
        var logger = new CapturingLogger();
        var (listener, _, clientSide) = CreateConnectedPair();
        listener.Stop();

        using var conn = new NetworkConnection(clientSide, logger);

        conn.Connect("127.0.0.1", 1); // early-return path

        Assert.True(logger.Contains("already connected"));
    }

    [Fact]
    public void Connect_WhenNotConnected_LogsConnectingAndConnectedMessages()
    {
        var logger = new CapturingLogger();
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint!).Port;

        using var conn = new NetworkConnection(logger);

        var acceptTask = Task.Run(() =>
        {
            var serverSide = listener.AcceptTcpClient();
            _disposables.Add(serverSide);
        });

        conn.Connect("127.0.0.1", port);
        acceptTask.Wait(TimeSpan.FromSeconds(5));
        listener.Stop();
        _disposables.Add(conn);

        Assert.True(logger.Contains("Connecting to"));
        Assert.True(logger.Contains("Connected to"));
    }

    [Fact]
    public void Connect_ThenSendAndReceive_MessageArrivesViaConnectPath()
    {
        // Kills AutoFlush and StreamWriter mutations in Connect()
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint!).Port;

        TcpClient? serverSide = null;
        var acceptTask = Task.Run(() =>
        {
            serverSide = listener.AcceptTcpClient();
        });

        using var clientConn = new NetworkConnection(_logger);
        clientConn.Connect("127.0.0.1", port);
        acceptTask.Wait(TimeSpan.FromSeconds(5));
        listener.Stop();

        using var serverConn = new NetworkConnection(serverSide!, _logger);
        _disposables.Add(serverSide!);

        clientConn.SendLine("via-connect-path");

        Assert.Equal("via-connect-path", serverConn.ReceiveLine());
    }

    [Fact]
    public void SendLine_WhenConnected_LogsBroadcastTrace()
    {
        var logger = new CapturingLogger();
        var (listener, serverSide, clientSide) = CreateConnectedPair();
        listener.Stop();

        using var clientConn = new NetworkConnection(clientSide, logger);
        using var serverConn = new NetworkConnection(serverSide, _logger);

        clientConn.SendLine("trace-test");

        Assert.True(logger.Contains("broadcasted"));
    }

    [Fact]
    public void SendLine_WhenNotConnected_LogsError()
    {
        var logger = new CapturingLogger();
        using var conn = new NetworkConnection(logger);

        Assert.Throws<InvalidOperationException>(() => conn.SendLine("x"));

        Assert.True(logger.Contains("not connected"));
    }

    [Fact]
    public void ReceiveLine_WhenConnected_LogsReceivedTrace()
    {
        var logger = new CapturingLogger();
        var (listener, serverSide, clientSide) = CreateConnectedPair();
        listener.Stop();

        using var clientConn = new NetworkConnection(clientSide, logger);
        using var serverConn = new NetworkConnection(serverSide, _logger);

        serverConn.SendLine("received-trace-test");
        clientConn.ReceiveLine();

        Assert.True(logger.Contains("received"));
    }

    [Fact]
    public void ReceiveLine_WhenNotConnected_LogsError()
    {
        var logger = new CapturingLogger();
        using var conn = new NetworkConnection(logger);

        Assert.Throws<InvalidOperationException>(() => conn.ReceiveLine());

        Assert.True(logger.Contains("not connected"));
    }

    [Fact]
    public void ReceiveLine_WhenRemoteCloses_LogsClientClosed()
    {
        var logger = new CapturingLogger();
        var (listener, serverSide, clientSide) = CreateConnectedPair();
        listener.Stop();

        using var clientConn = new NetworkConnection(clientSide, logger);
        serverSide.Close();

        Assert.Throws<IOException>(() => clientConn.ReceiveLine());

        Assert.True(logger.Contains("closed"));
    }

    [Fact]
    public void Disconnect_WhenNotConnected_LogsNotConnected()
    {
        var logger = new CapturingLogger();
        using var conn = new NetworkConnection(logger);

        conn.Disconnect();

        Assert.True(logger.Contains("not connected"));
    }

    [Fact]
    public void Disconnect_WhenConnected_LogsDisconnectingAndDisconnected()
    {
        var logger = new CapturingLogger();
        var (listener, _, clientSide) = CreateConnectedPair();
        listener.Stop();

        var conn = new NetworkConnection(clientSide, logger);
        conn.Disconnect();

        Assert.True(logger.Contains("Disconnecting"));
        Assert.True(logger.Contains("Socket shutdown successful"));
        Assert.True(logger.Contains("Disposing the network streams"));
        Assert.True(logger.Contains("Network streams and client disposed"));
        Assert.True(logger.Contains("Disconnected"));
    }

    [Fact]
    public void ReceiveLine_AfterDisconnect_ThrowsInvalidOperationException()
    {
        // Kills line 182 logical mutation: || -> &&
        // After Disconnect(), IsConnected=false but _reader != StreamReader.Null.
        // The || guard must still throw; && would not.
        var (listener, _, clientSide) = CreateConnectedPair();
        listener.Stop();

        var conn = new NetworkConnection(clientSide, _logger);
        conn.Disconnect();

        Assert.Throws<InvalidOperationException>(() => conn.ReceiveLine());
    }

    [Fact]
    public void SendLine_AfterDisconnect_ThrowsInvalidOperationException()
    {
        // Mirrors ReceiveLine_AfterDisconnect — also exercises the !IsConnected path
        // with a non-null writer, guarding against similar logical mutations in SendLine.
        var (listener, _, clientSide) = CreateConnectedPair();
        listener.Stop();

        var conn = new NetworkConnection(clientSide, _logger);
        conn.Disconnect();

        Assert.Throws<InvalidOperationException>(() => conn.SendLine("should-fail"));
    }

    [Fact]
    public void SendLine_WhenNotConnected_LogsExactErrorMessage()
    {
        // Kills line 141 String mutation — asserts the exact log contains the message param.
        var logger = new CapturingLogger();
        using var conn = new NetworkConnection(logger);

        Assert.Throws<InvalidOperationException>(() => conn.SendLine("payload-xyz"));

        Assert.True(logger.Contains("payload-xyz"));
    }

    [Fact]
    public void ReceiveLine_WhenNotConnected_LogsExactErrorMessage()
    {
        // Kills line 185 String mutation on the "Failed to receive" log.
        var logger = new CapturingLogger();
        using var conn = new NetworkConnection(logger);

        Assert.Throws<InvalidOperationException>(() => conn.ReceiveLine());

        Assert.True(logger.Contains("Failed to receive message"));
    }

    [Fact]
    public void ReceiveLine_WhenRemoteCloses_LogsExactClosedMessage()
    {
        // Kills line 193 String mutation on the "Client closed connection" log.
        var logger = new CapturingLogger();
        var (listener, serverSide, clientSide) = CreateConnectedPair();
        listener.Stop();

        using var clientConn = new NetworkConnection(clientSide, logger);
        serverSide.Close();

        Assert.Throws<IOException>(() => clientConn.ReceiveLine());

        Assert.True(logger.Contains("Client closed connection"));
    }

    [Fact]
    public void Connect_ThenSendAndReceive_MessageArrivesPromptly()
    {
        // Kills AutoFlush=true mutations: if AutoFlush were false the data stays
        // in the StreamWriter buffer, and ReceiveLine() would block indefinitely.
        // Using Task.Run + explicit timeout: if the message doesn't arrive within
        // 3 s the assertion fails, killing the mutant.
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint!).Port;

        TcpClient? serverSide = null;
        var acceptTask = Task.Run(() => { serverSide = listener.AcceptTcpClient(); });

        using var clientConn = new NetworkConnection(_logger);
        clientConn.Connect("127.0.0.1", port);
        acceptTask.Wait(TimeSpan.FromSeconds(5));
        listener.Stop();

        using var serverConn = new NetworkConnection(serverSide!, _logger);
        _disposables.Add(serverSide!);

        clientConn.SendLine("autoflush-check");

        var receiveTask = Task.Run(() => serverConn.ReceiveLine());
        bool arrivedInTime = receiveTask.Wait(TimeSpan.FromSeconds(3));

        Assert.True(arrivedInTime, "Message should arrive promptly (AutoFlush=true required).");
        Assert.Equal("autoflush-check", receiveTask.Result);
    }
}

/// <summary>
/// Tests for <see cref="ServerConnection.WaitForConnections"/>.
/// The server runs on a background thread; each test connects once and
/// verifies the handleConnect delegate fires with a live NetworkConnection.
/// </summary>
public sealed class ServerConnectionTests
{
    private static int FindFreePort()
    {
        var l = new TcpListener(IPAddress.Loopback, 0);
        l.Start();
        int port = ((IPEndPoint)l.LocalEndpoint!).Port;
        l.Stop();
        return port;
    }

    [Fact]
    public void WaitForConnections_InvokesHandleConnect_WithConnectedNetworkConnection()
    {
        // Happy path: server accepts a client and fires handleConnect.
        var serverLogger = new CapturingLogger();
        int port = FindFreePort();
        var tcs = new TaskCompletionSource<bool>();

        var serverThread = new Thread(() =>
        {
            ServerConnection.WaitForConnections(
                conn =>
                {
                    tcs.TrySetResult(conn.IsConnected);
                    conn.Disconnect();
                },
                port,
                serverLogger);
        })
        { IsBackground = true };

        serverThread.Start();

        // Give the listener time to start.
        Thread.Sleep(100);

        using var client = new TcpClient();
        client.Connect(IPAddress.Loopback, port);

        // Use 2 s — well within Stryker's mutant timeout so a missing handleConnect
        // call causes a fast failure rather than a Stryker timeout (which is "survived").
        bool completed = tcs.Task.Wait(TimeSpan.FromSeconds(2));
        Assert.True(completed, "handleConnect should have been called within 2 s.");
        Assert.True(tcs.Task.Result, "NetworkConnection should be connected inside handleConnect.");
        Assert.True(serverLogger.Contains("Server listening on port"));
        Assert.True(serverLogger.Contains("Client connected"));
    }

    [Fact]
    public void WaitForConnections_HandleConnectThrows_DoesNotCrashServer()
    {
        // Error path in the per-connection thread (try/catch in the thread lambda).
        var serverLogger = new CapturingLogger();
        int port = FindFreePort();
        var handlerCalledTcs = new TaskCompletionSource<bool>();

        var serverThread = new Thread(() =>
        {
            ServerConnection.WaitForConnections(
                conn =>
                {
                    handlerCalledTcs.TrySetResult(true);
                    throw new InvalidOperationException("Simulated handler failure.");
                },
                port,
                serverLogger);
        })
        { IsBackground = true };

        serverThread.Start();
        Thread.Sleep(100);

        using var client = new TcpClient();
        client.Connect(IPAddress.Loopback, port);

        bool handlerWasCalled = handlerCalledTcs.Task.Wait(TimeSpan.FromSeconds(2));
        Assert.True(handlerWasCalled, "handleConnect should have been invoked within 2 s.");

        // Give the finally block time to log "Client disconnect."
        Thread.Sleep(300);

        Assert.True(serverLogger.Contains("Error in connection handler"));
        Assert.True(serverLogger.Contains("Client disconnect"));
    }
}

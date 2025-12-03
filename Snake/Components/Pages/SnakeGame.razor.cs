// <copyright file="SnakeGame.razor.cs" company="UofU-CS3500">
// Copyright (c) 2025 UofU-CS3500. All rights reserved.
// </copyright>

using System.Text.Json;
using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;
using CS3500.LogSupport;
using CS3500.Networking;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;

namespace Snake.Components.Pages;


/// <summary>
///  The C# component of the <see cref="SnakeGame"/> client.
/// </summary>
public partial class SnakeGame : ComponentBase
{
    // Drawing/Canvas variables here.
    private BECanvasComponent   canvasReference = null!;
    private Canvas2DContext     context = null!;

    // GUI Model here:
    private int GUIWidth;
    private int GUIHeight;
    private DateTime StartTime = DateTime.Now;
    private int frameNumberGUI = 0;
    private string errorMessage = string.Empty;

    // WORLD Model here:
    private World worldModel = new();

    // Controller Model here:
    private IJSObjectReference _jsModule = null!;

    // Player data
    int playerId;
    int worldSize;

    // Network model
    private NetworkConnection server = new( NullLogger.Instance );
    private string networkStatus = string.Empty;
    private int frameNumberNetwork = 0;
    private DateTime connectTime = DateTime.Now;

    // Game state variables
    private bool playPressed = false;
    private bool gameStarted = false;

    // Player name entered by the user
    private string playerName = string.Empty;

    // Names that are not allowed (edit this list however you want)
    private static readonly string[] disallowedNames = new[] { "wall", "snake", "power" };

    // Database controller
    // TODO: Replace with connection string from secrets file
    private static readonly string ConnectionString = 
        "Server=;Database=;User Id=;Password=;TrustServerCertificate=True;";

    private readonly DatabaseController dbController = new(ConnectionString);

    /// <summary>
    ///   First step in the Blazor Page Life Cycle.  In some circumstances
    ///   you would load data here.  We do not need to.
    /// </summary>
    protected override void OnInitialized( )
    {
        Logger.LogDetailsBrief( LogLevel.Debug, "OnInitialized" );
    }

     /// <summary>
     ///   The first time we start up, we load our JavaScript and start the animation
     ///   process.
     /// </summary>
     /// <param name="firstRender"> Whether the first render has taken place or not. </param>
     /// <returns>Task that creates the canvas.</returns>
    protected override async Task OnAfterRenderAsync( bool firstRender )
     {
         if ( firstRender )
         {
             Logger.LogDetailsBrief( LogLevel.Debug, "First Render" );

             _jsModule = await JsRuntime.InvokeAsync<IJSObjectReference>( "import", "./Components/Pages/SnakeGame.razor.js" );
             context = await canvasReference.CreateCanvas2DAsync();

             await _jsModule.InvokeVoidAsync( "initJS", DotNetObjectReference.Create( this ) );
             await _jsModule.InvokeVoidAsync( "ToggleAnimation", true );
         }
     }

    private async void Connect()
    {
    // Basic validation before we even try to connect
    if (string.IsNullOrWhiteSpace(playerName))
    {
        errorMessage = "Please enter a name before playing.";
        networkStatus = string.Empty;
        playPressed = false;
        await InvokeAsync(StateHasChanged);
        return;
    }

    // Trim + enforce max length on the C# side too
    var chosenName = playerName.Trim();
    if (chosenName.Length > 16)
    {
        chosenName = chosenName[..16];
    }

    // Check against disallowed names (case-insensitive)
    foreach (var banned in disallowedNames)
    {
        if (string.Equals(banned, chosenName, StringComparison.OrdinalIgnoreCase))
        {
            errorMessage = "That name is not allowed. Please pick a different one.";
            networkStatus = string.Empty;
            playPressed = false;
            await InvokeAsync(StateHasChanged);
            return;
        }
    }

    playPressed = true;
    Logger.LogInformation("Connecting!");

    await Task.Run(() =>
    {
        errorMessage = string.Empty;
        networkStatus = "Connecting...";
        InvokeAsync(StateHasChanged);

        try
        {
            server.Connect("localhost", 11000);
            networkStatus = "Connected";
            connectTime = DateTime.Now;
        }
        catch (Exception e)
        {
            errorMessage = e.Message;
            networkStatus = "Couldn't connect! Please refresh the page and try again.";
            playPressed = false;
            InvokeAsync(StateHasChanged);
            return;
        }

        // Make startup info disappear
        networkStatus = string.Empty;
        gameStarted = true;
        InvokeAsync(StateHasChanged);

        // Send player name to server (use user's input instead of "Timothy")
        server.SendLine(chosenName);

        // Receive player ID and world size
        playerId = int.Parse(server.ReceiveLine());
        worldSize = int.Parse(server.ReceiveLine());

        
        // TODO
        // Database: Start a new game session
        // This is when the client first connects, so we create a game entry
        try
        {
            int gameId = dbController.StartNewGame();
            Logger.LogInformation($"Started new game session with ID: {gameId}");
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"Failed to record game start in database: {ex.Message}");
            // Don't crash the game if DB fails - just log and continue
        }

        // Receive walls (until non-wall is received)
        while (server.IsConnected)
        {
            string nextLine = server.ReceiveLine();
            if (nextLine.Contains("wall"))
            {
                Wall? wall = JsonSerializer.Deserialize<Wall>(nextLine);
                lock (worldModel)
                {
                    if (wall != null)
                    {
                        worldModel.Walls[wall.Id] = wall;
                    }
                }

                Logger.LogTrace($"Received wall object {wall?.Id}");
            }
            else
            {
                GameLoopIteration(nextLine);
                break;
            }
        }

        // Main receive loop
        while (server.IsConnected)
        {
            try
            {
                string nextLine = server.ReceiveLine();
                GameLoopIteration(nextLine);
            }
            catch (Exception e)
            {
                Logger.LogWarning("Exception caught in receive loop: {1}", e.Message);
            }

        }

        // Clean up after loop exits
        errorMessage = "Disconnected!";
        gameStarted = false;
        playPressed = false;
        InvokeAsync(StateHasChanged);
    });
}

    /// <summary>
    /// Adds data (that is not a wall) to the world model and updates the database with player data.
    /// </summary>
    /// <param name="line"> A JSON object received from the server. Can be a snake or power-up.</param>
    private void GameLoopIteration(string line)
    {
        if (line.Contains("snake"))
        {
            Snake? snake = JsonSerializer.Deserialize<Snake>(line);
            lock (worldModel)
            {
                if (snake != null)
                {
                    worldModel.Snakes[snake.Id] = snake;

                    // TODO
                    // Database: Record or update player
                    // Check if snake has been seen before, if not, add new row in to players table
                    // If snake has been seen before, check if score is max, if so, update max score in players table
                    // If "dc" property is true, update leave time in players table
                    try
                    {
                        // Check if snake disconnected
                        if (snake.Disconnected)
                        {
                            worldModel.Snakes.Remove(snake.Id);
                            dbController.RecordPlayerDisconnect(snake.Id);
                            Logger.LogTrace($"Recorded disconnect for snake {snake.Id}");
                        }
                        else
                        {
                            // Record or update the player (handles both new and existing)
                            dbController.RecordOrUpdatePlayer(snake.Id, snake.Name, snake.Score);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning($"Failed to update player in database: {ex.Message}");
                        // Don't crash - just log and continue
                    }
                }
            }

            if (snake != null)
            {
                Logger.LogTrace($"Received snake object {snake.Id}");
            }
        }

        if (line.Contains("power"))
        {
            PowerUp? powerUp = JsonSerializer.Deserialize<PowerUp>(line);
            lock (worldModel)
            {
                if (powerUp != null)
                {
                    worldModel.PowerUps[powerUp.Id] = powerUp;
                }

                if (powerUp != null)
                {
                    Logger.LogTrace($"Received powerup object {powerUp.Id}");
                }
            }
        }
    }

    /// <summary>
    ///   Draw the world
    /// </summary>
    /// <param name="timeStamp">
    ///   Tells you how many milliseconds have
    ///   elapsed sense the web page was loaded.
    /// </param>
    [JSInvokable]
    public async void Draw( double timeStamp = 0 )
    {
        frameNumberGUI++;
        double fps = frameNumberGUI / (DateTime.Now - StartTime).TotalSeconds;

        if (server.IsConnected)
        {
            try
            {
                // Group all the draw commands into one large single draw.
                await context.BeginBatchAsync();

                // Clear canvas and start with drawing the outside world
                await context.SaveAsync();
                await context.SetTransformAsync(1, 0, 0, 1, 0, 0); // Reset transform to identity
                await context.SetFillStyleAsync( "#3a0647" ); // Game background color, also found in nav bar
                await context.FillRectAsync( 0, 0, GUIWidth, GUIHeight );
                await context.RestoreAsync();

                World worldCopy;
                Point2D? head = null;
                lock (worldModel)
                {
                    worldCopy = worldModel.Clone();
                }

                if (worldCopy.Snakes.TryGetValue(playerId, out var playerSnake) && playerSnake.Body.Any())
                {
                    head = playerSnake.Head;
                }

                // Because we are modifying the transformation matrix, we need to save it so we can restore it at the end
                await context.SaveAsync();

                // clip the view so that objects drawn outside the canvas will not be shown
                await context.BeginPathAsync();
                await context.RectAsync( 0, 0, GUIWidth, GUIHeight );
                await context.ClipAsync();

                if (head != null)
                {
                    // Center on player's head
                    await context.TranslateAsync(GUIWidth / 2.0, GUIHeight / 2.0);
                    await context.TranslateAsync(-head.X, -head.Y);
                }
                else
                {
                    // If we don't have a snake, just center the view on the origin
                    await context.TranslateAsync(GUIWidth / 2.0, GUIHeight / 2.0);
                }

                // Draw background
                await context.SetFillStyleAsync( "#0c0c1a" );

                // ReSharper disable PossibleLossOfFraction
                await context.FillRectAsync(-worldSize / 2, -worldSize / 2, worldSize, worldSize);

                // Draw all game objects (thread-safe)
                lock (worldModel)
                {
                    foreach (var wall in worldCopy.Walls.Values)
                    {
                        DrawWall(wall);
                    }

                    foreach (var powerup in worldCopy.PowerUps.Values)
                    {
                        DrawPowerUp(powerup);
                    }

                    foreach (var snake in worldCopy.Snakes.Values)
                    {
                        DrawSnake(snake);
                    }
                }

                await context.RestoreAsync();

                // Draw Heads-Up Display (no transformation - screen coordinates)
                await DrawHud(fps);

                // Draw game monitor border
                await context.SetShadowBlurAsync(20);
                await context.SetShadowColorAsync("#00ff00");
                await context.SetStrokeStyleAsync("#00ff00");
                await context.SetLineWidthAsync(4);
                await context.StrokeRectAsync(0, 0, GUIWidth, GUIHeight);
                await context.SetShadowBlurAsync(0); // reset after

                await context.EndBatchAsync();
            }
            catch ( Exception e )
            {
                Logger.LogDetailsBrief( LogLevel.Debug, $"Error drawing: {e}" );
            }
        }
    }

    /// <summary>
    /// Draws a snake on the canvas.
    /// </summary>
    /// <param name="snake"> The <see cref="Snake"/> object to draw. </param>
    async Task DrawSnake(Snake snake)
    {
        if (!snake.Alive)
        {
            return;
        }

        Point2D firstPoint;

        if (snake.Id != playerId)
        {
            if (!snake.Alive)
            {
                return;
            }

            // Glow ON
            await context.SetShadowBlurAsync(20);
            await context.SetShadowColorAsync("red");
            await context.SetStrokeStyleAsync("red");
            await context.SetLineWidthAsync(10);

            await context.BeginPathAsync();

            firstPoint = snake.Body[0];
            await context.MoveToAsync(firstPoint.X, firstPoint.Y);

            foreach (Point2D segment in snake.Body)
            {
                await context.LineToAsync(segment.X, segment.Y);
            }

            await context.StrokeAsync();
        }
        else
        {
            // Glow ON
            await context.SetShadowBlurAsync(20);
            await context.SetShadowColorAsync("#00ff00");
            await context.SetStrokeStyleAsync("#00ff00");
            await context.SetLineWidthAsync(10);

            await context.BeginPathAsync();

            firstPoint = snake.Body[0];
            await context.MoveToAsync(firstPoint.X, firstPoint.Y);

            foreach (Point2D segment in snake.Body)
            {
                await context.LineToAsync(segment.X, segment.Y);
            }

            await context.StrokeAsync();
        }

        // Draw the snake's name above its head
        Point2D head = snake.Body[snake.Body.Count - 1];
        await context.SetFontAsync("14px Courier");
        await context.SetFillStyleAsync("white");
        await context.FillTextAsync(snake.Name, head.X - 20, head.Y - 15);

        // Draw the score near the name (for player)
        if (snake.Id == playerId)
        {
            await context.SetFontAsync("12px Courier");
            await context.FillTextAsync($"Score: {snake.Score}", head.X - 20, head.Y - 30); 
        }

        // Glow OFF (reset)
        await context.SetShadowBlurAsync(0);
        await context.SetShadowColorAsync("transparent");

        Logger.LogTrace($"Drew snake with ID: {snake.Id}");
    }

    /// <summary>
    /// Draws a wall on the canvas
    /// </summary>
    /// <param name="wall">The <see cref="Wall"/> object to draw.</param>
    async Task DrawWall(Wall wall)
    {
        // Walls are just thick lines from p1 to p2
        await context.SetStrokeStyleAsync("#3a0647");  // Purple (same color as bg)
        await context.SetLineWidthAsync(50);           // 50 pixels thick
        await context.SetLineCapAsync(LineCap.Square); // Square ends

        await context.BeginPathAsync();
        await context.MoveToAsync(wall.P1.X, wall.P1.Y);
        await context.LineToAsync(wall.P2.X, wall.P2.Y);
        await context.StrokeAsync();
    }

    /// <summary>
    /// Draws a power-up on the canvas
    /// </summary>
    /// <param name="powerup"> The <see cref="PowerUp"/> object to draw.</param>
    async Task DrawPowerUp(PowerUp powerup)
    {
        // Don't draw dead powerups
        if (powerup.Died)
        {
            return;
        }

        // Draw powerups
        await context.SetFillStyleAsync("gold");
        Point2D drawPoint = powerup.GetDrawingPoint();
        await context.FillRectAsync(drawPoint.X, drawPoint.Y, 16, 16);
    }

    /// <summary>
    /// Draws the heads-up display showing scores and info
    /// </summary>
    private async Task DrawHud(double fps)
    {
        await context.SetFontAsync("20px Courier");
        await context.SetFillStyleAsync("white");

        // Show FPS
        if (gameStarted)
        {
            lock (worldModel)
            {
                context.FillTextAsync($"Players: {worldModel.Snakes.Count} - FPS: {fps:F1}", 10, 25);
            }
        }

        // Show player list and scores
        int yOffset = 50;
        await context.SetFontAsync("16px Courier");

        int scoreOrdering = 1;
        int i = 0;
        lock (worldModel)
        {
            foreach (var snake in worldModel.Snakes.Values.OrderByDescending(s => s.Score))
            {
                if (i < 8)
                {
                    context.SetFillStyleAsync("white");
                    context.FillTextAsync($"{scoreOrdering++}.", 10, yOffset);
                    context.FillTextAsync($"{snake.Name}: {snake.Score}", 35, yOffset);
                }

                i++;
                yOffset += 20;
            }
        }
    }

    /// <summary>
    ///   <para>
    ///     This method is called from the JavaScript side of the
    ///     browser.
    ///   </para>
    ///   <remarks>
    ///      Must be PUBLIC for JavaScript to call!
    ///   </remarks>
    /// </summary>
    /// <param name="width">The width to resize as.</param>
    /// <param name="height">The height to resize as.</param>
    [JSInvokable]
    public void ResizeInBlazor( int width, int height )
    {
        Logger.LogTrace( "Resizing the web page. {width} {height}", width, height );
        GUIWidth = Math.Min(Math.Max(100, width),1000);
        GUIHeight = Math.Min(Math.Max(100, height),1000);
    }

    /// <summary>
    /// Takes key presses from JS, sends them as packets to the server.
    /// </summary>
    /// <param name="key"> The key that was pressed. Follows JS standards. </param>
    [JSInvokable]
    public void HandleKeyPress(string key)
    {
        if (key == "Escape")
        {
            HandleDisconnect();
        }

        string? direction = key switch
        {
            "w" or "ArrowUp" => "up",
            "s" or "ArrowDown" => "down",
            "a" or "ArrowLeft" => "left",
            "d" or "ArrowRight" => "right",
            _ => null,
        };

        if (direction != null)
        {
            var cmd = new { moving = direction };
            string json = JsonSerializer.Serialize(cmd);
            server.SendLine(json);
        }
    }

    /// <summary>
    /// Handles disconnecting. Safely clears the world model and resets the game state variables.
    /// </summary>
    private void HandleDisconnect()
    {
        if (server.IsConnected)
        {
            server.Disconnect();
        }

        // reset game state
        gameStarted = false;
        playPressed = false;

        // clear the world
        lock (worldModel)
        {
            worldModel = new World();
        }

        // create a new connection object for next time
        server = new NetworkConnection(this.Logger);

        // Update the UI
        InvokeAsync(StateHasChanged);

        // TODO: When player disconnects, update the ending time in the games table entry
    }

    /// <summary>
    ///   Called by the system when the page is navigated away from.
    ///   Note: sometimes the debugger will create a "dummy" page when
    ///   first navigated to, then dispose it, then create the actual page.
    ///   You can ignore this behavior.
    /// </summary>
    public void Dispose( )
    {
        HandleDisconnect();

        _jsModule.InvokeVoidAsync( "ToggleAnimation", false );
        Logger.LogDetailsBrief( LogLevel.Debug, "Dispose" );
    }
}

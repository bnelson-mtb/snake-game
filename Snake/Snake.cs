// <copyright file="Snake.cs" company="UofU-CS3500">
// Copyright (c) 2025 UofU-CS3500. All rights reserved.
// </copyright>

using System.Drawing;
using System.Text.Json.Serialization;

namespace Snake;

/// <summary>
/// Represents a snake.
/// </summary>
[JsonSerializable(typeof(Snake))]
public class Snake
{
    /// <summary>
    /// Gets or sets the snake's unique identifier.
    /// </summary>
    [JsonPropertyName("snake")]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the location of each piece of the snake's body.
    /// </summary>
    [JsonPropertyName("body")]
    public List<Point2D> Body { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the snake has died.
    /// </summary>
    [JsonPropertyName("died")]
    public bool Died { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the snake is alive.
    /// </summary>
    [JsonPropertyName("alive")]
    public bool Alive { get; set; }

    /// <summary>
    /// Gets or sets the snake's name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the snake's score.
    /// </summary>
    [JsonPropertyName("score")]
    public int Score { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the snake has disconnected from the server.
    /// </summary>
    [JsonPropertyName("dc")]
    public bool Disconnected { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the snake has joined the server.
    /// </summary>
    [JsonPropertyName("join")]
    public bool Joined { get; set; }

    public IList<Rectangle> Rectangles
    {
        get
        {
            int thickness = 10;
            var rectangles = new List<Rectangle>();

            for (int i = 0; i < Body.Count - 1; i++)
            {
                var p1 = Body[i];
                var p2 = Body[i + 1];

                // Vertical segment
                if (p1.X == p2.X)
                {
                    int y = Math.Min(p1.Y, p2.Y);
                    int height = Math.Abs(p1.Y - p2.Y);
                    rectangles.Add(new Rectangle(p1.X - (thickness / 2), y, thickness, height));
                }

                // Horizontal segment
                else if (p1.Y == p2.Y)
                {
                    int x = Math.Min(p1.X, p2.X);
                    int width = Math.Abs(p1.X - p2.X);
                    rectangles.Add(new Rectangle(x, p1.Y - (thickness / 2), width, thickness));
                }
            }

            return rectangles;
        }
    }

    /// <summary>
    /// Gets the "Head" of the snake.
    /// </summary>
    public Point2D Head
    {
        get
        {
            return Body.Last();
        }
    }
}

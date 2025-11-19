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
            IList<Rectangle> rectangles = new List<Rectangle>();
            Point2D? lastPoint = null;
            foreach (Point2D point in Body)
            {
                if (lastPoint == null)
                {
                    lastPoint = point;
                }
                else
                {
                    // X caculation
                    int x = (lastPoint.X + point.X) / 2;

                    // Y calculation
                    int y = (lastPoint.Y + point.Y) / 2;

                    // width calculation
                    int calculateWidth = Math.Max(lastPoint.X - point.X, point.X - lastPoint.X);
                    int width = calculateWidth == 0 ? 10 : calculateWidth;

                    // height calculation
                    int calculateHeight = Math.Max(lastPoint.Y - point.Y, point.Y - lastPoint.Y);
                    int height = calculateHeight == 0 ? 10 : calculateHeight;
                    rectangles.Add(new Rectangle(x, y, width, height));
                }
            }

            return rectangles;
        }
    }

    public Point2D Head
    {
        get
        {
            return Body[ 0 ];
        }
    }
}

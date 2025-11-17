// <copyright file="Wall.cs" company="UofU-CS3500">
// Copyright (c) 2025 UofU-CS3500. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace Snake;

/// <summary>
/// Represents a wall, defined by two endpoints.
/// </summary>
[JsonSerializable(typeof(Wall))]
public class Wall
{
    /// <summary>
    /// Gets or sets the unique identifier for the wall.
    /// </summary>
    [JsonPropertyName("wall")]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the first endpoint of the wall.
    /// </summary>
    [JsonPropertyName("p1")]
    public Point2D P1 { get; set; }

    /// <summary>
    /// Gets or sets the second endpoint of the wall.
    /// </summary>
    [JsonPropertyName("p2")]
    public Point2D P2 { get; set; }

    /// <summary>
    /// Gets represents the width of the wall.
    /// </summary>
    public int Width => P2.X - P1.X == 0 ? 50 : Math.Max(P1.X - P2.X, P2.X - P1.X);

    /// <summary>
    /// Gets represents the height of the wall
    /// </summary>
    public int Height => P2.Y - P1.Y == 0 ? 50 : Math.Max(P2.Y - P1.Y, P1.Y - P2.Y);

    /// <summary>
    /// Gets the X value of the center of the wall.
    /// </summary>
    public int X => (P2.X - P1.X) / 2;

    /// <summary>
    /// Gets the Y value of the center of the wall.
    /// </summary>
    public int Y => (P2.Y - P1.Y) / 2;
}

// TODO: Check to see if at least one of the points is correct

// {"wall":1,"p1":{"X":-575,"Y":-575},"p2":{"X":-575,"Y":575}}

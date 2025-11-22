// <copyright file="Wall.cs" company="UofU-CS3500">
// Copyright (c) 2025 UofU-CS3500. All rights reserved.
// </copyright>

using System.Drawing;
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
}

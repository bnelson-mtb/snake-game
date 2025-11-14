// <copyright file="Wall.cs" company="UofU-CS3500">
// Copyright (c) 2025 UofU-CS3500. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace Snake;

/// <summary>
/// Represents a wall, defined by two endpoints.
/// </summary>
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

// TODO: Check to see if at least one of the points is correct

// {"wall":1,"p1":{"X":-575,"Y":-575},"p2":{"X":-575,"Y":575}}

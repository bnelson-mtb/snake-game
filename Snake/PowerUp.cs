// <copyright file="PowerUp.cs" company="UofU-CS3500">
// Copyright (c) 2025 UofU-CS3500. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace Snake;

/// <summary>
/// Represents a power-up item.
/// </summary>
public class PowerUp
{
    /// <summary>
    /// Gets or sets the unique identifier of the power-up.
    /// </summary>
    [JsonPropertyName("power")]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the location of the power-up.
    /// </summary>
    [JsonPropertyName("loc")]
    public Point2D Location { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the power-up has been consumed or removed.
    /// </summary>
    [JsonPropertyName("died")]
    public bool Died { get; set; }
}

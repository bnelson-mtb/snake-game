// <copyright file="PowerUp.cs" company="UofU-CS3500">
// Copyright (c) 2025 UofU-CS3500. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace Snake;

/// <summary>
/// Represents a power-up item.
/// </summary>
[JsonSerializable(typeof(PowerUp))]
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

    /// <summary>
    /// Gives a point opposite of the "Location" in order to normalize it for <see cref="Blazor.Extensions.Canvas"/>
    /// drawings in order to draw a shape where the "location" is the center of the shape drawn.
    /// </summary>
    /// <returns> A point -8, +8 points away from <see cref="Location"/>. </returns>
    public Point2D GetDrawingPoint()
    {
        return new Point2D(this.Location.X-8, this.Location.Y-8);
    }
}

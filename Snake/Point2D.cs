// <copyright file="Point2D.cs" company="UofU-CS3500">
// Copyright (c) 2025 UofU-CS3500. All rights reserved.
// </copyright>

namespace Snake;

/// <summary>
/// Represents a two-dimensional point with X and Y cooerdinates.
/// </summary>
public class Point2D
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Point2D"/> class.
    /// Constructs a new <see cref="Point2D"/> from an <see cref="X"/> and <see cref="Y"/> variable.
    /// </summary>
    /// <param name="x">The X coordinate. </param>
    /// <param name="y">The Y coordinate. </param>
    public Point2D(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }

    /// <summary>
    /// Gets or sets the horizontal (X) coordinate of the point.
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// Gets or sets the vertical (Y) coordinate of the point.
    /// </summary>
    public int Y { get; set; }
}

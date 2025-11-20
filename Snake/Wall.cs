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

    [JsonIgnore]
    public IList<Rectangle> Rectangles
    {
        get
        {
            int segmentSize = 50;
            var wallRectangles = new List<Rectangle>();

            bool isHorizontal = P1.Y == P2.Y;

            if (P1.Equals(P2))
            {
                 wallRectangles.Add(new Rectangle(P1.X - segmentSize / 2, P1.Y - segmentSize / 2, segmentSize, segmentSize));
                 return wallRectangles;
            }

            if (isHorizontal)
            {
                int startX = Math.Min(P1.X, P2.X);
                int endX = Math.Max(P1.X, P2.X);
                for (int x = startX; x <= endX; x += segmentSize)
                {
                    wallRectangles.Add(new Rectangle(x - segmentSize / 2, P1.Y - segmentSize / 2, segmentSize, segmentSize));
                }
            }
            else
            {
                int startY = Math.Min(P1.Y, P2.Y);
                int endY = Math.Max(P1.Y, P2.Y);
                for (int y = startY; y <= endY; y += segmentSize)
                {
                    wallRectangles.Add(new Rectangle(P1.X - segmentSize / 2, y - segmentSize / 2, segmentSize, segmentSize));
                }
            }
            return wallRectangles;
        }
    }
}

// TODO: Check to see if at least one of the points is correct

// {"wall":1,"p1":{"X":-575,"Y":-575},"p2":{"X":-575,"Y":575}}

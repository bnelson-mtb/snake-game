// <copyright file="World.cs" company="UofU-CS3500">
// Copyright (c) 2025 UofU-CS3500. All rights reserved.
// </copyright>

namespace Snake;

/// <summary>
/// This class represents the entire world of the game, holding the data for all the objects in the world.
/// </summary>
public class World
{
    /// <summary>
    /// Dict containing all the snakes in the world, with their unique IDs as keys.
    /// </summary>
    private Dictionary<int, Snake> snakes;

    /// <summary>
    /// Dict containing all the walls in the world, with their unique IDs as keys.
    /// </summary>
    private Dictionary<int, Wall> walls;

    /// <summary>
    /// Dict containing all the powerUps in the world, with their unique IDs as keys.
    /// </summary>
    private Dictionary<int, PowerUp> powerUps;

    /// <summary>
    /// Initializes a new instance of the <see cref="World"/> class.
    /// Creates a world object.
    /// </summary>
    public World()
    {
        this.snakes = new Dictionary<int, Snake>();
        this.walls = new Dictionary<int, Wall>();
        this.powerUps = new Dictionary<int, PowerUp>();
    }
}

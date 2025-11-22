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
    public Dictionary<int, Snake> Snakes;

    /// <summary>
    /// Dict containing all the walls in the world, with their unique IDs as keys.
    /// </summary>
    public Dictionary<int, Wall> Walls;

    /// <summary>
    /// Dict containing all the powerUps in the world, with their unique IDs as keys.
    /// </summary>
    public Dictionary<int, PowerUp> PowerUps;

    /// <summary>
    /// Initializes a new instance of the <see cref="World"/> class.
    /// Creates a world object.
    /// </summary>
    public World()
    {
        this.Snakes = new Dictionary<int, Snake>();
        this.Walls = new Dictionary<int, Wall>();
        this.PowerUps = new Dictionary<int, PowerUp>();
    }

    /// <summary>
    /// Clone method used to create a copy of this world.
    /// </summary>
    /// <returns> An exact copy of this world. </returns>
    public World Clone()
    {
        World output = new();
        output.Walls = new Dictionary<int, Wall>(this.Walls);
        output.Snakes = new Dictionary<int, Snake>(this.Snakes);
        output.PowerUps = new Dictionary<int, PowerUp>(this.PowerUps);
        return output;
    }
}

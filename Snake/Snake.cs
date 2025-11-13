// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json.Serialization;

namespace Snake;

public class Snake
{
    [JsonPropertyName("snake")]
    public int id { get; set; }

    [JsonPropertyName("body")]
    public List<Point2D> body { get; set; }

    [JsonPropertyName("died")]
    public bool died { get; set; }

    [JsonPropertyName("name")]
    public string name { get; set; }

    [JsonPropertyName("score")]
    public int score { get; set; }

    [JsonPropertyName("dc")]
    public bool dc { get; set; }

    [JsonPropertyName("join")]
    public bool join { get; set; }
}


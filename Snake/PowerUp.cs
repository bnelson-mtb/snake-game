// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json.Serialization;

namespace Snake;

public class PowerUp
{
    [JsonPropertyName("power")]
    public int power { get; set; }

    [JsonPropertyName("loc")]
    public Point2D location { get; set; }

    [JsonPropertyName("died")]
    public bool died { get; set; }
}

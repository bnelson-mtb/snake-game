// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json.Serialization;

namespace Snake;

public class Wall
{
    [JsonPropertyName("wall")]
    public int wall { get; set; }

    [JsonPropertyName("p1")]
    public Point2D p1 { get; set; }

    [JsonPropertyName("p2")]
    public Point2D p2 { get; set; }
}

// TODO: Check to see if at least one of the points is correct

// {"wall":1,"p1":{"X":-575,"Y":-575},"p2":{"X":-575,"Y":575}}


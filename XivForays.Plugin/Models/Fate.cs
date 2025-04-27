using System;
using System.Numerics;
using System.Text.Json.Serialization;

namespace XivMate.DataGathering.Forays.Dalamud.Models;

public class Fate
{
    public uint FateId { get; set; }
    public string Name { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    public float Radius { get; set; }
    public int StartedAt { get; set; }
    public long EndedAt { get; set; }
    public Guid InstanceId { get; set; }
    public int TerritoryId { get; set; }
    public int MapId { get; set; }
    public int LevelId { get; set; }}

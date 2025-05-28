using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text.Json.Serialization;

namespace XivMate.DataGathering.Forays.Dalamud.Models;

public class Fate
{
    public uint FateId { get; set; }
    public string Name { get; set; } = string.Empty;
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    public float Radius { get; set; }
    public int StartedAt { get; set; }
    public long EndedAt { get; set; }
    public Guid InstanceId { get; set; }
    public int TerritoryId { get; set; }
    public int MapId { get; set; }
    public int LevelId { get; set; }
    public FateReward? Reward { get; set; }
}

public class FateReward
{
    public string FateName { get; set; } = string.Empty;
    public uint Experience { get; set; }
    public uint Tomestones { get; set; }
    public FateCompletionLevel CompletionLevel { get; set; }
    public bool WasSuccessful { get; set; }
    public List<FateRewardItem> Items { get; set; } = new();
    public DateTime ReceivedAt { get; set; }
}

public class FateRewardItem
{
    public uint ItemId { get; set; }
    public uint Quantity { get; set; }
    public string ItemName { get; set; } = string.Empty;
}

public enum FateCompletionLevel
{
    None = 0,
    Bronze = 1,
    Silver = 2,
    Gold = 3
}

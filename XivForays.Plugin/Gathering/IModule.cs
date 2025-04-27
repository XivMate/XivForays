using System;

namespace XivMate.DataGathering.Forays.Dalamud.Gathering;

public interface IModule : IDisposable
{
    bool Enabled { get; }
    void LoadConfig(Configuration.Configuration configuration);
}

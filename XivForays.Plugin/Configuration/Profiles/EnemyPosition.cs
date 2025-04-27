namespace XivMate.DataGathering.Forays.Dalamud.Configuration.Profiles;

public class EnemyPositionProfile : AutoMapper.Profile
{
    public EnemyPositionProfile()
    {
        CreateMap<Models.EnemyPosition, Models.EnemyPosition>();
    }
}

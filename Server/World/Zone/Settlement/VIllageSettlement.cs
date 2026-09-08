
namespace Server.World.Zone.Settlement;

public class VillageSettlement : BaseSettlement
{
    
    public VillageSettlement(uint id, string name, BaseSettlement parentSettlement) : base(id, name)
    {
        SettlementType = Types.Village;
        ParentSettlement = parentSettlement;
    }

}
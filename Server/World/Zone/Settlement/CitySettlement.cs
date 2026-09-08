namespace Server.World.Zone.Settlement;

public class CitySettlement : BaseSettlement
{
    
    public CitySettlement(uint id, string name, BaseSettlement parentSettlement) : base(id, name)
    {
        SettlementType = Types.City;
        ParentSettlement = parentSettlement;
    }

}
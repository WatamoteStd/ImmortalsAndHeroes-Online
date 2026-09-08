namespace Server.World.Zone.Settlement;

public class CapitalSettlement : BaseSettlement
{
    
    public CapitalSettlement(uint id, string name) : base(id, name)
    {
        SettlementType = Types.Capital;
    }

}
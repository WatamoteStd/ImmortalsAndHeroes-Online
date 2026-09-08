
namespace Server.World.Zone.Settlement;

public class BaseSettlement
{
    
    public enum Types : byte
    {
        Village, City, Capital
    }
    public Types SettlementType;
    public uint Id {get; protected set;}
    public string Name {get; protected set;} = string.Empty;
    public ulong Silver {get; protected set;}
    public uint OwnerGuildId {get; protected set;}
    public bool IsControlledNPC {get; protected set;} = true;
    public float BaseTax {get; protected set;}
    public BaseSettlement? ParentSettlement {get; protected set;} = null;


    public BaseSettlement(uint id, string name)
    {
        
        Id = id;
        Name = name;

    }



    public void AddSilver(ulong amount) => Silver += amount;
    public bool TryRemoveSilver(ulong amount)
    {
        
        if (amount > Silver) return false;
        Silver -= amount;
        return true;

    }


}
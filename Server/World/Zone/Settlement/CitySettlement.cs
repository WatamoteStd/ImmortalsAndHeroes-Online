using Server.World.Zone.Settlement.Options;
using Shared.Udp.Packets.Category.Settlement;

namespace Server.World.Zone.Settlement;

public class CitySettlement : BaseSettlement
{

    public RoyalContract? royalContract {get; private set;}
    
    public CitySettlement(uint id, string name, BaseSettlement parentSettlement) : base(id, name)
    {
        SettlementType = Types.City;
        ParentSettlement = parentSettlement;
    }


    public void CreateRoyalContract(C2S_RoyalContractCreateRequestPacket contractPacket, PlayerEntity player)
    {
        
        if (royalContract != null) return;

        royalContract = new RoyalContract()
        {
            Id = (uint)Random.Shared.Next(), IsTaxFree = contractPacket.TaxFree,
            ItemId = contractPacket.ItemId, PerPlayerLimitEnabled = contractPacket.PerPlayerLimit, PerPlayerLimitCount = contractPacket.PerPlayerLimitCount,
            TotalCount = contractPacket.TotalCount, PricePerUnit = contractPacket.PricePerUnit, TaxPercent = contractPacket.TaxPercent,
            CreatedTime = DateTime.UtcNow
        };

        Console.WriteLine($"City:{Name}: Created new royal contract! Information below ->");
        Console.WriteLine($"Id:{royalContract.Id}");
        Console.WriteLine($"Item:{royalContract.ItemId.ToString()}");
        Console.WriteLine($"Count:{royalContract.TotalCount}");
        Console.WriteLine($"Price per unit:{royalContract.PricePerUnit}");
        Console.WriteLine($"Created At:{royalContract.CreatedTime}");

    }


}
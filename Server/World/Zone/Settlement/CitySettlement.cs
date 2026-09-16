using Server.World.Zone.Settlement.Options;
using Shared.Udp.Packets.Category.Settlement;
using Server.World.Zone.Settlement.Components;
using Shared.Udp.Packets.Category.Market;
using Server.Network;

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

        ulong totalCost;
        try
        {
            checked
            {
                totalCost = (ulong)contractPacket.TotalCount * (ulong)contractPacket.PricePerUnit;
            }
        }
        catch(OverflowException)
        {
            Console.WriteLine($"[City:{Name}] RoyalContract creation failed: total cost overflow.");
            return;
        }
        bool isSus = TryRemoveSilver(totalCost, player, TreasuryActionType.ContractRecerve);

        if (!isSus)
        {
            Console.WriteLine($"[City:{Name}] RoyalContract request denied. Not enought silver.");
            return;
        }

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


    public void GetRoyalContractInfo(uint userId) // USER ID NOT PLAYER (!ENTITY ID) 
    {

        var packet = new S2C_RoyalContractInfoResponsePacket
        {
            ItemId = Shared.Items.ItemType.None,
            PricePerUnit = 0,
            TaxPercent = 0f,
            Count = 0,
            PerPlayerLimitCount = 0
        };
        
        if (royalContract != null && !royalContract.IsCompleted)
        {
            
            packet.ItemId = royalContract.ItemId;
            packet.PricePerUnit = royalContract.PricePerUnit;
            packet.TaxPercent = royalContract.TaxPercent;
            packet.Count = (royalContract.TotalCount - royalContract.DeliveredCount);
            packet.PerPlayerLimitCount = royalContract.PerPlayerLimitCount;

        }

        OnRoyalContractResponseHandler(packet, userId);
        

    }

}
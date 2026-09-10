
using System.Runtime.InteropServices;
using Shared.Items;

namespace Server.World.Zone.Settlement.Options;

public class RoyalContract
{
    
    public required uint Id {get; init;}
    public required ItemType ItemId {get; init;}

    public required uint TotalCount {get; init;}
    public uint DeliveredCount {get; set;}
    public required ulong PricePerUnit {get; init;}
    public required bool IsTaxFree {get; init;} = false;
    public required float TaxPercent {get; init;}

    public required bool PerPlayerLimitEnabled {get; init;} = false;
    public required uint PerPlayerLimitCount {get; init;}

    public bool IsCompleted {get; private set;} = false;
    public required DateTime CreatedTime {get; init;}

    
    public Dictionary<uint, uint> PlayerDeliveries {get; set;} = new();

    public void AddResource(uint playerId, uint count)
    {
        
        DeliveredCount += count;
        
        ref uint current = ref CollectionsMarshal.GetValueRefOrAddDefault(PlayerDeliveries, playerId, out _);
        current += count;

        if (TotalCount == DeliveredCount) IsCompleted = true;

    }


}
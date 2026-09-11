
using Server.World.Zone.Settlement.Components;
using Shared.Udp.Packets.Category.Settlement;

namespace Server.World.Zone.Settlement;

public class BaseSettlement
{

    public event Action<S2C_TreasureUpdatePacket>? OnTreasuryUpdate;
    
    public enum Types : byte
    {
        Village, City, Capital
    }
    public Types SettlementType;
    public uint Id {get; protected set;}
    public string Name {get; protected set;} = string.Empty;
    public uint OwnerGuildId {get; protected set;}
    public bool IsControlledNPC {get; protected set;} = true;
    public float BaseTax {get; protected set;}
    public BaseSettlement? ParentSettlement {get; protected set;} = null;

    protected SettlementTreasury Treasury = new SettlementTreasury();


    public BaseSettlement(uint id, string name)
    {
        Id = id;
        Name = name;

        Treasury.OnTreasuryUpdate += (pck) =>
        {
            OnTreasuryUpdate?.Invoke(pck);
        };
    }



    public void AddSilver(ulong amount, PlayerEntity? player, TreasuryActionType action)
    {
        
        if (player != null)
        {
            player.ChangeSilver(-(int)amount);
            Treasury.AddSilver(amount, player, action);
        }
        else
        {
            Treasury.AddSilver(amount, null, action);
        }

    }
    public bool TryRemoveSilver(ulong amount, PlayerEntity? player, TreasuryActionType action)
    {
        
        return Treasury.TryRemoveSilver(amount, player, action);

    }


    #region PLAYER ACTIONS 


    public bool TryWithdrawByPlayer(PlayerEntity player, ulong amount)
    {
        
        bool isSuc = TryRemoveSilver(amount, player, TreasuryActionType.PlayerWithdraw);
        if (isSuc) 
        {
            player.ChangeSilver((int)amount);
            Console.WriteLine($"[City:{Name}] Player:{player.Name} withdraw:{amount} silver. Operation successfully completed!");
            return true;
        }
        else
        {
            Console.WriteLine($"[City:{Name}] Player:{player.Name} try to withdraw:{amount} silver. Operation blocked");
            var pck = new S2C_TreasureUpdatePacket { Amount = Treasury.Silver, Success = false};
            OnTreasuryUpdate?.Invoke(pck);
            return false;
        }

    }
    public bool TryDepositByPlayer(PlayerEntity player, ulong amount)
    {
        
        if (player.Silver < (int)amount || amount == 0)
        {
            var pck = new S2C_TreasureUpdatePacket { Amount = Treasury.Silver, Success = false};
            OnTreasuryUpdate?.Invoke(pck);
            return false;
        }

        AddSilver(amount, player, TreasuryActionType.PlayerDeposit);
        Console.WriteLine($"[City:{Name}] Player:{player.Name} deposit:{amount} silver. Current silver:{Treasury.Silver}");
        
        return true;

    }


    #endregion


}
using Shared.Udp.Packets.Category.Settlement;

namespace Server.World.Zone.Settlement.Components;

public class SettlementTreasury
{

    public event Action<S2C_TreasureUpdatePacket>? OnTreasuryUpdate;
    
    private ulong _silver;
    public ulong Silver
    {
        get => _silver;
        private set
        {
            if (_silver == value) return;
            _silver = value;
        
            var packet = new S2C_TreasureUpdatePacket { Amount = _silver, Success = true };
            OnTreasuryUpdate?.Invoke(packet);
        }
    }

    private TreasuryActionHistory[] History = new TreasuryActionHistory[100];
    private byte _freeIndex = 0;


    public virtual void AddSilver(ulong amount, PlayerEntity? player, TreasuryActionType action)
    {

        if (amount == 0) return;
        
        checked 
        {
            Silver += amount;
        }

        RecordHistory(amount, player, action, defaultSystemName: "Tax System");

    }

    public bool TryRemoveSilver(ulong amount, PlayerEntity? player, TreasuryActionType action)
    {
        
        if (Silver < amount || amount == 0) return false;

        Silver = Silver - amount;

        RecordHistory(amount, player, action, defaultSystemName: "Royal Contract");

        return true;

    }

    private void RecordHistory(ulong amount, PlayerEntity? player, TreasuryActionType action, string defaultSystemName)
    {
        History[_freeIndex] = new TreasuryActionHistory
        {
            PlayerId = player?.EntityId ?? uint.MaxValue,
            Name = player?.Name ?? defaultSystemName,
            Action = action,
            Amount = amount,
            Date = DateTime.UtcNow
        };

        _freeIndex = (byte)((_freeIndex + 1) % History.Length);
    }
}






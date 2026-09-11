using Shared.Udp.Packets.Category.Settlement;

namespace Server.World.Zone.Settlement.Components;

public class SettlementTreasury
{

    public event Action<S2C_TreasureUpdatePacket, S2C_TreasureHistoryUpdatePacket>? OnTreasuryUpdate;
    
    private ulong _silver;
    public ulong Silver
    {
        get => _silver;
        private set
        {
            if (_silver == value) return;
            _silver = value;
        
            var packet = new S2C_TreasureUpdatePacket { Amount = _silver, Success = true };

            int lastIndex = (_freeIndex - 1 + History.Length) % History.Length;
            var curHis = History[lastIndex];
            var hisPacket = new S2C_TreasureHistoryUpdatePacket
            {
                Timestamp = new DateTimeOffset(curHis.Date).ToUnixTimeMilliseconds(),
                Name = curHis.Name,
                Amount = curHis.Amount,
                ActionType = (C2S_TreasuryActionPacket.TreasuryActionType)curHis.Action

            };
            OnTreasuryUpdate?.Invoke(packet, hisPacket);
        }
    }

    private TreasuryActionHistory[] History = new TreasuryActionHistory[100];
    private byte _freeIndex = 0;


    public virtual void AddSilver(ulong amount, PlayerEntity? player, TreasuryActionType action)
    {

        if (amount == 0) return;
        
        RecordHistory(amount, player, action, defaultSystemName: "Tax System");
        checked 
        {
            Silver += amount;
        }

    }

    public bool TryRemoveSilver(ulong amount, PlayerEntity? player, TreasuryActionType action)
    {
        
        if (Silver < amount || amount == 0) return false;

        RecordHistory(amount, player, action, defaultSystemName: "Royal Contract");

        Silver = Silver - amount;

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






namespace Server.World.Zone.Settlement.Components;

public enum TreasuryActionType : byte
{
    None = 0,
    PlayerDeposit = 1,
    TaxMarket = 2,
    TaxPlotRent = 3,
    TaxPlotBuy = 4,

    // WIHTDRAW

    PlayerWithdraw = 5,
    ContractRecerve = 6,

}
public readonly struct TreasuryActionHistory
{
    
    public uint PlayerId {get; init;}
    public string Name {get; init;}
    public required TreasuryActionType Action {get; init;}
    public required ulong Amount {get; init;}
    public required DateTime Date {get; init;}

}
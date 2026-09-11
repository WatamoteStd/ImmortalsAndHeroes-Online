namespace Shared.Udp.Packets;

public enum PacketTypes : ushort
{
    
    C2S_Handshake = 0,
    S2C_HandshakeSuccess = 1,
    S2C_HandshakeFailed = 2,

    S2C_SpawnEntity = 3,
    S2C_MoveEntity = 4,
    C2S_MoveRequest = 5,
    S2C_ItemDiff = 6,
    S2C_InventorySnapshot = 7,

    C2S_ChangeRegionRequest = 8,
    S2C_ChangeRegion = 9,
    S2C_RemoveEntity = 10,
    C2S_AttackRequest = 11,
    S2C_EntityDamageTaked = 12,
    S2C_PlayerExpSync = 13,
    C2S_MasteryTreeLearnRequest = 14,
    S2C_BranchUpdate = 15,
    S2C_StatsSync = 16,
    C2S_CastAbilityRequest = 17,
    S2C_PlayerAbilitySync = 18,
    S2C_CastAbilityFailed = 19,
    S2C_CastAbilitySuccessful= 20,
    S2C_AbilityCasted = 21,
    S2C_EntityMoveSpeedChanged = 22,
    C2S_AdminConsoleCommand = 23,
    S2C_ProjectileDeleted = 24,
    S2C_ProjectileCreated = 25,
    S2C_SilverChanged = 26,
    C2S_TreasuryAction = 27, // 0 - withdraw | 1 deposit
    S2C_TreasureUpdate = 28,
    C2S_RoyalContractCreateRequest = 29,
    S2C_TreasureHistoryUpdate = 30
    

}
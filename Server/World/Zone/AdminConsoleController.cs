using Shared.Udp.Packets;
using Shared.Udp.Packets.Category.Game;
using Server.Network.Interfaces;
using Shared.Items;
using Shared.Udp.Packets.Category;
using Shared.Ability;

namespace Server.World.Zone;

public class AdminConsoleController
{
    
    private IWorldBroadcaster _broadcaster;

    public AdminConsoleController(IWorldBroadcaster broadcaster)
    {
        _broadcaster = broadcaster;
    }

    public void ExecuteCommand(C2S_AdminConsoleCommandPacket packet, PlayerEntity player, WorldZone region)
    {
        
        string[] parts = packet.Payload.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return;

        string mainCmd = parts[0].ToLower();


        switch(mainCmd)
        {
            
            case "/add":
                {
                    
                    if (parts.Length < 3) return;
                    string subCmd = parts[1].ToLower();

                    if (subCmd == "item")
                    {
                        
                        if (uint.TryParse(parts[2], out uint itemId))
                        {
                            
                            ItemType itemType = (ItemType)itemId;
                            var itemData = ItemRegistry.GetItemData(itemType);
                            
                            if (itemData.ItemName != "Unexpected")
                            {
                                
                                ushort count = 1;
                                if (parts.Length >= 4 && ushort.TryParse(parts[3], out ushort parsedCount))
                                {
                                    count = parsedCount;
                                }

                                player.AddItem((ItemType)itemId, count);
                                Console.WriteLine($"[ADMIN] Gave item {itemId} x{count} to {player.Name}");

                                var pkt = new C2S_AdminConsoleCommandPacket
                                {
                                    Payload = $"[Server] Give item:{itemData.ItemName} to player:{player.Name}"
                                };

                                _broadcaster.SendToPlayer<C2S_AdminConsoleCommandPacket>(player.PlayerId, PacketTypes.C2S_AdminConsoleCommand, pkt);

                            }
                            else
                            {
                                Console.WriteLine($"[ADMIN] Unknown Item ID: {itemId}");
                                var pkt = new C2S_AdminConsoleCommandPacket
                                {
                                    Payload = $"[Server] Unknown ItemID:{itemId}"
                                    
                                };
                                _broadcaster.SendToPlayer<C2S_AdminConsoleCommandPacket>(player.PlayerId, PacketTypes.C2S_AdminConsoleCommand, pkt);
                            }

                        }
                        

                    }
                    else if (subCmd == "ability")
                    {
                        
                        if (uint.TryParse(parts[2], out uint abilityId))
                        {
                            
                            AbilityTypes abilityType = (AbilityTypes)abilityId;
                            if (AbilityRegistry.TryGetAbility(abilityType, out var abilityData)) 
                            {
                                
                                player.AddAbility(abilityType);
                                Console.WriteLine($"[ADMIN] Ability added:{abilityData.Title} to {player.Name}");

                                var pkt = new C2S_AdminConsoleCommandPacket
                                {
                                    Payload = $"[Server] Ability added:{abilityData.Title} to {player.Name}"
                                };

                                _broadcaster.SendToPlayer<C2S_AdminConsoleCommandPacket>(player.PlayerId, PacketTypes.C2S_AdminConsoleCommand, pkt);

                            }
                            else
                            {
                                Console.WriteLine($"[ADMIN] Unknown Ability ID:{abilityId}");
                                var pkt = new C2S_AdminConsoleCommandPacket
                                {
                                    Payload = $"[Server] Unknown AbilityId:{abilityId}"
                                    
                                };
                                _broadcaster.SendToPlayer<C2S_AdminConsoleCommandPacket>(player.PlayerId, PacketTypes.C2S_AdminConsoleCommand, pkt);
                            }

                        }

                    }
                    else if (subCmd == "silver")
                    {
                        
                        if (int.TryParse(parts[2], out int silverCount))
                        {
                            player.ChangeSilver(silverCount);
                            Console.WriteLine($"[AdminPanel] Player:{player.Name} add:{silverCount}. Total silver:{player.Silver}");

                            var pkt = new C2S_AdminConsoleCommandPacket
                            {
                                Payload = $"[Server] Player:{player.Name} add:{silverCount}. Total silver:{player.Silver}"
                            };

                            _broadcaster.SendToPlayer<C2S_AdminConsoleCommandPacket>(player.PlayerId, PacketTypes.C2S_AdminConsoleCommand, pkt);
                        }

                    }

                }
            break;

        }

    }

}
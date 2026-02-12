using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

namespace CS2_Poor_MapDecals.Managers;

public class EventManager(CS2_Poor_MapDecals plugin)
{
    private readonly CS2_Poor_MapDecals _plugin = plugin;
    public void RegisterEvents()
    {
        //Events:
        _plugin.RegisterEventHandler<EventRoundStart>(OnRoundStart);
        _plugin.RegisterEventHandler<EventPlayerPing>(OnPlayerPing);

        //Listeners:
        _plugin.RegisterListener<Listeners.OnServerPrecacheResources>((ResourceManifest manifest) =>
        {
            foreach (var prop in _plugin.Config.Props)
            {
                manifest.AddResource(prop);
            }
        });
        _plugin.RegisterListener<Listeners.OnMapStart>(OnMapStart);
        //_plugin.RegisterListener<Listeners.CheckTransmit>(OnCheckTransmit);
        _plugin.RegisterListener<Listeners.OnTick>(OnTick);
        _plugin.AddCommandListener("say", OnPlayerChatListener);
        _plugin.AddCommandListener("say_team", OnPlayerChatListener);
    }

    private void OnTick()
    {
        foreach(var player in _plugin.MenuManager!._selectedMaterial)
        {
            player.Key.PrintToCenterHtml($"{_plugin.Localizer["OnTickNotification", player.Value.material!]}");
        }
    }

    private HookResult OnPlayerChatListener(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (player == null) return HookResult.Continue;
        if (!AdminManager.PlayerHasPermissions(player, _plugin.Config.AdminFlag) || !_plugin.MenuManager!._listenForChat.ContainsKey(player))
        {
            player.PrintToChat($"{_plugin.Localizer["Prefix"]}{_plugin.Localizer["NoAccess"]}");
            return HookResult.Continue;
        }
        var msg = commandInfo.GetArg(1);
        if(string.IsNullOrWhiteSpace(msg)) return HookResult.Continue;
        if(!int.TryParse(msg, out int value))
        {
            player.PrintToChat($"{_plugin.Localizer["Prefix"]}{_plugin.Localizer["NoArg"]}");
            return HookResult.Continue;
        }

        _plugin.MenuManager._listenForChat[player].ModelGroupIndex = value;
        
        player.PrintToChat($"{_plugin.Localizer["Prefix"]}{_plugin.Localizer[$"PlayerSelectedSkin", value]}");

        _plugin.MenuManager._listenForChat[player].EntityProp!.AcceptInput("Skin", _plugin.MenuManager._listenForChat[player].EntityProp, _plugin.MenuManager._listenForChat[player].EntityProp, value.ToString());

        Server.NextFrame(() =>
        {
            _plugin.MenuManager._listenForChat.Remove(player);
        });

        return HookResult.Continue;
    }

    private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        _plugin.PropManager!.SpawnProps();
        return HookResult.Continue;
    }

    private HookResult OnPlayerPing(EventPlayerPing @event, GameEventInfo info)
    {
        var ping = @event;
        var player = ping.Userid;
        if (player == null) return HookResult.Continue;

        if (!AdminManager.PlayerHasPermissions(player, _plugin.Config.AdminFlag) || !_plugin.MenuManager!._selectedMaterial.TryGetValue(player, out var selected))
        {
            return HookResult.Continue;
        }

        var pawn = player.PlayerPawn.Value;
        if (pawn == null) return HookResult.Continue;

        if (!selected!.onPing) return HookResult.Continue;
        if(_plugin.PluginUtils!.CheckMaterial(selected.material!))
        {
            _plugin.PluginUtils!.CreatePropModelOnClick(new Vector(ping.X, ping.Y, ping.Z), new QAngle(pawn.EyeAngles.X, pawn.EyeAngles.Y, pawn.EyeAngles.Z), selected.material!, selected.isVip, selected.isOnGround, selected.materialIndex);
        }
        else
        {
            _plugin.PluginUtils!.CreateDecalOnClick(player, new Vector(ping.X, ping.Y, ping.Z));
        }
        
        return HookResult.Continue;
    }

    /*
    private void OnCheckTransmit(CCheckTransmitInfoList infoList)
    {
        var allAdvs = Utilities.FindAllEntitiesByDesignerName<CEnvDecal>("env_decal");
        if (allAdvs == null || !allAdvs.Any()) return;
        try
        {
            foreach (var entry in infoList)
            {
                CCheckTransmitInfo info;
                CCSPlayerController? player;

                try
                {
                    (info, player) = ((CCheckTransmitInfo, CCSPlayerController))entry;
                }
                catch
                {
                    continue;
                }

                if (player == null) continue;

                if (AdminManager.PlayerHasPermissions(player, _plugin.Config.VipFlag))
                {
                    foreach (var ad in allAdvs)
                    {
                        //if (ad?.Entity?.Name != null && ad.Entity.Name.Contains("advert"))
                        if (ad.Entity!.Name == null) continue;
                        if (ad!.Entity!.Name.StartsWith("advert") && ad!.Entity!.Name.Contains("force"))
                        {
                            info.TransmitEntities.Remove(ad);
                        }
                    }
                }
            }
        }
        catch (Exception error)
        {
            _plugin.DebugMode($"CheckTransmit: ${error}");
        }

    }

    */


    private void OnMapStart(string mapName)
    {
        _plugin.PropManager!._props.Clear();
        _plugin.AllowAdminCommands = false;
        _plugin.PingPlacement = false;
        _plugin.DecalAdToPlace = 0;
        Server.NextFrame(() =>
        {
            _plugin.PropManager._mapName = mapName;
            _plugin.PropManager!._mapFilePath = Path.Combine(_plugin.ModuleDirectory, "maps", $"{mapName}.json");

            _plugin.PropManager.GenerateJsonFile();
            Server.NextFrame(() =>
            {
                _plugin.PropManager.LoadPropsFromMap();
            });
        });
    }

}

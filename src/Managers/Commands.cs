using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

namespace CS2_Poor_MapAdvertisements.Managers;

public class CommandsManager(CS2_Poor_MapAdvertisements plugin)
{
    private readonly CS2_Poor_MapAdvertisements _plugin = plugin;

    public void RegisterCommands()
    {
        if (_plugin.Config.EnableCMD)
        {
            _plugin.AddCommand("css_mapadverts", "Map advertisements menu", OnMapAdvert);
        }
    }
    private void OnMapAdvert(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if(player == null) return;
        var pawn = player.PlayerPawn.Value;
        if (pawn == null) return;

        if (!AdminManager.PlayerHasPermissions(player, _plugin.Config.AdminFlag))
        {
            player.PrintToChat($"{_plugin.Localizer["Prefix"]}{_plugin.Localizer["NoAccess"]}");
            return;
        }

        _plugin.MenuManager!.ShowMapAdvertMenu(player);

        return;
    }

}

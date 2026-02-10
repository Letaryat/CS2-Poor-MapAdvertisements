using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CS2_Poor_MapDecals.Models;
using CS2MenuManager.API.Enum;
using CS2MenuManager.API.Menu;

namespace CS2_Poor_MapDecals.Menu;

public partial class PluginMenu(CS2_Poor_MapDecals plugin)
{
    private readonly CS2_Poor_MapDecals _plugin = plugin;

    public Dictionary<CCSPlayerController, SelectedMaterialModel> _selectedMaterial = new();
    private string[] _retardedWayCords = ["X+", "X-", "Y+", "Y-", "Z+", "Z-"];
    private int[] _decalSize = [16, 32, 64, 128, 256, 512, 1024];

    public void ShowMapAdvertMenu(CCSPlayerController player)
    {
        if (player == null) return;
        WasdMenu menu = new("Map adverts menu", _plugin);
        menu.AddItem("Create prop", (p, o) =>
        {
            CreatePropMenu(player, menu);
            Server.PrintToChatAll("CHUJ");
        });
        menu.AddItem("Create decal", (p, o) =>
        {
            CreateDecalMenu(player, menu);
        });

        menu.AddItem("Edit existing props", (p, o) =>
        {
            EditPropsMenu(player, menu);
        });

        menu.AddItem("Edit existing decals", (p, o) =>
        {

        });

        menu.AddItem("Save all", (p, o) =>
        {

        });

        menu.Display(player, 0);
    }
}
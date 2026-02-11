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
    public Dictionary<CCSPlayerController, PropModel> _listenForChat = new();
    private string[] _retardedWayCords = ["X+", "X-", "Y+", "Y-", "Z+", "Z-"];
    private int[] _decalSize = [16, 32, 64, 128, 256, 512, 1024];

    public void ShowMapAdvertMenu(CCSPlayerController player)
    {
        if (player == null) return;
        WasdMenu menu = new("Map adverts menu", _plugin);
        menu.AddItem("Create prop", (p, o) =>
        {
            CreatePropMenu(player, menu);
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
            EditDecalMenu(player, menu);
        });

        menu.AddItem("Clear advert cache", (p, o) =>
        {
            if(!_plugin.MenuManager!._selectedMaterial.ContainsKey(p)) return;
            _plugin.MenuManager._selectedMaterial.Remove(p);
        });

        menu.Display(player, 0);
    }

    private void CordsMenu(CCSPlayerController player, WasdMenu prevMenu, PropModel prop, int propId, int _type)
    {
        if (player == null) return;
        var pawn = player.PlayerPawn.Value;
        if (pawn == null || !pawn.IsValid) return;

        var entity = prop.EntityProp;
        if (entity == null) return;

        WasdMenu menu = new($"Position for #{propId}", _plugin);

        // _type - 0 Position, 1 - Angles
        if (_type == 0)
        {
            foreach (var i in _retardedWayCords)
            {
                menu.AddItem($"Position {i}5", (p, o) =>
                {
                    var pos = entity!.AbsOrigin!;
                    var newPos = new Vector(pos.X, pos.Y, pos.Z);

                    if (i == "X+") newPos = new Vector(pos.X + 5, pos.Y, pos.Z);
                    else if (i == "X-") newPos = new Vector(pos.X - 5, pos.Y, pos.Z);
                    else if (i == "Y+") newPos = new Vector(pos.X, pos.Y + 5, pos.Z);
                    else if (i == "Y-") newPos = new Vector(pos.X, pos.Y - 5, pos.Z);
                    else if (i == "Z+") newPos = new Vector(pos.X, pos.Y, pos.Z + 5);
                    else if (i == "Z-") newPos = new Vector(pos.X, pos.Y, pos.Z - 5);

                    entity.Teleport(newPos, entity.AbsRotation);
                    o.PostSelectAction = PostSelectAction.Nothing;
                });
            }
        }
        else
        {
            foreach (var i in _retardedWayCords)
            {
                menu.AddItem($"Angle {i}5", (p, o) =>
                {
                    var angles = entity!.AbsRotation!;
                    var newQangle = new QAngle(angles.X, angles.Y, angles.Z);

                    if (i == "X+") newQangle = new QAngle(angles.X + 5, angles.Y, angles.Z);
                    else if (i == "X-") newQangle = new QAngle(angles.X - 5, angles.Y, angles.Z);
                    else if (i == "Y+") newQangle = new QAngle(angles.X, angles.Y + 5, angles.Z);
                    else if (i == "Y-") newQangle = new QAngle(angles.X, angles.Y - 5, angles.Z);
                    else if (i == "Z+") newQangle = new QAngle(angles.X, angles.Y, angles.Z + 5);
                    else if (i == "Z-") newQangle = new QAngle(angles.X, angles.Y, angles.Z - 5);

                    entity.Teleport(entity.AbsOrigin, newQangle);
                    o.PostSelectAction = PostSelectAction.Nothing;
                });
            }
        }
        menu.PrevMenu = prevMenu;
        menu.Display(player, 0);
    }

}
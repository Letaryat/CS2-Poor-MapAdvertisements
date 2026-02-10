using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;
using CS2_Poor_MapDecals.Models;
using CS2MenuManager.API.Enum;
using CS2MenuManager.API.Menu;

namespace CS2_Poor_MapDecals.Managers;

public class MenuManager(CS2_Poor_MapDecals plugin)
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

    // Anything related to props:
    public void CreatePropMenu(CCSPlayerController player, WasdMenu? prevMenu)
    {
        if (player == null) return;
        var pawn = player.PlayerPawn.Value;
        if (pawn == null || !pawn.IsValid) return;

        if (!_selectedMaterial.TryGetValue(player, out var data))
        {
            data = new SelectedMaterialModel
            {
                material = null,
                isVip = false,
                isOnGround = false,
                materialIndex = 0
            };
            _selectedMaterial[player] = data;
        }

        WasdMenu menu = new("Create prop", _plugin);

        menu.AddItem(_selectedMaterial[player].material != null ? _selectedMaterial[player].material! : "Select Material first", DisableOption.DisableHideNumber);

        menu.AddItem("Choose material", (p, o) =>
        {
            PropMaterialsMenu(player, menu);
        });

        menu.AddItem("Spawn prop", (p, o) =>
        {
            _plugin.PluginUtils!.CreatePropModelOnClick(new Vector(pawn.AbsOrigin!.X, pawn.AbsOrigin!.Y, pawn.AbsOrigin!.Z), new QAngle(pawn.EyeAngles.X, pawn.EyeAngles.Y, pawn.EyeAngles.Z), _selectedMaterial[player].material!, _selectedMaterial[player].isVip, _selectedMaterial[player].isOnGround, _selectedMaterial[player].materialIndex);
        }, disableOption: _selectedMaterial[player].material == null
        ? DisableOption.DisableHideNumber
        : DisableOption.None);

        menu.PrevMenu = prevMenu;
        menu.Display(player, 0);
    }

    private void PropMaterialsMenu(CCSPlayerController player, WasdMenu prevMenu)
    {
        if (player == null) return;
        WasdMenu menu = new("Prop materials", _plugin);
        foreach (var material in _plugin.Config.Props)
        {
            if (_plugin.PluginUtils!.CheckMaterial(material))
            {
                menu.AddItem(material, (p, o) =>
                {
                    if (!_selectedMaterial.ContainsKey(player))
                    {
                        _selectedMaterial.TryAdd(player, new SelectedMaterialModel
                        {
                            material = material,
                            isVip = false,
                            isOnGround = false,
                            materialIndex = 0
                        });
                    }
                    else
                    {
                        _selectedMaterial[player].material = material;
                    }
                    o.PostSelectAction = PostSelectAction.Close;

                    Server.NextFrame(() =>
                    {
                        CreatePropMenu(player, (WasdMenu)prevMenu.PrevMenu!);
                    });

                });
            }
        }
        menu.PrevMenu = prevMenu;
        menu.Display(player, 0);
    }

    private void EditPropsMenu(CCSPlayerController player, WasdMenu prevMenu)
    {
        if (player == null) return;
        WasdMenu menu = new("List of props", _plugin);
        foreach (var prop in _plugin.PropManager!._props)
        {
            if (_plugin.PluginUtils!.CheckMaterial(prop.modelPath!))
            {
                menu.AddItem($"{prop.Id}", (p, o) =>
                {
                    EditSpecificProp(player, menu, prop);
                });
            }
        }
        menu.PrevMenu = prevMenu;
        menu.Display(player, 0);
    }

    private void EditSpecificProp(CCSPlayerController player, WasdMenu prevMenu, PropModel prop)
    {
        if (player == null) return;
        var pawn = player.PlayerPawn.Value;
        if (pawn == null || !pawn.IsValid) return;

        WasdMenu menu = new("Edit prop", _plugin);

        var entity = prop.EntityProp;
        menu.AddItem("Teleport to prop", (p, o) =>
        {
            pawn.Teleport(new Vector(prop.posX, prop.posY, prop.posZ));

            o.PostSelectAction = PostSelectAction.Nothing;
        });

        menu.AddItem("Vip Only: {WIP}", (p, o) =>
        {
            pawn.Teleport(new Vector(prop.posX, prop.posY, prop.posZ));
            o.PostSelectAction = PostSelectAction.Nothing;
        });

        menu.AddItem("Select skin {WIP}", (p, o) =>
        {
            pawn.Teleport(new Vector(prop.posX, prop.posY, prop.posZ));
            o.PostSelectAction = PostSelectAction.Nothing;
        });

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

        menu.PrevMenu = prevMenu;
        menu.Display(player, 0);
    }


    // Decal menus:
    public void CreateDecalMenu(CCSPlayerController player, WasdMenu? prevMenu)
    {
        if (player == null) return;
        var pawn = player.PlayerPawn.Value;
        if (pawn == null || !pawn.IsValid) return;

        if (!_selectedMaterial.TryGetValue(player, out var data))
        {
            data = new SelectedMaterialModel
            {
                material = null,
                isVip = false,
                isOnGround = false,
                materialIndex = 0,
                width = 0,
                height = 0
            };
            _selectedMaterial[player] = data;
        }

        WasdMenu menu = new("Create decal", _plugin);

        menu.AddItem(_selectedMaterial[player].material != null ? _selectedMaterial[player].material! : "Select Material first", DisableOption.DisableHideNumber);

        menu.AddItem("Choose material", (p, o) =>
        {
            DecalMaterialsMenu(player, menu);
        });

        menu.AddItem(_selectedMaterial[player].width != 0 ? $"Width: {_selectedMaterial[player].width}" : "Select Width first", (p, o) =>
        {
            DecalHeightxWidthMenu(player, menu, "Width");
        });

        menu.AddItem(_selectedMaterial[player].height != 0 ? $"Width: {_selectedMaterial[player].height}" : "Select Height first", (p, o) =>
        {
            DecalHeightxWidthMenu(player, menu, "Height");
        });

        menu.AddItem($"Depth: {_selectedMaterial[player].depth}", (p, o) =>
        {
            DecalsDepthMenu(player, menu);
        });

        menu.AddItem($"Spawn on Ping: {_selectedMaterial[player].onPing}", (p, o) =>
        {
            if (_selectedMaterial[player].onPing) _selectedMaterial[player].onPing = false;
            else _selectedMaterial[player].onPing = true;

            Server.NextFrame(() =>
            {
                CreateDecalMenu(player, prevMenu);
            });

        }, disableOption: _selectedMaterial[player].material == null
        ? DisableOption.DisableHideNumber
        : DisableOption.None);

        menu.PrevMenu = prevMenu;
        menu.Display(player, 0);

        //Server.PrintToChatAll($"Chuj = {_selectedMaterial[player].material} | {_selectedMaterial[player].onPing} | {_selectedMaterial[player].width} | {_selectedMaterial[player].height}");
    }

    private void DecalMaterialsMenu(CCSPlayerController player, WasdMenu prevMenu)
    {
        if (player == null) return;
        WasdMenu menu = new("Decal materials", _plugin);
        foreach (var material in _plugin.Config.Props)
        {
            if (!_plugin.PluginUtils!.CheckMaterial(material))
            {
                menu.AddItem(material, (p, o) =>
                {
                    if (!_selectedMaterial.ContainsKey(player))
                    {
                        _selectedMaterial.TryAdd(player, new SelectedMaterialModel
                        {
                            material = material,
                            isVip = false,
                            isOnGround = false,
                            materialIndex = 0
                        });
                    }
                    else
                    {
                        _selectedMaterial[player].material = material;
                    }
                    o.PostSelectAction = PostSelectAction.Close;

                    Server.NextFrame(() =>
                    {
                        CreateDecalMenu(player, (WasdMenu)prevMenu.PrevMenu!);
                    });

                });
            }
        }
        menu.PrevMenu = prevMenu;
        menu.Display(player, 0);
    }

    private void DecalHeightxWidthMenu(CCSPlayerController player, WasdMenu prevMenu, string _type)
    {
        if (player == null) return;
        WasdMenu menu = new($"Set {_type}", _plugin);

        foreach (var size in _decalSize)
        {
            menu.AddItem($"{size}", (p, o) =>
            {
                if (_type == "Height") _selectedMaterial[player].height = size;
                else _selectedMaterial[player].width = size;

                o.PostSelectAction = PostSelectAction.Close;

                Server.NextFrame(() =>
                {
                    CreateDecalMenu(player, (WasdMenu)prevMenu.PrevMenu!);
                });
            });
        }

        menu.PrevMenu = prevMenu;
        menu.Display(player, 0);
    }

    private void DecalsDepthMenu(CCSPlayerController player, WasdMenu prevMenu)
    {
        if (player == null) return;
        WasdMenu menu = new($"Current depth: {_selectedMaterial[player].depth}", _plugin);

        menu.AddItem("+1 to depth", (p, o) =>
        {
            _selectedMaterial[player].depth++;
            o.PostSelectAction = PostSelectAction.Reset;

            Server.NextFrame(() =>
            {
                DecalsDepthMenu(player, prevMenu);
            });
        });

        menu.AddItem("-1 to depth", (p, o) =>
        {
            _selectedMaterial[player].depth--;
            o.PostSelectAction = PostSelectAction.Reset;

            Server.NextFrame(() =>
            {
                DecalsDepthMenu(player, prevMenu);
            });

        });

        menu.PrevMenu = prevMenu;
        menu.Display(player, 0);
    }

}
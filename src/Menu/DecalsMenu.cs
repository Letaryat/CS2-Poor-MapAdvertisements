
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CS2_Poor_MapDecals.Models;
using CS2MenuManager.API.Enum;
using CS2MenuManager.API.Menu;

namespace CS2_Poor_MapDecals.Menu;

public partial class PluginMenu
{
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
                height = 0,
                depth = 0
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

    private void EditDecalMenu(CCSPlayerController player, WasdMenu prevMenu)
    {
        if (player == null) return;
        WasdMenu menu = new("List of decals", _plugin);
        foreach (var prop in _plugin.PropManager!._props)
        {
            if (!_plugin.PluginUtils!.CheckMaterial(prop.modelPath!))
            {
                menu.AddItem($"{prop.Id}", (p, o) =>
                {
                    EditSpecificDecal(player, menu, prop, prop.Id);
                });
            }
        }
        menu.PrevMenu = prevMenu;
        menu.Display(player, 0);
    }

    private void EditSpecificDecal(CCSPlayerController player, WasdMenu prevMenu, PropModel prop, int propId)
    {
        if (player == null) return;
        var pawn = player.PlayerPawn.Value;
        if (pawn == null || !pawn.IsValid) return;

        var entity = prop.EntityProp;
        if (entity == null) return;

        WasdMenu menu = new($"Decal #{propId}", _plugin);

        menu.AddItem("Teleport to prop", (p, o) =>
        {
            pawn.Teleport(new Vector(prop.posX, prop.posY, prop.posZ));
            o.PostSelectAction = PostSelectAction.Nothing;
        });

        menu.AddItem($"Width: {prop.width}", (p, o) =>
        {
            WidthAndHeightMenu(player, menu, prop, propId, 0);
        });

        menu.AddItem($"Height {prop.height}", (p, o) =>
        {
            WidthAndHeightMenu(player, menu, prop, propId, 1);
        });

        menu.AddItem("Change Position", (p, o) =>
        {
            CordsMenu(player, menu, prop, propId, 0);
        });

        menu.AddItem("Change Angles", (p, o) =>
        {
            CordsMenu(player, menu, prop, propId, 1);
        });

        menu.AddItem("Save decal configuration", (p, o) =>
        {
            _plugin.PropManager!.SavePropConfiguration(entity.As<CPhysicsPropOverride>(), prop);
            player.PrintToChat($"Saved new decal_{prop.Id} configuration");
            Server.NextFrame(() =>
            {
                EditDecalMenu(player, (WasdMenu)prevMenu.PrevMenu!);
            });
        });

        menu.PrevMenu = prevMenu;
        menu.Display(player, 0);
    }
    private void WidthAndHeightMenu(CCSPlayerController player, WasdMenu prevMenu, PropModel prop, int propId, int _type)
    {
        if (player == null) return;
        var pawn = player.PlayerPawn.Value;
        if (pawn == null || !pawn.IsValid) return;

        var entity = prop.EntityProp;
        if (entity == null) return;

        WasdMenu menu = new($"Width / Height for #{propId}", _plugin);

        // _type - 0 Width, 1 - Height
        if (_type == 0)
        {
            foreach (var i in _decalSize)
            {
                menu.AddItem($"Width: {i}", (p, o) =>
                {
                    var oldDecal = entity.As<CEnvDecal>();
                    var pos = oldDecal.AbsOrigin;
                    var angle = oldDecal.AbsRotation;
                    var height = oldDecal.Height;
                    var material = oldDecal.DecalMaterial;

                    oldDecal.Remove();

                    var newProp = _plugin.PluginUtils!.CreateDecal(pos!, angle!, prop.modelPath!, i, height, prop.forceOnVip, prop.depth);
                    prop.EntityProp = newProp;

                    prop.width = i;

                    o.PostSelectAction = PostSelectAction.Nothing;
                    Server.NextFrame(() =>
                    {
                        EditSpecificDecal(player, prevMenu, prop, propId);
                    });
                });
            }
        }
        else
        {
            foreach (var i in _decalSize)
            {
                menu.AddItem($"Height: {i}", (p, o) =>
                {
                    var oldDecal = entity.As<CEnvDecal>();
                    var pos = oldDecal.AbsOrigin;
                    var angle = oldDecal.AbsRotation;
                    var width = oldDecal.Width;
                    var material = oldDecal.DecalMaterial;

                    oldDecal.Remove();

                    prop.height = i;

                    var newProp = _plugin.PluginUtils!.CreateDecal(pos!, angle!, prop.modelPath!, width, i, prop.forceOnVip, prop.depth);
                    prop.EntityProp = newProp;
                    o.PostSelectAction = PostSelectAction.Nothing;
                    Server.NextFrame(() =>
                    {
                        EditSpecificDecal(player, prevMenu, prop, propId);
                    });
                });
            }
        }
        menu.PrevMenu = prevMenu;
        menu.Display(player, 0);
    }

}
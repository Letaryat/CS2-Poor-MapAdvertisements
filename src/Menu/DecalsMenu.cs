
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CS2_Poor_MapAdvertisements.Models;
using CS2MenuManager.API.Enum;
using CS2MenuManager.API.Menu;

namespace CS2_Poor_MapAdvertisements.Menu;

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
                depth = 14
            };
            _selectedMaterial[player] = data;
        }

        WasdMenu menu = new("Create decal", _plugin);

        menu.AddItem(_selectedMaterial[player].material != null ? _selectedMaterial[player].material! : $"{_plugin.Localizer["ChooseMaterial"]}", DisableOption.DisableHideNumber);

        menu.AddItem($"{_plugin.Localizer["Material_Header"]}", (p, o) =>
        {
            DecalMaterialsMenu(player, menu);
        });

        menu.AddItem(
            _selectedMaterial[player].width != 0
                ? string.Format(_plugin.Localizer["Decal_Width"], _selectedMaterial[player].width)
                : _plugin.Localizer["SelectFirst_Width"],
            (p, o) =>
            {
                DecalHeightxWidthMenu(player, menu, "Width");
            });

        menu.AddItem(
            _selectedMaterial[player].width != 0
                ? string.Format(_plugin.Localizer["Decal_Height"], _selectedMaterial[player].height)
                : _plugin.Localizer["SelectFirst_Height"],
            (p, o) =>
            {
                DecalHeightxWidthMenu(player, menu, "Height");
            });

        menu.AddItem($"{string.Format(_plugin.Localizer["Decal_Depth"], _selectedMaterial[player].depth)}",
            (p, o) =>
            {
                DecalsDepthMenu(player, menu);
            });

        menu.AddItem($"{_plugin.Localizer["VipOnly", data.isVip]}", (p, o) =>
        {
            if (data.isVip)
            {
                data.isVip = false;
            }
            else
            {
                data.isVip = true;
            }

            Server.NextFrame(() =>
            {
                CreateDecalMenu(player, prevMenu);
            });
        });

        menu.AddItem($"{_plugin.Localizer["SpawnOnPing", _selectedMaterial[player].onPing]}", (p, o) =>
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
    }

    private void DecalMaterialsMenu(CCSPlayerController player, WasdMenu prevMenu)
    {
        if (player == null) return;
        WasdMenu menu = new($"{_plugin.Localizer["Material_Header"]}", _plugin);
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
        WasdMenu menu = new($"{_plugin.Localizer["DecalDepth_Header", _selectedMaterial[player].depth]}", _plugin);

        menu.AddItem($"{_plugin.Localizer["DecalDepth_Item"]}", (p, o) =>
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

    private void EditDepth(CCSPlayerController player, WasdMenu prevMenu, PropModel prop)
    {
        if (player == null) return;
        WasdMenu menu = new($"{_plugin.Localizer["DecalDepth_Header", _selectedMaterial[player].depth]}", _plugin);

        menu.AddItem($"{_plugin.Localizer["DecalDepth_ItemPlus"]}", (p, o) =>
        {
            prop.depth++;
            o.PostSelectAction = PostSelectAction.Reset;

            var entity = prop.EntityProp;
            if (entity == null) return;

            var oldDecal = entity.As<CEnvDecal>();
            var pos = oldDecal.AbsOrigin;
            var angle = oldDecal.AbsRotation;
            var width = oldDecal.Width;
            var height = oldDecal.Height;
            var material = oldDecal.DecalMaterial;

            oldDecal.Remove();

            var newProp = _plugin.PluginUtils!.CreateDecal(pos!, angle!, prop.modelPath!, width, height, prop.forceOnVip, prop.depth);
            prop.EntityProp = newProp;

            o.PostSelectAction = PostSelectAction.Nothing;
        });

        menu.AddItem($"{_plugin.Localizer["DecalDepth_ItemMinus"]}", (p, o) =>
        {
            prop.depth--;
            o.PostSelectAction = PostSelectAction.Reset;

            var entity = prop.EntityProp;
            if (entity == null) return;

            var oldDecal = entity.As<CEnvDecal>();
            var pos = oldDecal.AbsOrigin;
            var angle = oldDecal.AbsRotation;
            var width = oldDecal.Width;
            var height = oldDecal.Height;
            var material = oldDecal.DecalMaterial;

            oldDecal.Remove();

            var newProp = _plugin.PluginUtils!.CreateDecal(pos!, angle!, prop.modelPath!, width, height, prop.forceOnVip, prop.depth);
            prop.EntityProp = newProp;

            o.PostSelectAction = PostSelectAction.Nothing;
        });

        menu.PrevMenu = prevMenu;
        menu.Display(player, 0);
    }
    private void EditDecalMenu(CCSPlayerController player, WasdMenu prevMenu)
    {
        if (player == null) return;
        WasdMenu menu = new($"{_plugin.Localizer["ListOfDecals_Header"]}", _plugin);
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

        WasdMenu menu = new($"{_plugin.Localizer[$"EditDecal_Header"]} #{propId}", _plugin);

        menu.AddItem($"{_plugin.Localizer[$"TeleportToAdv"]}", (p, o) =>
        {
            pawn.Teleport(new Vector(prop.posX, prop.posY, prop.posZ));
            o.PostSelectAction = PostSelectAction.Nothing;
        });

        menu.AddItem($"{_plugin.Localizer["VipOnly", prop.forceOnVip]}", (p, o) =>
{
    if (prop.forceOnVip == true)
    {
        prop.forceOnVip = false;
    }
    else
    {
        prop.forceOnVip = true;
    }

    Server.NextFrame(() =>
    {
        EditSpecificProp(player, prevMenu, prop, propId);
    });
});

        menu.AddItem($"{_plugin.Localizer[$"ChooseMaterial"]}", (p, o) =>
        {
            DecalMaterialEdit(player, menu, prop, propId);
        });

        menu.AddItem($"{_plugin.Localizer[$"Decal_Width", prop.width]}", (p, o) =>
        {
            WidthAndHeightMenu(player, menu, prop, propId, 0);
        });

        menu.AddItem($"{_plugin.Localizer[$"Decal_Height", prop.height]} ", (p, o) =>
        {
            WidthAndHeightMenu(player, menu, prop, propId, 1);
        });

        menu.AddItem($"{_plugin.Localizer[$"Decal_Depth", prop.depth]}", (p, o) =>
        {
            EditDepth(player, menu, prop);
        });

        menu.AddItem($"{_plugin.Localizer[$"ChangePositionAdvert"]}", (p, o) =>
        {
            CordsMenu(player, menu, prop, propId, 0);
        });

        menu.AddItem($"{_plugin.Localizer[$"ChangeAnglesAdvert"]}", (p, o) =>
        {
            CordsMenu(player, menu, prop, propId, 1);
        });
        menu.AddItem($"{_plugin.Localizer[$"ConfigChangePositionAdvert"]}", (p, o) =>
        {
            CordsMenu(player, menu, prop, propId, 3);
        });
        menu.AddItem($"{_plugin.Localizer[$"ConfigChangeAnglesAdvert"]}", (p, o) =>
        {
            CordsMenu(player, menu, prop, propId, 2);
        });

        menu.AddItem($"{_plugin.Localizer[$"SavePropConfig"]}", (p, o) =>
        {
            _plugin.PropManager!.SavePropConfiguration(entity.As<CPhysicsPropOverride>(), prop);
            player.PrintToChat($"{_plugin.Localizer["Prefix"]}{_plugin.Localizer[$"SavedProp", prop.Id]}");
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

        WasdMenu menu = new($"{_plugin.Localizer[$"ChangeHeader_{_type}"]} #{propId}", _plugin);

        // _type - 0 Width, 1 - Height
        if (_type == 0)
        {
            foreach (var i in _decalSize)
            {
                menu.AddItem($"{i}", (p, o) =>
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

                    o.PostSelectAction = PostSelectAction.Close;
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
                menu.AddItem($"{i}", (p, o) =>
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


    private void DecalMaterialEdit(CCSPlayerController player, WasdMenu prevMenu, PropModel prop, int propId)
    {
        if (player == null) return;
        WasdMenu menu = new($"{_plugin.Localizer["Material_Header"]}", _plugin);

        var entity = prop.EntityProp;
        if (entity == null) return;

        foreach (var material in _plugin.Config.Props)
        {
            if (!_plugin.PluginUtils!.CheckMaterial(material))
            {
                menu.AddItem(material, (p, o) =>
                {
                    var oldDecal = entity.As<CEnvDecal>();
                    var pos = oldDecal.AbsOrigin;
                    var angle = oldDecal.AbsRotation;
                    var width = oldDecal.Width;
                    var height = oldDecal.Height;

                    oldDecal.Remove();

                    prop.modelPath = material;

                    var newProp = _plugin.PluginUtils!.CreateDecal(pos!, angle!, material, width, height, prop.forceOnVip, prop.depth);
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

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CS2_Poor_MapDecals.Models;
using CS2MenuManager.API.Enum;
using CS2MenuManager.API.Menu;

namespace CS2_Poor_MapDecals.Menu;

public partial class PluginMenu
{
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
                materialIndex = 0,
                width = 0,
                height = 0,
                depth = 0,
                onPing = false,
            };
            _selectedMaterial[player] = data;
        }

        WasdMenu menu = new($"{_plugin.Localizer["Prop_Header"]}", _plugin);

        menu.AddItem(_selectedMaterial[player].material != null ? _selectedMaterial[player].material! : $"{_plugin.Localizer["ChooseMaterial"]}", DisableOption.DisableHideNumber);

        menu.AddItem($"{_plugin.Localizer["Material_Header"]}", (p, o) =>
        {
            PropMaterialsMenu(player, menu);
        });
        menu.AddItem($"{_plugin.Localizer["SpawnOnPing", _selectedMaterial[player].onPing]} ", (p, o) =>
        {
            if (_selectedMaterial[player].onPing) _selectedMaterial[player].onPing = false;
            else _selectedMaterial[player].onPing = true;

            Server.NextFrame(() =>
            {
                CreatePropMenu(player, prevMenu);
            });

        }, disableOption: _selectedMaterial[player].material == null
        ? DisableOption.DisableHideNumber
        : DisableOption.None);

        menu.PrevMenu = prevMenu;
        menu.Display(player, 0);
    }

    private void PropMaterialsMenu(CCSPlayerController player, WasdMenu prevMenu)
    {
        if (player == null) return;
        WasdMenu menu = new($"{_plugin.Localizer["Material_Header"]}", _plugin);
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
        WasdMenu menu = new($"{_plugin.Localizer["ListOfProps_Header"]}", _plugin);
        foreach (var prop in _plugin.PropManager!._props)
        {
            if (_plugin.PluginUtils!.CheckMaterial(prop.modelPath!))
            {
                menu.AddItem($"{prop.Id}", (p, o) =>
                {
                    EditSpecificProp(player, menu, prop, prop.Id);
                });
            }
        }
        menu.PrevMenu = prevMenu;
        menu.Display(player, 0);
    }

    private void EditSpecificProp(CCSPlayerController player, WasdMenu prevMenu, PropModel prop, int propId)
    {

        if (player == null) return;

        var pawn = player.PlayerPawn.Value;
        if (pawn == null || !pawn.IsValid) return;

        WasdMenu menu = new($"{_plugin.Localizer["EditProp_Header", propId]}", _plugin);

        var entity = prop.EntityProp;
        if (entity == null) return;

        menu.AddItem($"{_plugin.Localizer["TeleportToAdv"]}", (p, o) =>
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

        menu.AddItem($"{_plugin.Localizer["SelectPropSkin"]} {prop.ModelGroupIndex}", (p, o) =>
        {
            _listenForChat.Add(player, prop);

            player.PrintToChat($"{_plugin.Localizer["Prefix"]}{_plugin.Localizer["NotificationSkin"]}");

            o.PostSelectAction = PostSelectAction.Nothing;
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
                EditPropsMenu(player, (WasdMenu)prevMenu.PrevMenu!);
            });
        });

        menu.PrevMenu = prevMenu;
        menu.Display(player, 0);
    }

}
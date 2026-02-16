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
        WasdMenu menu = new($"{_plugin.Localizer["MapAdvertMenu_Header"]}", _plugin);
        menu.AddItem($"{_plugin.Localizer["CreatePropMenu"]}", (p, o) =>
        {
            CreatePropMenu(player, menu);
        });
        menu.AddItem($"{_plugin.Localizer["CreateDecalMenu"]}", (p, o) =>
        {
            CreateDecalMenu(player, menu);
        });

        menu.AddItem($"{_plugin.Localizer["EditPropsMenu"]}", (p, o) =>
        {
            EditPropsMenu(player, menu);
        });

        menu.AddItem($"{_plugin.Localizer["EditDecalsMenu"]}", (p, o) =>
        {
            EditDecalMenu(player, menu);
        });

        menu.AddItem($"{_plugin.Localizer["RemoveAdvertsMenu"]}", (p, o) =>
        {
            RemoveAdvertsMenu(player, menu);
        });

        menu.AddItem($"{_plugin.Localizer["SaveAdvertsMenu"]}", (p, o) =>
        {
            try
            {
                _plugin.PropManager!.SaveAllAdverts();
                p.PrintToChat($"{_plugin.Localizer["Prefix"]}{_plugin.Localizer["SavedAdverts"]}");
            }
            catch (Exception error)
            {
                p.PrintToChat($"{_plugin.Localizer["Prefix"]}{_plugin.Localizer["SavedAdvertsError"]}");
                _plugin.DebugMode($"{error}");
            }

        });

        menu.AddItem($"{_plugin.Localizer["ClearCacheMenu"]}", (p, o) =>
        {
            if (!_plugin.MenuManager!._selectedMaterial.ContainsKey(p)) return;
            _plugin.MenuManager._selectedMaterial.Remove(p);

        });

        menu.Display(player, 0);
    }

    private void RemoveAdvertsMenu(CCSPlayerController player, WasdMenu prevMenu)
    {
        if (player == null) return;
        var pawn = player.PlayerPawn.Value;
        if (pawn == null || !pawn.IsValid) return;

        WasdMenu menu = new($"{_plugin.Localizer["RemoveAdvert_Header"]}", _plugin);

        foreach (var adv in _plugin.PropManager!._props)
        {
            menu.AddItem($"{adv.Id}", (p, o) =>
            {
                _plugin.PropManager.RemovePropFromFile(adv.Id);
                player.PrintToChat($"{_plugin.Localizer["SuccessRemove", adv.Id]}");
                o.PostSelectAction = PostSelectAction.Close;
                Server.NextFrame(() =>
                {
                    ShowMapAdvertMenu(player);
                });
            });
        }

        menu.PrevMenu = prevMenu;
        menu.Display(player, 0);
    }

    private void CordsMenu(CCSPlayerController player, WasdMenu prevMenu, PropModel prop, int propId, int _type)
    {
        if (player == null) return;
        var pawn = player.PlayerPawn.Value;
        if (pawn == null || !pawn.IsValid) return;

        var entity = prop.EntityProp;
        if (entity == null) return;

        WasdMenu menu = new($"{_plugin.Localizer[$"CordsFor_{_type}", propId]} ", _plugin);

        // _type - 0 Position, 1 - Angles, 2 - Config angles values, 3 - Config position values

        if (_type == 0)
        {
            foreach (var v in _plugin.Config.customPositionValues)
            {
                foreach (var i in _retardedWayCords)
                {
                    menu.AddItem($"{i} {v}", (p, o) =>
                    {
                        var pos = entity!.AbsOrigin!;
                        var newPos = new Vector(pos.X, pos.Y, pos.Z);

                        if (i == "X+") newPos = new Vector(pos.X + v, pos.Y, pos.Z);
                        else if (i == "X-") newPos = new Vector(pos.X - v, pos.Y, pos.Z);
                        else if (i == "Y+") newPos = new Vector(pos.X, pos.Y + v, pos.Z);
                        else if (i == "Y-") newPos = new Vector(pos.X, pos.Y - v, pos.Z);
                        else if (i == "Z+") newPos = new Vector(pos.X, pos.Y, pos.Z + v);
                        else if (i == "Z-") newPos = new Vector(pos.X, pos.Y, pos.Z - v);

                        entity.Teleport(newPos, entity.AbsRotation);
                        o.PostSelectAction = PostSelectAction.Nothing;
                    });
                }
            }

        }
        else if (_type == 1)
        {
            foreach (var v in _plugin.Config.customAngleValues)
            {
                foreach (var i in _retardedWayCords)
                {
                    menu.AddItem($"{i} {v}", (p, o) =>
                    {
                        var angles = entity!.AbsRotation!;
                        var newQangle = new QAngle(angles.X, angles.Y, angles.Z);

                        if (i == "X+") newQangle = new QAngle(angles.X + v, angles.Y, angles.Z);
                        else if (i == "X-") newQangle = new QAngle(angles.X - v, angles.Y, angles.Z);
                        else if (i == "Y+") newQangle = new QAngle(angles.X, angles.Y + v, angles.Z);
                        else if (i == "Y-") newQangle = new QAngle(angles.X, angles.Y - v, angles.Z);
                        else if (i == "Z+") newQangle = new QAngle(angles.X, angles.Y, angles.Z + v);
                        else if (i == "Z-") newQangle = new QAngle(angles.X, angles.Y, angles.Z - v);

                        entity.Teleport(entity.AbsOrigin, newQangle);
                        o.PostSelectAction = PostSelectAction.Nothing;

                    });
                }
            }
        }

        /*
        if (_type == 0)
        {
            foreach (var i in _retardedWayCords)
            {
                menu.AddItem($"{i}5", (p, o) =>
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
        else if (_type == 1)
        {
            foreach (var i in _retardedWayCords)
            {
                menu.AddItem($"{i}5", (p, o) =>
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

        else if (_type == 2)
        {

            foreach (var a in _plugin.Config.customAngleValues)
            {
                foreach (var i in _retardedWayCords)
                {
                    menu.AddItem($"{i}{a}", (p, o) =>
                    {
                        var angles = entity!.AbsRotation!;

                        float x = angles.X;
                        float y = angles.Y;
                        float z = angles.Z;

                        if (i.StartsWith("X"))
                        {
                            x = _plugin.PluginUtils!.SnapToStep(x, a);
                            x += (i == "X+" ? a : -a);
                        }
                        else if (i.StartsWith("Y"))
                        {
                            y = _plugin.PluginUtils!.SnapToStep(y, a);
                            y += (i == "Y+" ? a : -a);
                        }
                        else if (i.StartsWith("Z"))
                        {
                            z = _plugin.PluginUtils!.SnapToStep(z, a);
                            z += (i == "Z+" ? a : -a);
                        }

                        entity.Teleport(entity.AbsOrigin, new QAngle(x, y, z));
                        o.PostSelectAction = PostSelectAction.Nothing;
                    });
                }

            }
        }
        else if (_type == 3)
        {

            foreach (var a in _plugin.Config.customPositionValues)
            {
                foreach (var i in _retardedWayCords)
                {
                    menu.AddItem($"{i}{a}", (p, o) =>
                    {
                        var position = entity!.AbsOrigin!;

                        float x = position.X;
                        float y = position.Y;
                        float z = position.Z;

                        if (i.StartsWith("X"))
                        {
                            x = _plugin.PluginUtils!.SnapToStep(x, a);
                            x += (i == "X+" ? a : -a);
                        }
                        else if (i.StartsWith("Y"))
                        {
                            y = _plugin.PluginUtils!.SnapToStep(y, a);
                            y += (i == "Y+" ? a : -a);
                        }
                        else if (i.StartsWith("Z"))
                        {
                            z = _plugin.PluginUtils!.SnapToStep(z, a);
                            z += (i == "Z+" ? a : -a);
                        }

                        entity.Teleport(new Vector(x,y,z), entity.AbsRotation);
                        o.PostSelectAction = PostSelectAction.Nothing;
                    });
                }
            }
        }
        */
        menu.PrevMenu = prevMenu;
        menu.Display(player, 0);
    }

}
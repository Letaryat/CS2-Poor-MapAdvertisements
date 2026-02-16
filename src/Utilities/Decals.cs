using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace CS2_Poor_MapAdvertisements.Utils;

public partial class PluginUtils
{
    /*
        Thanks to github.com/f3nixCodings for providing me updated way of creating decals.
    */
    public const int DecalDepth = 12;
    private const float DecalBackwardOffset = 2f;
    public CEnvDecal? CreateDecal(Vector cords, QAngle angle, string material, float width, float height, bool forceOnVip, int depth)
    {
        try
        {
            using var keyValues = new CEntityKeyValues();
            var entity = Utilities.CreateEntityByName<CEnvDecal>("env_decal");
            if (entity == null) return null;

            entity.Entity!.Name = $"advert_decals_";

            if (forceOnVip)
            {
                entity.Entity!.Name += "_force";
            }

            keyValues.SetString("targetname", entity.Entity.Name);
            keyValues.SetString("material", material);

            entity.Width = width;
            entity.Height = height;
            entity.Depth = depth;
            entity.RenderOrder = 1;
            entity.RenderMode = RenderMode_t.kRenderNormal;
            entity.ProjectOnWorld = true;

            entity.Teleport(cords, angle);
            entity.DispatchSpawn(keyValues);
            return entity;
        }
        catch (Exception error)
        {
            _plugin.DebugMode($"{error}");
            return null;
        }
    }

    public void CreateDecalOnClick(CCSPlayerController player, Vector position)
    {
        var pawn = player.PlayerPawn.Value;
        if (pawn == null || !pawn.IsValid) return;
        float flippedYaw = (pawn.EyeAngles.Y + 180.0f) % 360.0f;
        QAngle spriteAngle = new QAngle(pawn.EyeAngles.X, flippedYaw, pawn.EyeAngles.Z);
        Vector impactPos = new Vector(position.X, position.Y, position.Z);

        Vector backward = -GetForwardVector(pawn.EyeAngles);
        backward = Normalize(backward);

        Vector offsetPos = impactPos + backward * 2f;

        var eyeAngleZ = GetPlayerEyeVector(pawn);

        if (!_plugin.MenuManager!._selectedMaterial.TryGetValue(player, out var selected)) return;

        try
        {
            if (eyeAngleZ < -0.90)
            {
                offsetPos.Z += 1f;
                var entity = CreateDecal(offsetPos, new QAngle(0, spriteAngle.Y, 0), selected.material!, selected.width, selected.height, selected.isVip, selected.depth);
                var model = _plugin.PropManager!.PushCordsToFile(offsetPos, new QAngle(0, spriteAngle.Y, 0), selected.material!, selected.width, selected.height, selected.isVip, selected.depth, false, 0, entity!);
            }
            else
            {
                var entity = CreateDecal(offsetPos, new QAngle(90, spriteAngle.Y, 0), selected.material!, selected.width, selected.height, selected.isVip, selected.depth);
                var model = _plugin.PropManager!.PushCordsToFile(offsetPos, new QAngle(90, spriteAngle.Y, 0), selected.material!, selected.width, selected.height, selected.isVip, selected.depth, false, 0, entity!);
                if (entity != null)
                {
                    model!.EntityProp = entity;
                }
            }
        }
        catch (Exception error)
        {
            _plugin.DebugMode($"{error}");
        }
    }


}
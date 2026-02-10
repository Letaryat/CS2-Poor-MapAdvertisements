using System.Runtime.InteropServices;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using System.Runtime.CompilerServices;


namespace CS2_Poor_MapDecals.Utils;

public class PluginUtils(CS2_Poor_MapDecals plugin)
{
    private readonly CS2_Poor_MapDecals _plugin = plugin;
    public const int DecalDepth = 12;
    private const float DecalBackwardOffset = 2f;
    public void CreateDecal(Vector cords, QAngle angle, int index, float width, float height, bool forceOnVip)
    {
        if (index < 0 || index >= _plugin.Config.Props.Length)
        {
            _plugin.DebugMode($"Invalid decal index {index}. Skipping.");
            return;
        }

        try
        {
            using var keyValues = new CEntityKeyValues();
            var entity = Utilities.CreateEntityByName<CEnvDecal>("env_decal");
            if (entity == null) return;

            var tick = Server.TickCount;
            entity.Entity!.Name = forceOnVip ? $"advert_force_{tick}" : $"advert_{tick}";

            // Set material via CEntityKeyValues -- the engine handles loading the material
            // and creating the proper CStrongHandle with reference counting, preventing
            // the resource from being garbage-collected during long server runs.
            keyValues.SetString("targetname", entity.Entity.Name);
            keyValues.SetString("material", "materials/cybershoke.vmat");

            entity.Width = width;
            entity.Height = height;
            entity.Depth = 12;
            entity.RenderOrder = 1;
            entity.RenderMode = RenderMode_t.kRenderNormal;
            entity.ProjectOnWorld = true;

            entity.Teleport(cords, angle);
            entity.DispatchSpawn(keyValues);
        }
        catch (Exception error)
        {
            _plugin.DebugMode($"{error}");
        }
    }

    public CPhysicsPropOverride? CreatePropModel(Vector cords, QAngle angle, string material, bool forceOnVip, bool onGround, int materialIndex, int propId)
    {
        var entity = Utilities.CreateEntityByName<CPhysicsPropOverride>("prop_physics_override");
        if (entity == null) return null;

        entity.Entity!.Name = $"advert_prop{propId}";
        if (forceOnVip)
        {
            entity.Entity!.Name += "_force";
        }
        QAngle qangle = new QAngle(0, angle.Y, 0);
        entity.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags &= ~(uint)(1 << 2);
        entity.SetModel(material);
        entity.Teleport(new Vector(cords.X, cords.Y, cords.Z), qangle);
        if (onGround)
        {
            entity.AbsRotation!.X = -90;
        }
        if (materialIndex != 0)
        {
            entity.AcceptInput("Skin", entity, entity, materialIndex.ToString());
        }

        entity!.DispatchSpawn();
        return entity;
    }

        public CPhysicsPropOverride? CreatePropModelOnClick(Vector cords, QAngle angle, string material, bool forceOnVip, bool onGround, int materialIndex)
    {
        var entity = Utilities.CreateEntityByName<CPhysicsPropOverride>("prop_physics_override");
        if (entity == null) return null;

        entity.Entity!.Name = $"advert_prop";
        if (forceOnVip)
        {
            entity.Entity!.Name += "_force";
        }
        QAngle qangle = new QAngle(0, angle.Y, 0);
        entity.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags &= ~(uint)(1 << 2);
        entity.SetModel(material);
        entity.Teleport(new Vector(cords.X, cords.Y, cords.Z), qangle);
        if (onGround)
        {
            entity.AbsRotation!.X = -90;
        }
        if (materialIndex != 0)
        {
            entity.AcceptInput("Skin", entity, entity, materialIndex.ToString());
        }

        entity!.DispatchSpawn();
        return entity;
    }

    public void CreateDecalOnClick(CCSPlayerPawn pawn, Vector position, string material, float width, float height, bool forceOnVip)
    {
        float flippedYaw = (pawn.EyeAngles.Y + 180.0f) % 360.0f;
        QAngle spriteAngle = new QAngle(pawn.EyeAngles.X, flippedYaw, pawn.EyeAngles.Z);
        Vector impactPos = new Vector(position.X, position.Y, position.Z);

        Vector backward = -GetForwardVector(pawn.EyeAngles);
        backward = Normalize(backward);

        Vector offsetPos = impactPos + backward * 2f;

        var eyeAngleZ = GetPlayerEyeVector(pawn);

        try
        {
            if (eyeAngleZ < -0.90)
            {
                offsetPos.Z += 1f;
                CreateDecal(offsetPos, new QAngle(0, spriteAngle.Y, 0), 0, width, height, forceOnVip);
                _plugin.PropManager!.PushCordsToFile(offsetPos, new QAngle(0, spriteAngle.Y, 0), material, width, height, forceOnVip);
            }
            else
            {
                CreateDecal(offsetPos, new QAngle(90, spriteAngle.Y, 0), 0, width, height, forceOnVip);
                _plugin.PropManager!.PushCordsToFile(offsetPos, new QAngle(90, spriteAngle.Y, 0), material, width, height, forceOnVip);
            }
        }
        catch (Exception error)
        {
            _plugin.DebugMode($"{error}");
        }
    }

    public Vector GetForwardVector(QAngle angles)
    {
        float radYaw = angles.Y * (float)(Math.PI / 180.0);
        return new Vector((float)Math.Cos(radYaw), (float)Math.Sin(radYaw), 0);
    }
    public Vector Normalize(Vector vec)
    {
        float length = MathF.Sqrt(vec.X * vec.X + vec.Y * vec.Y + vec.Z * vec.Z);
        if (length == 0)
            return new Vector(0, 0, 0);
        return new Vector(vec.X / length, vec.Y / length, vec.Z / length);
    }

    private float GetPlayerEyeVector(CCSPlayerPawn pawn)
    {
        // Credits to: 
        // https://github.com/edgegamers/Jailbreak/blob/main/mod/Jailbreak.Warden/Paint/WardenPaintBehavior.cs#L131
        if (pawn == null || !pawn.IsValid) return 0;
        var eyeAngle = pawn.EyeAngles;
        var pitch = Math.PI / 180 * eyeAngle.X;
        var yaw = Math.PI / 180 * eyeAngle.Y;
        var eyeVector = new Vector((float)(Math.Cos(yaw) * Math.Cos(pitch)), (float)(Math.Sin(yaw) * Math.Cos(pitch)), (float)-Math.Sin(pitch));
        return eyeVector.Z;
    }
    public bool CheckMaterial(string materialPath)
    {
        var splitMaterialPath = materialPath.Split(".");
        if (splitMaterialPath[1] == "vmdl")
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}

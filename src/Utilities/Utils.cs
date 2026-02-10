using System.Runtime.InteropServices;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using System.Runtime.CompilerServices;


namespace CS2_Poor_MapDecals.Utils;

public partial class PluginUtils(CS2_Poor_MapDecals plugin)
{
    private readonly CS2_Poor_MapDecals _plugin = plugin;
 
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

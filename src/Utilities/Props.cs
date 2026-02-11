using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace CS2_Poor_MapDecals.Utils;

public partial class PluginUtils
{
    public CPhysicsPropOverride? CreatePropModel(Vector cords, QAngle angle, string material, bool forceOnVip, bool onGround, int materialIndex, int propId)
    {
        var entity = Utilities.CreateEntityByName<CPhysicsPropOverride>("prop_physics_override");
        if (entity == null) return null;

        entity.Entity!.Name = $"advert_prop{propId}";
        if (forceOnVip)
        {
            entity.Entity!.Name += "_force";
        }
        //QAngle qangle = new QAngle(0, angle.Y, 0);
        entity.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags &= ~(uint)(1 << 2);
        entity.SetModel(material);
        entity.Teleport(new Vector(cords.X, cords.Y, cords.Z), angle);
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

        //int newId = _plugin.PropManager!._props.Count();

        Server.PrintToChatAll("TEST");

        entity.Entity!.Name = $"advert_prop";
        if (forceOnVip)
        {
            entity.Entity!.Name += "_force";
        }
        //QAngle qangle = new QAngle(0, angle.Y, 0);
        entity.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags &= ~(uint)(1 << 2);
        entity.SetModel(material);
        entity.Teleport(new Vector(cords.X, cords.Y, cords.Z), angle);
        if (onGround)
        {
            entity.AbsRotation!.X = -90;
        }
        if (materialIndex != 0)
        {
            entity.AcceptInput("Skin", entity, entity, materialIndex.ToString());
        }

        entity!.DispatchSpawn();

        var model = _plugin.PropManager!.PushCordsToFile(cords, angle, material, 0, 0, forceOnVip, 0, onGround, materialIndex, entity);

        if(entity != null)
        {
            model!.EntityProp = entity;
        }

        return entity;
    }

    /*
    public void NewPropToNewPropList(Vector pos, QAngle angle, string ModelPath, bool forceOnVip, bool onGround, int materialIndex)
    {
        if(_newPropsCount <= -1)
        {
            _newPropsCount = _plugin.PropManager!._props.Count();
        }
        _plugin.PropManager!._newPropModels.Add(new PropModel
        {
            Id = _newPropsCount,
            modelPath = ModelPath,
            posX = pos.X,
            posY = pos.Y,
            posZ = pos.Z,
            angleX = angle.X,
            angleY = angle.Y,
            angleZ = angle.Z,
            isOnGround = onGround,
            forceOnVip = forceOnVip,
            width = 0,
            height = 0,
            depth = 0
        });
        _newPropsCount++;
        return;
    }
    */

}
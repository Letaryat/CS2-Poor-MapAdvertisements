using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CS2_Poor_MapDecals.Models;
using System.Text.Json;

namespace CS2_Poor_MapDecals.Managers
{
    public class PropManager(CS2_Poor_MapDecals plugin)
    {
        private readonly CS2_Poor_MapDecals _plugin = plugin;
        public string? _mapName;
        public string? _mapFilePath;
        public readonly List<PropModel> _props = [];
        //public readonly Dictionary<int, CPhysicsPropOverride> _entityProps = [];
        public readonly List<PropModel> _newPropModels = [];
        private static readonly object _fileLock = new();

        public void GenerateJsonFile()
        {
            string directoryPath = Path.Combine(_plugin.ModuleDirectory, "maps");
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                if (!File.Exists(_mapFilePath))
                {
                    File.WriteAllText(_mapFilePath!, "[]");
                }
            }
            catch (Exception e)
            {
                _plugin.DebugMode($"{e}");
            }
        }

        public PropModel? PushCordsToFile(Vector pos, QAngle angle, string ModelPath, float width, float height, bool forceToVip, int depth, bool isOnGround, int ModelGroupIndex, CBaseEntity entityProp)
        {
            lock (_fileLock)
            {
                if (pos == null || angle == null) return null;
                int newId = _props.Count();

                var model = new PropModel
                {
                    Id = newId,
                    modelPath = ModelPath,
                    ModelGroupIndex = ModelGroupIndex,
                    posX = pos.X,
                    posY = pos.Y,
                    posZ = pos.Z,
                    angleX = angle.X,
                    angleY = angle.Y,
                    angleZ = angle.Z,
                    width = width,
                    height = height,
                    forceOnVip = forceToVip,
                    isOnGround = isOnGround,
                    depth = depth,
                    EntityProp = entityProp
                };
                _props.Add(model);

                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(_mapFilePath!, JsonSerializer.Serialize(_props, options));
                return model;
            }
        }

        public void LoadPropsFromMap()
        {
            if (File.Exists(_mapFilePath))
            {
                string json = File.ReadAllText(_mapFilePath);
                if (!string.IsNullOrEmpty(json))
                {
                    _props.Clear();
                    var loadedProps = JsonSerializer.Deserialize<List<PropModel>>(json) ?? [];
                    _props.AddRange(loadedProps);
                }
            }

        }

        public void SpawnProps()
        {
            foreach (var prop in _props)
            {
                if (_plugin.PluginUtils!.CheckMaterial(prop.modelPath!))
                {
                    var ent = _plugin.PluginUtils!.CreatePropModel(new Vector(prop.posX, prop.posY, prop.posZ), new QAngle(prop.angleX, prop.angleY, prop.angleZ), prop.modelPath!, prop.forceOnVip, prop.isOnGround ? true : false, prop.ModelGroupIndex, prop.Id);
                    if (ent != null)
                    {
                        prop.EntityProp = ent;
                    }
                }
                else
                {
                    var ent = _plugin.PluginUtils!.CreateDecal(new Vector(prop.posX, prop.posY, prop.posZ), new QAngle(prop.angleX, prop.angleY, prop.angleZ), prop.modelPath!, prop.width, prop.height, prop.forceOnVip, prop.depth);
                    if (ent != null)
                    {
                        prop.EntityProp = ent;
                    }
                }
            }
        }

        public PropModel? GetPropById(int id)
        {
            return id >= 0 && id < _props.Count ? _props[id] : null;
        }
        public void RemovePropFromFile(int id)
        {
            //int id = Convert.ToInt32(idstring);
            //if (id < 0 || id >= _props.Count()) return;
            _props.RemoveAt(id);

            for (int i = 0; i < _props.Count; i++)
            {
                _props[i].Id = i;
            }
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(_mapFilePath!, JsonSerializer.Serialize(_props, options));
        }

        public void SavePropConfiguration(CBaseEntity entity, PropModel prop)
        {
            if (entity == null || !entity.IsValid) return;
            var pos = entity.AbsOrigin;
            var ang = entity.AbsRotation;

            if (pos == null || ang == null) return;

            prop.posX = pos.X; prop.posY = pos.Y; prop.posZ = pos.Z;
            prop.angleX = ang.X; prop.angleY = ang.Y; prop.angleZ = ang.Z;

            if (!_plugin.PluginUtils!.CheckMaterial(prop.modelPath!))
            {
                var ent = entity.As<CEnvDecal>();
                prop.width = ent.Width;
                prop.height = ent.Height;
            }


            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(_mapFilePath!, JsonSerializer.Serialize(_props, options));
        }

        public void SaveAllAdverts()
        {
            foreach (var prop in _props)
            {
                var entity = prop.EntityProp;
                if (entity == null || !entity.IsValid)
                    continue;

                var pos = entity.AbsOrigin;
                var ang = entity.AbsRotation;

                if (pos == null || ang == null)
                    continue;

                prop.posX = pos.X;
                prop.posY = pos.Y;
                prop.posZ = pos.Z;

                prop.angleX = ang.X;
                prop.angleY = ang.Y;
                prop.angleZ = ang.Z;

                if (entity is CEnvDecal decal)
                {
                    prop.width = decal.Width;
                    prop.height = decal.Height;
                }
            }

            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(_mapFilePath!, JsonSerializer.Serialize(_props, options));
        }

        /*
        public CPhysicsPropOverride GetEntityProp(int id)
        {
            var allAdvs = Utilities.FindAllEntitiesByDesignerName<CPhysicsPropOverride>("prop_physics_override");
            CPhysicsPropOverride? lookedAdv = null;
            foreach(var adv in allAdvs)
            {
                var name = adv.Entity!.Name;
                if (name.Contains($"advert_prop{id}"))
                {
                    lookedAdv = adv;
                }
            }
            if(lookedAdv == null) return null!;
            return lookedAdv;
        }
        */

    }
}

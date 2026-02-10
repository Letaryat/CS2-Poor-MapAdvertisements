using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace CS2_Poor_MapDecals.Models
{
    public class PropModel
    {
        public int Id { get; set; }
        public string? modelPath { get; set; }
        public int ModelGroupIndex { get; set; }
        public float posX { get; set; }
        public float posY { get; set; }
        public float posZ { get; set; }
        public float angleX { get; set; }
        public float angleY { get; set; }
        public float angleZ { get; set; }
        public float width { get; set; }
        public float height { get; set; }
        public bool forceOnVip { get; set; }
        public bool isOnGround { get; set; }

        [JsonIgnore]
        public CPhysicsPropOverride? EntityProp { get; set; }

    }
}
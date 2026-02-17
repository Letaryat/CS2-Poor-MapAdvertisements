namespace CS2_Poor_MapAdvertisements.Models
{
    public class SelectedMaterialModel
    {
        public string? material { get; set; }
        public bool isVip { get;set; }
        public bool isOnGround { get;set; }
        public int materialIndex { get;set; }
        public float width { get;set; }
        public float height { get;set; }
        public int depth { get;set; }
        public bool onPing { get;set; }
    }
}
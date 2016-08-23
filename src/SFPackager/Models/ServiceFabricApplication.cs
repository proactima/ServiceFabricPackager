namespace SFPackager.Models
{
    public class ServiceFabricApplication
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string TypeName { get; set; }
        public string TypeVersion { get; set; }
        public int Status { get; set; }
        public int HealthState { get; set; }
    }
}
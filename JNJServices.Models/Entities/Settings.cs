using System.Text.Json.Serialization;

namespace JNJServices.Models.Entities
{
    public class Settings
    {
        public Settings()
        {
            InputOptionsList = new List<string>();
        }

        public int SettingID { get; set; }
        public string SettingLabel { get; set; } = string.Empty;
        public string SettingKey { get; set; } = string.Empty;
        public string SettingValue { get; set; } = string.Empty;
        public string InputType { get; set; } = string.Empty;
        [JsonIgnore]
        public string InputOptions { get; set; } = string.Empty;
        // This will convert the comma-separated string into a list of strings.
        public List<string> InputOptionsList { get; set; }
        public int DisplayOrder { get; set; }
        public bool InactiveFlag { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

using System.Text.Json.Serialization;

namespace ChatifyMobile
{
    public class User
    {
        [JsonPropertyName("sid")]
        public string Sid { get; set; }
        
        [JsonPropertyName("username")]
        public string Username { get; set; }
        
        [JsonPropertyName("public_key")]
        public string Public_Key { get; set; }
    }

    public class ChatMessage
    {
        public string Sender { get; set; }
        public string Content { get; set; }
        public string Time { get; set; }
    }
}

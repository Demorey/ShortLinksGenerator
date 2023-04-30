using MongoDB.Bson;

namespace ShortLinksGenerator;

public class UserRecord
{  
    public UserRecord(string userIp, string sourceLink, string shortLink)
    {
        
        UserIp = userIp;
        SourceLink = sourceLink;
        ShortLink = shortLink;
    }
    
    public ObjectId Id { get; set; }
    public string UserIp { get; set; }
    public string ShortLink { get; set; }
    public string SourceLink { get; set; }
    public int ClickCounter { get; set; } = 0;
}
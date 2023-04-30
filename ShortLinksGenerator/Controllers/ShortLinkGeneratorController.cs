using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using ShortLinksGenerator.Service;

namespace ShortLinksGenerator.Controllers;

[ApiController]
[Route("[controller]")]
public class ShortLinkGeneratorController : ControllerBase
{
    public static MongoClient client = new MongoClient("mongodb://localhost:27017");
    public static IMongoDatabase db = client.GetDatabase("UsersShortLinks");


    [HttpGet]
    [Route("/add")]
    public IActionResult AddShortLink()
    {
        // получаем URL-ссылку из заголовка
        string? url = Request.Query["url"];
        if (url is null)
        {
            return BadRequest("Url is empty");
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress!.ToString();
        var users = db.GetCollection<UserRecord>("users");
        var filterTemplate = Builders<UserRecord>.Filter;
        var filter = filterTemplate.And(filterTemplate.Eq("UserIp", ipAddress), filterTemplate.Eq("SourceLink", url) );
        var documents = users.Find(filter).ToList();
        
        if (documents.Count > 0)
        {
            
                return Ok($"Исходная ссылка:  {documents[0].SourceLink}\n" +
                          $"Сокращенная ссылка:  {documents[0].ShortLink}\n" +
                          $"Переходов по сокращенной ссылке:  {documents[0].ClickCounter}");
            
            
        }

        // создаем новую короткую ссылку
        var shortLink = GenShortLinks.GenShortLink();
        users.InsertOne(new UserRecord(ipAddress, url, shortLink));
        
        // возвращаем ответ с короткой ссылкой
        return Ok(shortLink);

    }


    [HttpGet]
    [Route("/{shortLink}")]
    public async Task<IActionResult> RedirectToSourseUrl(string shortLink)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress!.ToString();
        var users = db.GetCollection<UserRecord>("users");
        var filterTemplate = Builders<UserRecord>.Filter;
        var filter = filterTemplate.Eq("ShortLink", shortLink);
        var documents = users.Find(filter).ToList();

        if ( documents.Count > 0)
        {
            var updateClickCounter = new BsonDocument("$set", new BsonDocument("ClickCounter", documents[0].ClickCounter + 1));
            await users.UpdateOneAsync(filter, updateClickCounter);
            return Redirect(@"https://"+documents[0].SourceLink);
        }

        return Ok("Неверная ссылка!");
    }
    
    [HttpGet]
    [Route("/mylinks")]
    public IActionResult ViewAllLinks()
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress!.ToString();
        var users = db.GetCollection<UserRecord>("users");
        var filterTemplate = Builders<UserRecord>.Filter;
        var filter = filterTemplate.Eq("UserIp", ipAddress);
        var documents = users.Find(filter).ToList();

        if (documents.Count == 0) return BadRequest("У вас нет записей!");
        
        var userLinks = new List<string>{};
        userLinks.Add("Ваши ссылки:\n");
        foreach (var link in documents)
        {
            userLinks?.Add(($"Исходная ссылка:  {link.SourceLink}\n" +
                            $"Сокращенная ссылка:  {link.ShortLink}\n" +
                            $"Переходов по сокращенной ссылке:  {link.ClickCounter}"));
        }
            
        return Ok(string.Join("\n\n\n", userLinks!));

    }
}
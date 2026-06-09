using System.Text;
using DatabaseLibrary;
using DatabaseLibrary.Models;
using HtmlAgilityPack;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

string url = "https://4pda.to/forum/index.php?showtopic=1110954";
if (args.Length > 0) url = args[0];

Console.WriteLine($"Парсинг: {url}");

// Загрузка страницы
var web = new HtmlWeb();
web.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)";
var doc = web.Load(url);

// Ищем контейнеры постов – div с атрибутом data-post
var posts = doc.DocumentNode.SelectNodes("//div[@data-post]");
if (posts == null || posts.Count == 0)
{
    Console.WriteLine("Посты не найдены!");
    return;
}

Console.WriteLine($"Найдено постов: {posts.Count}");

string dbPath = Path.Combine(AppContext.BaseDirectory, "forum.db");
using var repo = new SqliteMessageRepository($"Data Source={dbPath}");

foreach (var post in posts)
{
    // 1. ID сообщения – берём из атрибута data-post
    string idStr = post.GetAttributeValue("data-post", "");
    if (!int.TryParse(idStr, out int msgId))
        continue;

    // 2. Автор – внутри span.post_nick лежит ссылка с именем
    var authorLink = post.SelectSingleNode(".//span[contains(@class,'post_nick')]//a");
    string author = authorLink?.InnerText.Trim() ?? "Unknown";

    // 3. Текст сообщения – div.post_body (InnerHtml сохраняет HTML-разметку)
    var msgNode = post.SelectSingleNode(".//div[contains(@class,'post_body')]");
    string message = msgNode?.InnerHtml.Trim() ?? "";

    Console.WriteLine($"ID={msgId}, Author='{author}', MsgLen={message.Length}");

    var record = new MessageRecord
    {
        Id = msgId,
        Name = author.Length > 256 ? author[..256] : author,
        Message = message.Length > 8096 ? message[..8096] : message
    };

    try
    {
        repo.Add(record);
        Console.WriteLine("   -> Сохранено");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   -> Ошибка: {ex.Message}");
    }
}
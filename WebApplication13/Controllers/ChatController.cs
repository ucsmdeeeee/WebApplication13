using Microsoft.AspNetCore.Mvc;
using WebApplication13.Models;
using System.Linq;

[Route("api/[controller]")]
[ApiController]
public class ChatController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ChatController> _logger;

    public ChatController(ApplicationDbContext context, ILogger<ChatController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Получить все чаты для пользователя с именами собеседников
    [HttpGet("get/{userId}")]
    public IActionResult GetChats(int userId)
    {
        var chats = _context.Chats
            .Where(c => c.User1Id == userId || c.User2Id == userId)
            .ToList();

        var chatList = new List<object>();

        foreach (var chat in chats)
        {
            var user1 = _context.Users.FirstOrDefault(u => u.Id == chat.User1Id);
            var user2 = _context.Users.FirstOrDefault(u => u.Id == chat.User2Id);

            var chatInfo = new
            {
                chat.Id,
                chat.User1Id,
                chat.User2Id,
                User1Username = user1 != null ? user1.Username : "Unknown",  // Если пользователя не найдено, возвращаем "Unknown"
                User2Username = user2 != null ? user2.Username : "Unknown"  // Если пользователя не найдено, возвращаем "Unknown"
            };

            chatList.Add(chatInfo);
        }

        return Ok(chatList);
    }

    // Создать чат между двумя пользователями
    [HttpPost("create")]
    public async Task<IActionResult> CreateChat([FromBody] CreateChatDto chatDto)
    {
        var existingChat = _context.Chats
            .FirstOrDefault(c =>
                (c.User1Id == chatDto.User1Id && c.User2Id == chatDto.User2Id) ||
                (c.User1Id == chatDto.User2Id && c.User2Id == chatDto.User1Id));

        if (existingChat != null)
        {
            return BadRequest(new { message = "Chat already exists between these users." });
        }

        var chat = new Chat
        {
            User1Id = chatDto.User1Id,
            User2Id = chatDto.User2Id
        };

        _context.Chats.Add(chat);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Chat created successfully!" });
    }

    // Отправить сообщение в чат
    [HttpPost("sendMessage")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageDto messageDto)
    {
        var message = new Message
        {
            ChatId = messageDto.ChatId,
            SenderId = messageDto.SenderId,
            ReceiverId = messageDto.ReceiverId,
            Content = messageDto.Content,
            Timestamp = DateTime.UtcNow
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        var sender = _context.Users.FirstOrDefault(u => u.Id == messageDto.SenderId)?.Username ?? "Unknown";
        var receiver = _context.Users.FirstOrDefault(u => u.Id == messageDto.ReceiverId)?.Username ?? "Unknown";

        return Ok(new { message = "Message sent successfully!", Sender = sender, Receiver = receiver });
    }
}

public class CreateChatDto
{
    public int User1Id { get; set; }
    public int User2Id { get; set; }
}

public class SendMessageDto
{
    public int ChatId { get; set; }
    public int SenderId { get; set; }
    public int ReceiverId { get; set; }
    public string Content { get; set; }
}




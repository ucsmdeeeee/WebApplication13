using Microsoft.AspNetCore.Mvc;
using WebApplication13.Models;

[Route("api/[controller]")]
[ApiController]
public class MessageController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MessageController> _logger;

    public MessageController(ApplicationDbContext context, ILogger<MessageController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Получить все сообщения для конкретного чата с именами пользователей
    [HttpGet("getMessages/{chatId}")]
    public IActionResult GetMessages(int chatId)
    {
        var messages = _context.Messages
            .Where(m => m.ChatId == chatId)
            .OrderBy(m => m.Timestamp)
            .Select(m => new
            {
                m.Id,
                m.ChatId,
                m.Content,
                m.Timestamp,
                Sender = _context.Users.Where(u => u.Id == m.SenderId).Select(u => u.Username).FirstOrDefault(),
                Receiver = _context.Users.Where(u => u.Id == m.ReceiverId).Select(u => u.Username).FirstOrDefault()
            })
            .ToList();

        // Обработка данных, если Sender или Receiver не найден
        var messageList = messages.Select(m => new
        {
            m.Id,
            m.ChatId,
            m.Content,
            m.Timestamp,
            Sender = string.IsNullOrEmpty(m.Sender) ? "Unknown" : m.Sender, // Если Sender пустой, выводим "Unknown"
            Receiver = string.IsNullOrEmpty(m.Receiver) ? "Unknown" : m.Receiver // Если Receiver пустой, выводим "Unknown"
        }).ToList();

        return Ok(messageList);
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

        return Ok(new { message = "Message sent successfully!" });
    }
}



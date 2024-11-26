using Microsoft.AspNetCore.Mvc;
using WebApplication13.Models;
using System.Globalization;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class LogController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public LogController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetLogs(
        [FromQuery] string? startDate,
        [FromQuery] string? endDate,
        [FromQuery] string? ip)
    {
        var logs = _context.LogEntries.AsQueryable();

        if (!string.IsNullOrEmpty(startDate) && DateTime.TryParse(startDate, out var start))
        {
            logs = logs.Where(l => l.Timestamp >= start);
        }

        if (!string.IsNullOrEmpty(endDate) && DateTime.TryParse(endDate, out var end))
        {
            logs = logs.Where(l => l.Timestamp <= end);
        }

        if (!string.IsNullOrEmpty(ip))
        {
            logs = logs.Where(l => l.IP == ip);
        }

        return Ok(await logs.ToListAsync());
    }

    [HttpPost("process")]
    public async Task<IActionResult> ProcessLogs()
    {
        var logSettings = new LogSettings
        {
            Directory = "path_to_logs", // Replace with configuration setting
            FileMask = "*.log"
        };

        var logFiles = Directory.GetFiles(logSettings.Directory, logSettings.FileMask);

        foreach (var file in logFiles)
        {
            var lines = await System.IO.File.ReadAllLinesAsync(file);

            foreach (var line in lines)
            {
                var logEntry = ParseApacheLog(line);
                if (logEntry != null)
                {
                    _context.LogEntries.Add(logEntry);
                }
            }
        }

        await _context.SaveChangesAsync();
        return Ok("Logs processed successfully.");
    }

    private LogEntry? ParseApacheLog(string logLine)
    {
        try
        {
            // Example parsing, adapt according to your log format
            var parts = logLine.Split(' ');
            return new LogEntry
            {
                IP = parts[0],
                Timestamp = DateTime.ParseExact(parts[3], "[dd/MMM/yyyy:HH:mm:ss]", CultureInfo.InvariantCulture),
                Url = parts[6],
                Status = parts[8],
                UserAgent = string.Join(' ', parts.Skip(9))
            };
        }
        catch
        {
            return null; // Return null if the log line is not parsable
        }
    }
}

public class LogSettings
{
    public string Directory { get; set; }
    public string FileMask { get; set; }
}

using System.Globalization;

namespace Masareef.Entities.DTOs;


public class BusinessListDto
{
    public int BusinessId { get; set; }
    public string BusinessName { get; set; } = null!;
    public string? Description { get; set; }
    public string Currency { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }

    // extra 
    public string StatusText => IsActive ? "Active" : "not Active";
    public string CreatedAtText =>
        CreatedAt.ToString("dd MMM yyyy", new CultureInfo("ar-EG"));

    public int DaysSinceCreated =>
    (int)(DateTime.UtcNow.Date - CreatedAt.Date).TotalDays;
}
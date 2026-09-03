namespace Masareef.Entities.DTOs;

public class CategoryDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public string? Icon { get; set; }
    public byte CategoryKind { get; set; }   // 1=Expense 2=Income
    public byte CategoryScope { get; set; }   // 1=Business 2=Home 3=Both
}
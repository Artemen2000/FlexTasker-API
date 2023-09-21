namespace FlexTasker.Models;

public class TodoItem
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsComplete { get; set; }
    public string? Description { get; set; }
    public bool IsStarred { get; set; }
    public int listId { get; set; }
}

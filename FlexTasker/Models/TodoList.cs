namespace FlexTasker.Models
{
	public enum Type { DEFAULT, USER }
	public class TodoList
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public long UserId { get; set; }
		public Type ListType { get; set;}
	}
}

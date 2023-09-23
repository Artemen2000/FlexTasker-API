using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlexTasker.Models;
using FlexTasker.Database;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace FlexTasker.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class TodoItemsController : ControllerBase
	{
		private readonly ApplicationContext _context;

		public TodoItemsController(ApplicationContext context)
		{
			_context = new ApplicationContext();
		}

		[SwaggerOperation(Summary = "Returns JSON with all tasks linked to the user of a token")]
		[Authorize]
		[HttpGet("/api/getTasks")]
		public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodoItems()
		{
			long userid = _context.users.SingleOrDefault(user => user.Name == User.Identity.Name).Id;
			return await _context.todoItems.FromSql($"SELECT * FROM \"todoItems\" WHERE \"listId\" in (SELECT \"Id\" from \"todoLists\" WHERE \"UserId\" = {userid}) ORDER BY \"Id\"").ToListAsync();
		}

		[SwaggerOperation(Summary = "Returns JSON with all tasks linked to the user of a token and marked with a star")]
		[Authorize]
		[HttpGet("/api/getStarredTasks")]
		public async Task<ActionResult<IEnumerable<TodoItem>>> GetStarredTodoItems()
		{
			long userid = _context.users.SingleOrDefault(user => user.Name == User.Identity.Name).Id;
			return await _context.todoItems.FromSql($"SELECT * FROM \"todoItems\" WHERE \"listId\" in (SELECT \"Id\" from \"todoLists\" WHERE \"UserId\" = {userid}) AND \"IsStarred\" = true").ToListAsync();
		}

		// GET: api/TodoItems/5
		// <snippet_GetByID>
		[SwaggerOperation(Summary = "Returns JSON with the task defined by id or 404 if task doesn't exist or isn't linked to the user defined by token")]
		[Authorize]
		[HttpGet("/api/getTask/{id}")]
		public async Task<ActionResult<TodoItem>> GetTodoItem(int id)
		{
			long userid = _context.users.SingleOrDefault(user => user.Name == User.Identity.Name).Id;
			var todoItem = await _context.todoItems.FromSql($"SELECT * FROM \"todoItems\" WHERE \"listId\" in (SELECT \"Id\" from \"todoLists\" WHERE \"UserId\" = {userid}) AND \"Id\" = {id}").ToListAsync();

			if (todoItem.Count == 0)
			{
				return NotFound();
			}

			return todoItem[0];
		}
		// </snippet_GetByID>


		// POST: api/TodoItems
		// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
		// <snippet_Create>
		[SwaggerOperation(Summary = "Creates task defined by body content. Returns JSON with the task if OK. Can return 404 if tasklist not found")]
		[Authorize]
		[HttpPost("/api/createTask")]
		public async Task<ActionResult<TodoItem>> PostTodoItem(TodoItem todoItem)
		{
			long userid = _context.users.SingleOrDefault(user => user.Name == User.Identity.Name).Id;
			var todoList = await _context.todoLists.FromSql($"SELECT * FROM \"todoLists\" WHERE \"UserId\" = {userid} AND \"Id\" = {todoItem.listId}").ToListAsync();

			if (todoList.Count == 0)
			{
				return NotFound();
			}

			_context.todoItems.Add(todoItem);
			await _context.SaveChangesAsync();

			//    return CreatedAtAction("GetTodoItem", new { id = todoItem.Id }, todoItem);
			return CreatedAtAction(nameof(GetTodoItem), new { id = todoItem.Id }, todoItem);
		}
		// </snippet_Create>


		// PUT: api/TodoItems/5
		// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
		// <snippet_Update>
		[SwaggerOperation(Summary = "Updates task defined by id and body content. Returns 204 if OK. Can return 404 or 400")]
		[Authorize]
		[HttpPut("/api/updateTask/{id}")]
		public async Task<IActionResult> PutTodoItem(long id, TodoItem todoItem)
		{
			if (id != todoItem.Id)
			{
				return BadRequest();
			}

			long userid = _context.users.SingleOrDefault(user => user.Name == User.Identity.Name).Id;
			var todoList = await _context.todoLists.FromSql($"SELECT * FROM \"todoLists\" WHERE \"UserId\" = {userid} AND \"Id\" = {todoItem.listId}").ToListAsync();

			if (todoList.Count == 0)
			{
				return NotFound();
			}

			_context.Entry(todoItem).State = EntityState.Modified;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!TodoItemExists(id))
				{
					return NotFound();
				}
				else
				{
					throw;
				}
			}

			return NoContent();
		}
		// </snippet_Update>


		// DELETE: api/TodoItems/5
		[SwaggerOperation(Summary = "Deletes task defined by id. Returns 204 if OK. Can return 404 if task isn't accessible")]
		[Authorize]
		[HttpDelete("/api/deleteTask/{id}")]
		public async Task<IActionResult> DeleteTodoItem(long id)
		{
			long userid = _context.users.SingleOrDefault(user => user.Name == User.Identity.Name).Id;
			var todoList = await _context.todoLists.FromSql($"SELECT * FROM \"todoLists\" WHERE \"UserId\" = {userid} AND \"Id\" in (SELECT \"listId\" from \"todoItems\" WHERE \"Id\" = {(int)id})").ToListAsync();
			if (todoList.Count == 0)
			{
				return NotFound();
			}

			var todoItem = await _context.todoItems.FindAsync((int)id);
			if (todoItem == null)
			{
				return NotFound();
			}

			_context.todoItems.Remove(todoItem);
			await _context.SaveChangesAsync();

			return NoContent();
		}

		private bool TodoItemExists(long id)
		{
			return _context.todoItems.Any(e => e.Id == id);
		}
	}
}

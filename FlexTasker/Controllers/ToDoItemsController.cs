using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlexTasker.Models;
using FlexTasker.Database;
using Microsoft.AspNetCore.Authorization;

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

		// GET: api/TodoItems
		[Authorize]
		[HttpGet]
		public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodoItems()
		{

			return await _context.todoItems.ToListAsync();
		}

		// GET: api/TodoItems/5
		// <snippet_GetByID>
		[HttpGet("{id}")]
		public async Task<ActionResult<TodoItem>> GetTodoItem(int id)
		{
			var todoItem = await _context.todoItems.FindAsync(id);

			if (todoItem == null)
			{
				return NotFound();
			}

			return todoItem;
		}
		// </snippet_GetByID>

		// PUT: api/TodoItems/5
		// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
		// <snippet_Update>
		[HttpPut("{id}")]
		public async Task<IActionResult> PutTodoItem(long id, TodoItem todoItem)
		{
			if (id != todoItem.Id)
			{
				return BadRequest();
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

		// POST: api/TodoItems
		// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
		// <snippet_Create>
		[HttpPost]
		public async Task<ActionResult<TodoItem>> PostTodoItem(TodoItem todoItem)
		{
			_context.todoItems.Add(todoItem);
			await _context.SaveChangesAsync();

			//    return CreatedAtAction("GetTodoItem", new { id = todoItem.Id }, todoItem);
			return CreatedAtAction(nameof(GetTodoItem), new { id = todoItem.Id }, todoItem);
		}
		// </snippet_Create>

		// DELETE: api/TodoItems/5
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteTodoItem(long id)
		{
			var todoItem = await _context.todoItems.FindAsync(id);
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

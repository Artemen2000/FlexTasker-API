using FlexTasker.Database;
using FlexTasker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace FlexTasker.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class TodoListController : ControllerBase
	{
		private readonly ApplicationContext _context;

		public TodoListController(ApplicationContext context)
		{
			_context = new ApplicationContext();
		}


		[Authorize]
		[HttpGet("/api/getTaskLists")]
		public async Task<ActionResult<IEnumerable<TodoList>>> GetTodoLists()
		{
			string username = User.Identity.Name;
			long userid = _context.users.SingleOrDefault(user => user.Name == username).Id;
			return await _context.todoLists.FromSql($"SELECT * FROM \"todoLists\" WHERE \"UserId\" = {userid }").ToListAsync();
		}

		[Authorize]
		[HttpGet("/api/getTaskLists/{id}")]
		public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodoList(long id)
		{
			string username = User.Identity.Name;
			long userid = _context.users.SingleOrDefault(user => user.Name == username).Id;
			List<TodoList> list = await _context.todoLists.FromSql($"SELECT * FROM \"todoLists\" WHERE \"UserId\" = {userid} AND \"Id\" = {(int)id}").ToListAsync();
			if (list.Count == 0) return NotFound();
			return await _context.todoItems.FromSql($"SELECT * FROM \"todoItems\" WHERE \"listId\" = {(int)id}").ToListAsync();
		}

		[Authorize]
		[HttpPost("/api/createTaskList")]
		public async Task<ActionResult<TodoList>> PostTodoList(TodoList todoList)
		{
			_context.todoLists.Add(todoList);
			await _context.SaveChangesAsync();
			return CreatedAtAction(nameof(GetTodoList), new { id = todoList.Id }, todoList);
		}

		[Authorize]
		[HttpPut("/api/updateTaskList/{id}")]
		public async Task<IActionResult> PutTodoList(long id, TodoList todoList)
		{
			if (id != todoList.Id)
				return BadRequest();

			_context.Entry(todoList).State = EntityState.Modified;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!TodoListExists(id))
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

		[Authorize]
		[HttpDelete("/api/deleteTaskList/{id}")]
		public async Task<IActionResult> DeleteTodoList(long id)
		{
			var todoList = await _context.todoLists.FindAsync(id);
			if (todoList == null)
				return NotFound();

			_context.todoLists.Remove(todoList);
			await _context.SaveChangesAsync();

			return NoContent();
		}

		private bool TodoListExists(long id)
		{
			return _context.todoLists.Any(e => e.Id == id);
		}
	}
}

using FlexTasker.Database;
using FlexTasker.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.Collections.Generic;

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
		[HttpGet("/api/getTaskList/{id}")]
		public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodoList(long id)
		{
			string username = User.Identity.Name;
			long userid = _context.users.SingleOrDefault(user => user.Name == username).Id;
			return await _context.todoItems.FromSql($"SELECT * FROM \"todoItems\" WHERE \"listId\" = (SELECT \"Id\" FROM \"todoLists\" WHERE \"UserId\" = {userid} AND \"Id\" = {id}) ORDER BY \"Id\"").ToListAsync();
		}

		[Authorize]
		[HttpPost("/api/createTaskList")]
		public async Task<ActionResult<TodoList>> PostTodoList(string? name, TodoList? todoList)
		{
			if (name == null && todoList == null) return BadRequest();
			long userid = _context.users.SingleOrDefault(user => user.Name == User.Identity.Name).Id;
			TodoList todoList1 = new TodoList { Id = 0, Name = (name != null) ? name : todoList.Name, UserId = userid, ListType = Models.Type.USER };
			_context.todoLists.Add(todoList1);
			await _context.SaveChangesAsync();
			return CreatedAtAction(nameof(GetTodoList), new { id = todoList1.Id }, todoList1);
		}

		[Authorize]
		[HttpPut("/api/updateTaskList/{id}")]
		public async Task<IActionResult> PutTodoList(long id, string? name, TodoList? todoList)
		{
			if (todoList == null)
				return BadRequest();

			var orig = _context.todoLists.SingleOrDefault(list => list.Id == id);
			_context.Entry(orig).State = EntityState.Detached;
			long userid = _context.users.SingleOrDefault(user => user.Name == User.Identity.Name).Id;
			
			if (todoList == null)
			{
				todoList = new TodoList { Id = (int)id, Name = name, UserId = userid,ListType = orig.ListType };
			}

			todoList.Id = (int)id;
			todoList.ListType = orig.ListType;
			todoList.UserId = orig.UserId;

			if (userid != orig.UserId) return NotFound();

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
			long userId = _context.users.SingleOrDefault(user => user.Name == User.Identity.Name).Id;
			var todoList = await _context.todoLists.FindAsync((int)id);

			if (todoList == null)
				return NotFound();
			if (todoList.UserId != userId)
				return NotFound();
			if (todoList.ListType == Models.Type.DEFAULT)
				return Forbid();

			var todoItems = await _context.todoItems.FromSql($"SELECT * FROM \"todoItems\" WHERE \"listId\" = {id}").ToListAsync();
			_context.todoItems.RemoveRange(todoItems);
			_context.todoLists.Remove(todoList);
			await _context.SaveChangesAsync();

			return NoContent();
		}

		private bool TodoListExists(long id)
		{
			return _context.todoLists.Any(e => e.Id == id);
		}
		private bool BelongsTo(long listId, long userId)
		{
			List<TodoList> list = _context.todoLists.FromSql($"SELECT * FROM \"todoLists\" WHERE \"UserId\" = {userId} AND \"Id\" = {(int)listId}").ToList();
			if (list.Count == 0) return false;
			return true;
		}
	}
}

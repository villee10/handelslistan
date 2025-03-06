using Microsoft.AspNetCore.Mvc;
using Npgsql;
using server.Models;
using System.Collections.Generic;

namespace server.Controllers
{
    [ApiController]
    [Route("api/shoppinglist")]
    public class ShoppingListController : ControllerBase
    {
        private readonly string _connectionString;

        public ShoppingListController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");

        }

        [HttpGet("{userId}")]
        public ActionResult<IEnumerable<ShoppingList>> GetLists(int userId)
        {
            var lists = new List<ShoppingList>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(
                    "SELECT id, name, owner_id FROM shopping_lists WHERE owner_id = @userId", 
                    connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lists.Add(new ShoppingList
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                OwnerId = reader.GetInt32(2)
                            });
                        }
                    }
                }
            }

            return Ok(lists);
        }

        [HttpPost]
        public ActionResult<ShoppingList> CreateList([FromBody] ShoppingList list)
        {
            int newListId;
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(
                    "INSERT INTO shopping_lists (name, owner_id) VALUES (@name, @ownerId) RETURNING id", 
                    connection))
                {
                    command.Parameters.AddWithValue("@name", list.Name);
                    command.Parameters.AddWithValue("@ownerId", list.OwnerId);

                    newListId = Convert.ToInt32(command.ExecuteScalar());
                }
            }

            list.Id = newListId;
            return CreatedAtAction(nameof(GetList), new { id = newListId }, list);
        }

        [HttpGet("details/{id}")]
        public ActionResult<ShoppingList> GetList(int id)
        {
            ShoppingList list = null;

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(
                    "SELECT id, name, owner_id FROM shopping_lists WHERE id = @id", 
                    connection))
                {
                    command.Parameters.AddWithValue("@id", id);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            list = new ShoppingList
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                OwnerId = reader.GetInt32(2)
                            };
                        }
                    }
                }
            }

            return list != null ? Ok(list) : NotFound();
        }

        [HttpPut("{id}")]
        public IActionResult UpdateList(int id, [FromBody] ShoppingList list)
        {
            if (id != list.Id)
            {
                return BadRequest();
            }

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(
                    "UPDATE shopping_lists SET name = @name WHERE id = @id", 
                    connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@name", list.Name);

                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0 ? NoContent() : NotFound();
                }
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteList(int id)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("DELETE FROM shopping_lists WHERE id = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0 ? NoContent() : NotFound();
                }
            }
        }
    }
}
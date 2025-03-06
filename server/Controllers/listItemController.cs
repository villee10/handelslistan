using Microsoft.AspNetCore.Mvc;
using Npgsql;
using server.Models;
using System.Collections.Generic;

namespace server.Controllers
{
    [ApiController]
    [Route("api/shoppinglist")]
    public class ListItemsController : ControllerBase
    {
        private readonly string _connectionString;

        public ListItemsController(IConfiguration configuration)
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
    }
}
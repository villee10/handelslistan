using Microsoft.AspNetCore.Mvc;
using Npgsql;
using server.Models;

namespace server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShoppingListsController : ControllerBase
    {
        private readonly string _connectionString;

        public ShoppingListsController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // GET: api/ShoppingLists
        [HttpGet]
        public ActionResult<IEnumerable<shopping_list>> GetShoppingLists()
        {
            var shoppingLists = new List<shopping_list>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("SELECT id, name, owner_id FROM shopping_lists", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            shoppingLists.Add(new shopping_list
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                OwnerId = reader.GetInt32(2)
                            });
                        }
                    }
                }
            }

            return Ok(shoppingLists);
        }

        // GET: api/ShoppingLists/5
        [HttpGet("{id}")]
        public ActionResult<shopping_list> GetShoppingList(int id)
        {
            shopping_list shoppingList = null;

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("SELECT id, name, owner_id FROM shopping_lists WHERE id = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", id);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            shoppingList = new shopping_list
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                OwnerId = reader.GetInt32(2)
                            };
                        }
                    }
                }
            }

            return shoppingList != null ? Ok(shoppingList) : NotFound();
        }

        // POST: api/ShoppingLists
        [HttpPost]
        public ActionResult<shopping_list> PostShoppingList([FromBody] shopping_list shoppingList)
        {
            // Kontrollera att ägaren existerar
            if (!UserExists(shoppingList.OwnerId))
            {
                return BadRequest("Användaren existerar inte.");
            }

            int newListId;
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(
                    "INSERT INTO shopping_lists (name, owner_id) VALUES (@name, @owner_id) RETURNING id", 
                    connection))
                {
                    command.Parameters.AddWithValue("@name", shoppingList.Name);
                    command.Parameters.AddWithValue("@owner_id", shoppingList.OwnerId);

                    newListId = Convert.ToInt32(command.ExecuteScalar());
                }
            }

            shoppingList.Id = newListId;
            return CreatedAtAction(nameof(GetShoppingList), new { id = newListId }, shoppingList);
        }

        // PUT: api/ShoppingLists/5
        [HttpPut("{id}")]
        public IActionResult PutShoppingList(int id, [FromBody] shopping_list shoppingList)
        {
            if (id != shoppingList.Id)
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
                    command.Parameters.AddWithValue("@name", shoppingList.Name);

                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0 ? NoContent() : NotFound();
                }
            }
        }

        // DELETE: api/ShoppingLists/5
        [HttpDelete("{id}")]
        public IActionResult DeleteShoppingList(int id)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                
                // Ta först bort alla listartiklar
                using (var deleteItemsCommand = new NpgsqlCommand(
                    "DELETE FROM list_items WHERE list_id = @id", 
                    connection))
                {
                    deleteItemsCommand.Parameters.AddWithValue("@id", id);
                    deleteItemsCommand.ExecuteNonQuery();
                }

                // Ta sedan bort själva listan
                using (var deleteListCommand = new NpgsqlCommand(
                    "DELETE FROM shopping_lists WHERE id = @id", 
                    connection))
                {
                    deleteListCommand.Parameters.AddWithValue("@id", id);
                    int rowsAffected = deleteListCommand.ExecuteNonQuery();
                    return rowsAffected > 0 ? NoContent() : NotFound();
                }
            }
        }

        // Hjälpmetod för att kontrollera användarens existens
        private bool UserExists(int userId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("SELECT COUNT(*) FROM users WHERE id = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", userId);
                    return Convert.ToInt32(command.ExecuteScalar()) > 0;
                }
            }
        }
    }
}
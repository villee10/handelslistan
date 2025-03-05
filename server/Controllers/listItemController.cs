using Microsoft.AspNetCore.Mvc;
using Npgsql;
using server.Models;

namespace server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ListItemsController : ControllerBase
    {
        private readonly string _connectionString;

        public ListItemsController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // GET: api/ListItems
        [HttpGet]
        public ActionResult<IEnumerable<list_item>> GetListItems()
        {
            var listItems = new List<list_item>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("SELECT id, list_id, item_name, quantity, checked FROM list_items", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            listItems.Add(new list_item
                            {
                                Id = reader.GetInt32(0),
                                ListId = reader.GetInt32(1),
                                ItemName = reader.GetString(2),
                                Quantity = reader.GetInt32(3),
                                Checked = reader.GetBoolean(4)
                            });
                        }
                    }
                }
            }

            return Ok(listItems);
        }

        // GET: api/ListItems/5
        [HttpGet("{id}")]
        public ActionResult<list_item> GetListItem(int id)
        {
            list_item listItem = null;

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("SELECT id, list_id, item_name, quantity, checked FROM list_items WHERE id = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", id);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            listItem = new list_item
                            {
                                Id = reader.GetInt32(0),
                                ListId = reader.GetInt32(1),
                                ItemName = reader.GetString(2),
                                Quantity = reader.GetInt32(3),
                                Checked = reader.GetBoolean(4)
                            };
                        }
                    }
                }
            }

            return listItem != null ? Ok(listItem) : NotFound();
        }

        // GET: api/ListItems/List/5
        [HttpGet("List/{listId}")]
        public ActionResult<IEnumerable<list_item>> GetListItemsByListId(int listId)
        {
            // Kontrollera om listan existerar
            if (!ShoppingListExists(listId))
            {
                return NotFound("Shoppinglistan hittades inte.");
            }

            var listItems = new List<list_item>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("SELECT id, list_id, item_name, quantity, checked FROM list_items WHERE list_id = @list_id", connection))
                {
                    command.Parameters.AddWithValue("@list_id", listId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            listItems.Add(new list_item
                            {
                                Id = reader.GetInt32(0),
                                ListId = reader.GetInt32(1),
                                ItemName = reader.GetString(2),
                                Quantity = reader.GetInt32(3),
                                Checked = reader.GetBoolean(4)
                            });
                        }
                    }
                }
            }

            return Ok(listItems);
        }

        // POST: api/ListItems
        [HttpPost]
        public ActionResult<list_item> CreateListItem([FromBody] list_item listItem)
        {
            // Kontrollera om listan existerar
            if (!ShoppingListExists(listItem.ListId))
            {
                return BadRequest("Shoppinglistan hittades inte.");
            }

            int newItemId;
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(
                    "INSERT INTO list_items (list_id, item_name, quantity, checked) VALUES (@list_id, @item_name, @quantity, @checked) RETURNING id", 
                    connection))
                {
                    command.Parameters.AddWithValue("@list_id", listItem.ListId);
                    command.Parameters.AddWithValue("@item_name", listItem.ItemName);
                    command.Parameters.AddWithValue("@quantity", listItem.Quantity);
                    command.Parameters.AddWithValue("@checked", listItem.Checked);

                    newItemId = Convert.ToInt32(command.ExecuteScalar());
                }
            }

            listItem.Id = newItemId;
            return CreatedAtAction(nameof(GetListItem), new { id = newItemId }, listItem);
        }

        // PUT: api/ListItems/5
        [HttpPut("{id}")]
        public IActionResult UpdateListItem(int id, [FromBody] list_item listItem)
        {
            if (id != listItem.Id)
            {
                return BadRequest();
            }

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(
                    "UPDATE list_items SET item_name = @item_name, quantity = @quantity, checked = @checked WHERE id = @id", 
                    connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@item_name", listItem.ItemName);
                    command.Parameters.AddWithValue("@quantity", listItem.Quantity);
                    command.Parameters.AddWithValue("@checked", listItem.Checked);

                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0 ? NoContent() : NotFound();
                }
            }
        }

        // PUT: api/ListItems/5/check
        [HttpPut("{id}/check")]
        public IActionResult ToggleChecked(int id)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(
                    "UPDATE list_items SET checked = NOT checked WHERE id = @id RETURNING checked", 
                    connection))
                {
                    command.Parameters.AddWithValue("@id", id);

                    var checkedStatus = command.ExecuteScalar();
                    return checkedStatus != null ? Ok(checkedStatus) : NotFound();
                }
            }
        }

        // DELETE: api/ListItems/5
        [HttpDelete("{id}")]
        public IActionResult DeleteListItem(int id)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("DELETE FROM list_items WHERE id = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0 ? NoContent() : NotFound();
                }
            }
        }

        // DELETE: api/ListItems/List/5
        [HttpDelete("List/{listId}")]
        public IActionResult DeleteAllItemsInList(int listId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("DELETE FROM list_items WHERE list_id = @list_id", connection))
                {
                    command.Parameters.AddWithValue("@list_id", listId);
                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0 ? NoContent() : NotFound();
                }
            }
        }

        // Hjälpmetod för att kontrollera listans existens
        private bool ShoppingListExists(int listId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("SELECT COUNT(*) FROM shopping_lists WHERE id = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", listId);
                    return Convert.ToInt32(command.ExecuteScalar()) > 0;
                }
            }
        }
    }
}
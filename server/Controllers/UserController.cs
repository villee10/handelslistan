using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Data;
using server.Models;
using System.Security.Cryptography;
using System.Text;

namespace server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly string _connectionString;

        public UsersController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // GET: api/Users
        [HttpGet]
        public ActionResult<IEnumerable<users>> GetUsers()
        {
            var usersList = new List<users>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("SELECT id, username, email FROM users", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            usersList.Add(new users
                            {
                                Id = reader.GetInt32(0),
                                Username = reader.GetString(1),
                                Email = reader.GetString(2)
                            });
                        }
                    }
                }
            }

            return Ok(usersList);
        }

        // POST: api/Users/register
        [HttpPost("register")]
        public ActionResult<users> Register([FromBody] UserRegistrationDTO model)
        {
            // Kontrollera om användarnamnet redan finns
            if (UserExistsByUsername(model.Username))
            {
                return BadRequest("Användarnamnet är redan taget.");
            }

            // Kontrollera om e-postadressen redan finns
            if (UserExistsByEmail(model.Email))
            {
                return BadRequest("E-postadressen är redan registrerad.");
            }

            // Skapa hash av lösenordet
            string passwordHash = HashPassword(model.Password);

            int newUserId;
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(
                    "INSERT INTO users (username, email, password_hash) VALUES (@username, @email, @password) RETURNING id", 
                    connection))
                {
                    command.Parameters.AddWithValue("@username", model.Username);
                    command.Parameters.AddWithValue("@email", model.Email);
                    command.Parameters.AddWithValue("@password", passwordHash);

                    newUserId = Convert.ToInt32(command.ExecuteScalar());
                }
            }

            // Returnera användarinformation utan lösenord
            return CreatedAtAction(nameof(GetUser), new { id = newUserId }, new
            {
                Id = newUserId,
                Username = model.Username,
                Email = model.Email
            });
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public ActionResult<users> GetUser(int id)
        {
            users user = null;

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("SELECT id, username, email FROM users WHERE id = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new users
                            {
                                Id = reader.GetInt32(0),
                                Username = reader.GetString(1),
                                Email = reader.GetString(2)
                            };
                        }
                    }
                }
            }

            return user != null ? Ok(user) : NotFound();
        }

        // Hjälpmetoder
        private bool UserExistsByUsername(string username)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("SELECT COUNT(*) FROM users WHERE username = @username", connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    return Convert.ToInt32(command.ExecuteScalar()) > 0;
                }
            }
        }

        private bool UserExistsByEmail(string email)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("SELECT COUNT(*) FROM users WHERE email = @email", connection))
                {
                    command.Parameters.AddWithValue("@email", email);
                    return Convert.ToInt32(command.ExecuteScalar()) > 0;
                }
            }
        }

        // Lösenordshantering (från tidigare implementation)
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // DTOs och andra metoder kan vara kvar som tidigare
    }

    // DTOs för att hantera indata (behålls oförändrade)
    public class UserRegistrationDTO
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class UserLoginDTO
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class UserUpdateDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
using WebAppAuthenticationServer.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;


namespace WebAppAuthenticationServer.Services;

public class UserServices
{
    private readonly IMongoCollection<User> _users;

    public UserServices(IOptions<WebAppDatabaseSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        var database = client.GetDatabase(settings.Value.DatabaseName);

        _users = database.GetCollection<User>(settings.Value.UserCollectionName);
    }

    // get all users
    public async Task<List<User>> GetAsync() =>
    await _users.Find(user => true).ToListAsync();

    // get a user by id
    public async Task<User?> GetAsync(string id) =>
    await _users.Find<User>(user => user.Id == id).FirstOrDefaultAsync();

    // get a user by username
    public async Task<User?> GetAsyncByUsername(string username) =>
    await _users.Find<User>(user => user.Username == username).FirstOrDefaultAsync();

    // get a user by email
    public async Task<User?> GetAsyncByEmail(string email) =>
    await _users.Find<User>(user => user.Email == email).FirstOrDefaultAsync();

    // create a user
    public async Task<User?> CreateAsync(User user)
    {
        await _users.InsertOneAsync(user);
        return user;
    }

    // update a user
    public async Task UpdateAsync(string id, User userIn) =>
    await _users.ReplaceOneAsync(user => user.Id == id, userIn);

    // remove a user
    public async Task RemoveAsync(User userIn) =>
    await _users.DeleteOneAsync(user => user.Id == userIn.Id);


    // user exists by id
    public async Task<bool> UserExistsByIdAsync(string id) =>
    await _users.Find<User>(user => user.Id == id).AnyAsync();

    // userExists by username and email
    public async Task<bool> UserExistsAsync(string username, string email)
    {
        var userByEmail = await _users.Find<User>(user => user.Email == email).FirstOrDefaultAsync();
        var userByUsername = await _users.Find<User>(user => user.Username == username).FirstOrDefaultAsync();

        if (userByEmail == null && userByUsername == null)
            return false;
        return true;
    }


    // user exists by username
    public async Task<bool> UserExistsAsyncByUsername(string username) =>
    await _users.Find<User>(user => user.Username == username).AnyAsync();

    // user exists by email
    public async Task<bool> UserExistsAsyncByEmail(string email) =>
    await _users.Find<User>(user => user.Email == email).AnyAsync();
}


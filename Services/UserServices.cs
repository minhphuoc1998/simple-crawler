using Microsoft.Extensions.Options;
using MongoDB.Driver;
using WebApi.Models;

namespace WebApi.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _userCollection;
        public UserService(
            IOptions<UserDatabaseSettings> settings)
        {
            var mongoClient = new MongoClient(settings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(settings.Value.DatabaseName);
            _userCollection = mongoDatabase.GetCollection<User>(settings.Value.CollectionName);
        }
        public async Task<User?> FindOne(string username) {
            return await _userCollection.Find(user => user.Username == username).FirstOrDefaultAsync();
        }
        public async Task<User> Create(User user) {
            await _userCollection.InsertOneAsync(user);
            return user;
        }
    }
}

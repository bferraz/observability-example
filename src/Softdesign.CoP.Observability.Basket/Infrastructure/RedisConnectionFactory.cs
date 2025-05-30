using StackExchange.Redis;

namespace Softdesign.CoP.Observability.Basket.Infrastructure
{
    public interface IRedisConnectionFactory
    {
        ConnectionMultiplexer GetConnection();
    }

    public class RedisConnectionFactory : IRedisConnectionFactory
    {
        private readonly string _connectionString;
        private ConnectionMultiplexer? _connection;

        public RedisConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public ConnectionMultiplexer GetConnection()
        {
            if (_connection == null || !_connection.IsConnected)
            {
                _connection = ConnectionMultiplexer.Connect(_connectionString);
            }
            return _connection;
        }
    }
}

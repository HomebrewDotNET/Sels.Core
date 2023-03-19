using Sels.Core.Conversion.Attributes.KeyValue;
using Sels.Core.Conversion.Serializers.KeyValue;

namespace Sels.Core.Data.MySQL.Models
{
    /// <summary>
    /// Strongly typed MySql connection string that can be parsed from a string or converted to connection string.
    /// </summary>
    public class ConnectionString
    {
        // Statics 
        private static KeyValueSerializer _serializer = new KeyValueSerializer(x => x.SplitAndJoinRowsUsing(';')
                                                                                        .ConvertKeyValuePairUsing('='));

        /// <summary>
        /// The servers to connection to.
        /// </summary>
        [Key("Server")]
        public List<string> Servers { get; set; }
        /// <summary>
        /// The name of the database to connect to.
        /// </summary>
        public string? Database { get; set; }
        /// <summary>
        /// The tcp port to connect to.
        /// </summary>
        public int? Port { get; set; }
        /// <summary>
        /// The name of the user to connect with.
        /// </summary>
        [Key("Uid")]
        public string? Username { get; set; }
        /// <summary>
        /// The password to connect with.
        /// </summary>
        [Key("Pwd")]
        public string? Password { get; set; }
        /// <summary>
        /// The ssl mode for the connection.
        /// </summary>
        public MySqlSslMode? SslMode { get; set; }
        /// <summary>
        /// Allows the usage of variables in queries.
        /// </summary>
        [Key("Allow User Variables")]
        public bool AllowUserVariables { get; set; }

        /// <inheritdoc/>
        public override string? ToString()
        {
            return _serializer.Serialize(this);
        }
        /// <summary>
        /// Parses <paramref name="connectionString"/> and turns it into an instance of <see cref="ConnectionString"/>.
        /// </summary>
        /// <param name="connectionString">The connection string to parse</param>
        /// <returns>Instance parsed from <paramref name="connectionString"/></returns>
        public static ConnectionString Parse(string? connectionString)
        {
            if (connectionString.HasValue())
            {
                return _serializer.Deserialize<ConnectionString>(connectionString);
            }
            return new ConnectionString();
        }
    }
    /// <summary>
    /// The ssl modes that MySql supports.
    /// </summary>
    public enum MySqlSslMode
    {
        /// <summary>
        /// Use ssl if the server supports it.
        /// </summary>
        Preferred,
        /// <summary>
        /// Force encryption. Denies connection if the server doesn't support ssl.
        /// </summary>
        Required
    }
}

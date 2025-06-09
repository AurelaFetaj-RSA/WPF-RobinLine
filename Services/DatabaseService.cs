using System.Data.OleDb;

namespace WPF_RobinLine.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;
        private readonly string _tableName;
        private readonly string _codeField;
        private readonly List<string> _tableFields;

        public DatabaseService(string connectionString,
                             string tableName = "model_name",
                             string codeField = "Codice",
                             List<string> tableFields = null)
        {
            _connectionString = connectionString;
            _tableName = tableName;
            _codeField = codeField;
            _tableFields = tableFields ?? new List<string>
            {
                "lav_08", "au1_08", "au2_08", // Robot 1 fields
                "lav_09", "au1_09", "au2_09" // Robot 2 fields
            };
        }

        public void AddModelNameRecord(string modelName, List<string> flags)
        {
            if (flags.Count != _tableFields.Count)
                throw new ArgumentException("Flags count must match table fields count");

            using (var conn = new OleDbConnection(_connectionString))
            {
                conn.Open();

                // Build the parameterized query
                var columns = $"{_codeField},{string.Join(",", _tableFields)}";
                var parameters = $"@modelName,{string.Join(",", _tableFields.Select((_, i) => $"@p{i}"))}";

                string query = $"INSERT INTO {_tableName} ({columns}) VALUES ({parameters})";

                using (var cmd = new OleDbCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@modelName", modelName);

                    for (int i = 0; i < _tableFields.Count; i++)
                    {
                        cmd.Parameters.AddWithValue($"@p{i}", flags[i]);
                    }

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<string> GetModelNameRecord(string modelName)
        {
            var result = new List<string>();

            using (var conn = new OleDbConnection(_connectionString))
            {
                conn.Open();
                string query = $"SELECT * FROM {_tableName} WHERE {_codeField} = ?";

                using (var cmd = new OleDbCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@modelName", modelName);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            foreach (var field in _tableFields)
                            {
                                result.Add(reader[field]?.ToString() ?? string.Empty);
                            }
                        }
                    }
                }
            }
            return result;
        }

        public void DeleteModelNameRecord(string modelName)
        {
            using (var conn = new OleDbConnection(_connectionString))
            {
                conn.Open();
                string query = $"DELETE FROM {_tableName} WHERE {_codeField} = ?";

                using (var cmd = new OleDbCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@modelName", modelName);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<string> GetAllModelNames()
        {
            var modelNames = new List<string>();

            using (var conn = new OleDbConnection(_connectionString))
            {
                conn.Open();
                string query = $"SELECT {_codeField} FROM {_tableName} ORDER BY {_codeField}";

                using (var cmd = new OleDbCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            modelNames.Add(reader[_codeField].ToString());
                        }
                    }
                }
            }

            return modelNames;
        }

        public void UpdateModelNameRecord(string modelName, List<string> flags)
        {
            if (flags.Count != _tableFields.Count)
                throw new ArgumentException($"Expected {_tableFields.Count} flags, got {flags.Count}");

            using (var conn = new OleDbConnection(_connectionString))
            {
                conn.Open();

                // Build the SET clause for the update
                var setClauses = new List<string>();
                for (int i = 0; i < _tableFields.Count; i++)
                {
                    setClauses.Add($"{_tableFields[i]} = ?");
                }
                string setClause = string.Join(", ", setClauses);

                string query = $"UPDATE {_tableName} SET {setClause} WHERE {_codeField} = ?";

                using (var cmd = new OleDbCommand(query, conn))
                {
                    // Add parameters for the SET values
                    for (int i = 0; i < flags.Count; i++)
                    {
                        cmd.Parameters.Add(new OleDbParameter { Value = flags[i] });
                    }

                    // Add parameter for the WHERE clause 
                    cmd.Parameters.Add(new OleDbParameter { Value = modelName });

                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}

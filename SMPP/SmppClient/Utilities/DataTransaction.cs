#region Namespaces

using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient.Server;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Xml;

#endregion

namespace ArdanStudios.Common.Data
{
    /// <summary> Abstracts a database transaction </summary>
    public class DataTransaction : IDisposable
    {
        #region Private Properties

        /// <summary> The maximum value for a money type in SQL </summary>
        private const decimal SQL_MONEY_MAX_VALUE = 922337203685477.5807M;

        /// <summary> Flag to indicate if the object has been disposed </summary>
        private bool _disposed = false;

        /// <summary> The database object that maintains the connection </summary>
        private SqlDatabase database;

        /// <summary> The connection used for transactions </summary>
        private DbConnection connection;

        /// <summary> The refence to a database transaction </summary>
        private DbTransaction transaction;

        #endregion

        #region Public Properties

        /// <summary> Flag to identify is a transaction is in progress </summary>
        public bool IsOpenTransaction { get { return (transaction == null) ? false : true; } }

        #endregion

        #region Constructor

        /// <summary> Constructor </summary>
        public DataTransaction()
        {
            // Capture a database connection
            database = (SqlDatabase)DatabaseFactory.CreateDatabase();

            // Initialize the transaction object
            connection = null;
            transaction = null;
        }

        /// <summary> Constructor: Passing null for connectionString will use default connection </summary>
        /// <param name="connectionString"></param>
        public DataTransaction(string connectionString)
        {
            // Capture a database connection
            if (connectionString != null)
            {
                database = (SqlDatabase)DatabaseFactory.CreateDatabase(connectionString);
            }
            else
            {
                database = (SqlDatabase)DatabaseFactory.CreateDatabase();
            }

            // Initialize the transaction object
            connection = null;
            transaction = null;
        }

        /// <summary> Called to dipose the log </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary> Called to dispose the transaction </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            bool throwError = false;

            if (!_disposed)
            {
                if (disposing)
                {
                    if (transaction != null)
                    {
                        transaction.Rollback();
                        transaction.Dispose();
                        transaction = null;

                        throwError = true;
                    }

                    if (connection != null)
                    {
                        connection.Close();
                        connection = null;
                    }
                }

                // Mark everything as disposed
                _disposed = true;
            }

            if (throwError)
            {
                throw new Exception("Open Transaction");
            }
        }

        #endregion

        #region Transaction Support

        /// <summary> Called to begin a transaction </summary>
        public void Begin()
        {
            // Create a connection for the transaction
            connection = database.CreateConnection();

            // Open the connection
            connection.Open();

            // Begin the transaction
            transaction = connection.BeginTransaction();
        }

        /// <summary> Called to commit the open transaction </summary>
        public void Commit()
        {
            if (transaction != null)
            {
                transaction.Commit();
                transaction.Dispose();
                transaction = null;
            }
        }

        /// <summary> Called to commit the open transaction </summary>
        public void Rollback()
        {
            if (transaction != null)
            {
                transaction.Rollback();
                transaction.Dispose();
                transaction = null;
            }
        }

        #endregion

        #region Command Support

        /// <summary> Create a command object </summary>
        /// <param name="storedProcedureName"></param>
        /// <returns> A database command object </returns>
        public DbCommand GetStoredProcCommand(string storedProcedureName)
        {
            return database.GetStoredProcCommand(storedProcedureName);
        }

        #endregion

        #region In Parameter Support

        /// <summary> Adds a new Tiny In parameter to the command object </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AddInParameterTiny(DbCommand command, string name, byte? value)
        {
            database.AddInParameter(command, name, DbType.Byte, value);
        }

        /// <summary> Adds a new Short In parameter to the command object </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AddInParameterShort(DbCommand command, string name, short? value)
        {
            database.AddInParameter(command, name, DbType.Int16, value);
        }

        /// <summary> Adds a new Integer In parameter to the command object </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AddInParameterInteger(DbCommand command, string name, int? value)
        {
            database.AddInParameter(command, name, DbType.Int32, value);
        }

        /// <summary> Adds a new Long In parameter to the command object </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AddInParameterLong(DbCommand command, string name, long? value)
        {
            database.AddInParameter(command, name, DbType.Int64, value);
        }

        /// <summary> Adds a new Unsigned Long In parameter to the command object </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AddInParameterULong(DbCommand command, string name, ulong? value)
        {
            database.AddInParameter(command, name, DbType.UInt64, value);
        }

        /// <summary> Adds a new String In parameter to the command object </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AddInParameterString(DbCommand command, string name, string value)
        {
            database.AddInParameter(command, name, DbType.String, value);
        }

        /// <summary> Adds a new Decimal In parameter to the command object </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AddInParameterDecimal(DbCommand command, string name, decimal? value)
        {
            if (value > SQL_MONEY_MAX_VALUE)
            {
                value = SQL_MONEY_MAX_VALUE;
            }

            database.AddInParameter(command, name, DbType.Decimal, value);
        }

        /// <summary> Adds a new Double In parameter to the command object </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AddInParameterDouble(DbCommand command, string name, double? value)
        {
            database.AddInParameter(command, name, DbType.Double, value);
        }

        /// <summary> Adds a new DateTime In parameter to the command object </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AddInParameterDateTime(DbCommand command, string name, DateTime? value)
        {
            database.AddInParameter(command, name, DbType.DateTime, value);
        }

        /// <summary> Adds a new TimeSpan In parameter to the command object </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AddInParameterTimeSpan(DbCommand command, string name, TimeSpan? value)
        {
            database.AddInParameter(command, name, SqlDbType.Time, value);
        }

        /// <summary> Adds a new Boolean In parameter to the command object </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AddInParameterBoolean(DbCommand command, string name, bool? value)
        {
            database.AddInParameter(command, name, DbType.Boolean, value);
        }

        /// <summary> Adds a new Xml In parameter to the command object </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AddInParameterXml(DbCommand command, string name, string value)
        {
            database.AddInParameter(command, name, DbType.Xml, value);
        }

        /// <summary> Adds a new Table Parameter to command Object </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <param name="value">Object that implements IEnumerable SqlDataRecord</param>
        public void AddInParameterTable(DbCommand command, string name, IEnumerable<SqlDataRecord> value)
        {
            database.AddInParameter(command, name, SqlDbType.Structured, value);
        }

        /// <summary> Adds a new GUID Parameter to command Object </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <param name="value">Guid paramter</param>
        public void AddInParameterGuid(DbCommand command, string name, Guid? value)
        {
            database.AddInParameter(command, name, SqlDbType.UniqueIdentifier, value);
        }

        /// <summary> Adds a new var binary Parameter to command Object </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <param name="data">binary data</param>
        public void AddInParameterVarBinary(DbCommand command, string name, byte[] data)
        {
            database.AddInParameter(command, name, SqlDbType.VarBinary, data);
        }

        /// <summary> Adds a new image Parameter to command Object </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <param name="image"> binary image data </param>
        public void AddInParameterImage(DbCommand command, string name, byte[] image)
        {
            database.AddInParameter(command, name, SqlDbType.Image, image);
        }

        #endregion

        #region Out Parameter Support

        /// <summary> Adds a new Boolean parameter to the command object </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        public void AddOutParameterBoolean(DbCommand command, string name)
        {
            database.AddOutParameter(command, name, DbType.Boolean, 1);
        }

        /// <summary> Adds a new DateTime parameter to the command object </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        public void AddOutParameterDateTime(DbCommand command, string name)
        {
            database.AddOutParameter(command, name, DbType.DateTime, 8);
        }

        /// <summary> Adds a new tiny Out parameter to the command object </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        public void AddOutParameterTiny(DbCommand command, string name)
        {
            database.AddOutParameter(command, name, DbType.Byte, sizeof(byte));
        }

        /// <summary> Adds a new Short Out parameter to the command object </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        public void AddOutParameterShort(DbCommand command, string name)
        {
            database.AddOutParameter(command, name, DbType.Int16, sizeof(int));
        }

        /// <summary> Adds support for Image as an Out param.</summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        public void AddOutParameterImage(DbCommand command, string name)
        {
            database.AddOutParameter(command, name, SqlDbType.VarBinary, int.MaxValue);
        }

        /// <summary> Adds a new Integer Out parameter to the command object </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        public void AddOutParameterInteger(DbCommand command, string name)
        {
            database.AddOutParameter(command, name, DbType.Int32, sizeof(int));
        }

        /// <summary> Adds a new String Out parameter to the command object </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <param name="length"></param>
        public void AddOutParameterString(DbCommand command, string name, int length)
        {
            database.AddOutParameter(command, name, DbType.String, length);
        }

        /// <summary> Adds a new Decimal Out parameter to the command object </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        public void AddOutParameterDecimal(DbCommand command, string name)
        {
            database.AddOutParameter(command, name, DbType.Currency, sizeof(decimal));
        }

        /// <summary> Adds a new Double Out parameter to the command object </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        public void AddOutParameterDouble(DbCommand command, string name)
        {
            database.AddOutParameter(command, name, DbType.Double, sizeof(double));
        }

        // <summary> Adds a new Long Out parameter to the command object </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        public void AddOutParameterLong(DbCommand command, string name)
        {
            database.AddOutParameter(command, name, DbType.Int64, sizeof(long));
        }

        // <summary> Adds a new Unsigned Long Out parameter to the command object </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        public void AddOutParameterULong(DbCommand command, string name)
        {
            database.AddOutParameter(command, name, DbType.UInt64, sizeof(ulong));
        }

        /// <summary>Add a GUID Out Parameter to the command object</summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        public void AddOutParameterGuid(DbCommand command, string name)
        {
            database.AddOutParameter(command, name, DbType.Guid, 64);
        }

        /// <summary>Add a VarBianry Out Parameter to the command object</summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <param name="length"></param>
        public void AddOutParameterGuid(DbCommand command, string name, int length)
        {
            database.AddOutParameter(command, name, DbType.Binary, length);
        }

        #endregion

        #region Execute Support

        /// <summary> Execute a command that does not return results </summary>
        /// <param name="command"></param>
        /// <param name="commandTimeout"></param>
        public void ExecuteNonQuery(DbCommand command, int commandTimeout = 300)
        {
            // Give us 5 minutes
            command.CommandTimeout = commandTimeout;

            if (transaction == null)
            {
                database.ExecuteNonQuery(command);
            }
            else
            {
                database.ExecuteNonQuery(command, transaction);
            }
        }

        /// <summary> Execute a command that does return results </summary>
        /// <param name="command"></param>
        /// <param name="commandTimeout"></param>
        /// <returns> A Sql Data Reader </returns>
        public IDataReader ExecuteQuery(DbCommand command, int commandTimeout = 300)
        {
            // Give us 5 minutes
            command.CommandTimeout = commandTimeout;

            IDataReader dataReader = null;

            if (transaction == null)
            {
                dataReader = database.ExecuteReader(command);
            }
            else
            {
                dataReader = database.ExecuteReader(command, transaction);
            }

            return dataReader;
        }

        /// <summary>Execute a command that returns data as XmlReader. Used for reports.</summary>
        /// <param name="command"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>An XmlReader</returns>
        public XmlReader ExecuteXmlReader(DbCommand command, int commandTimeout = 300)
        {
            // Give us 5 minutes
            command.CommandTimeout = commandTimeout;

            XmlReader dataReader = null;

            if (transaction == null)
            {
                dataReader = (XmlReader)database.ExecuteXmlReader(command);
            }
            else
            {
                dataReader = (XmlReader)database.ExecuteReader(command, transaction);
            }

            return dataReader;
        }

        /// <summary> Execute a command and return a DataSet </summary>
        /// <param name="tableName"></param>
        /// <param name="command"></param>
        /// <param name="commandTimeout"></param>
        /// <returns> DataSet of results </returns>
        public DataSet ExecuteQuery(string tableName, DbCommand command, int commandTimeout = 300)
        {
            // Give us 5 minutes
            command.CommandTimeout = commandTimeout;

            DataSet dataSet = null;

            dataSet = database.ExecuteDataSet(command);
            dataSet.Tables[0].TableName = tableName;

            return dataSet;
        }

        /// <summary> Execute a command and return a DataSet </summary>
        /// <param name="tableNames"></param>
        /// <param name="command"></param>
        /// <param name="commandTimeout"></param>
        /// <returns> DataSet of results </returns>
        public DataSet ExecuteQuery(string[] tableNames, DbCommand command, int commandTimeout = 300)
        {
            // Give us 5 minutes
            command.CommandTimeout = commandTimeout;

            DataSet dataSet = null;

            dataSet = database.ExecuteDataSet(command);
            for (int table = 0; table < dataSet.Tables.Count; ++table)
            {
                dataSet.Tables[table].TableName = tableNames[table];
            }

            return dataSet;
        }

        #endregion

        #region Get Parameter Support

        /// <summary> Capture the predefined boolean output parameter </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <returns> The boolean value of the parameter</returns>
        public bool GetParameterValueBoolean(DbCommand command, string name)
        {
            return (bool)database.GetParameterValue(command, name);
        }

        /// <summary> Capture the predefined short output parameter </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <returns> The byte value of the parameter</returns>
        public byte GetParameterValueTiny(DbCommand command, string name)
        {
            return (byte)database.GetParameterValue(command, name);
        }

        /// <summary> Capture the predefined short output parameter </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <returns> The short value of the parameter</returns>
        public short GetParameterValueShort(DbCommand command, string name)
        {
            return (short)database.GetParameterValue(command, name);
        }

        /// <summary> Capture the predefined integer output parameter </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <returns> The integer value of the parameter</returns>
        public int GetParameterValueInteger(DbCommand command, string name)
        {
            return (int)database.GetParameterValue(command, name);
        }

        /// <summary> Capture the predefined integer output parameter </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <returns> The integer value of the parameter</returns>
        public int? GetParameterValueIntegerNull(DbCommand command, string name)
        {
            object intVal = database.GetParameterValue(command, name);
            return (intVal == DBNull.Value) ? null : (int?)intVal;
        }

        /// <summary> Capture the predefined string output parameter </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <returns> The string value of the parameter</returns>
        public string GetParameterValueString(DbCommand command, string name)
        {
            return (string)database.GetParameterValue(command, name);
        }

        /// <summary> Capture the predefined decimal output parameter </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <returns> The decimal value of the parameter</returns>
        public decimal GetParameterValueDecimal(DbCommand command, string name)
        {
            return (decimal)database.GetParameterValue(command, name);
        }

        /// <summary> Capture the predefined double output parameter </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <returns> The double value of the parameter</returns>
        public double GetParameterValueDouble(DbCommand command, string name)
        {
            return (double)database.GetParameterValue(command, name);
        }

        /// <summary> Capture the predefined decimal output parameter </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <returns> The decimal value of the parameter</returns>
        public decimal? GetParameterValueDecimalNull(DbCommand command, string name)
        {
            object value = database.GetParameterValue(command, name);
            return (value == DBNull.Value) ? null : (decimal?)value;
        }

        /// <summary> Capture the predefined double output parameter </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <returns> The double value of the parameter</returns>
        public double? GetParameterValueDoubleNull(DbCommand command, string name)
        {
            object value = database.GetParameterValue(command, name);
            return (value == DBNull.Value) ? null : (double?)value;
        }

        /// <summary> Capture the predefined DateTime output parameter </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <returns> The DateTime value of the parameter</returns>
        public DateTime GetParameterValueDateTime(DbCommand command, string name)
        {
            return (DateTime)database.GetParameterValue(command, name);
        }

        /// <summary> Capture the predefined TimeSpan output parameter </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <returns> The TimeSpan value of the parameter</returns>
        public TimeSpan GetParameterValueTimeSpan(DbCommand command, string name)
        {
            return (TimeSpan)database.GetParameterValue(command, name);
        }

        /// <summary> Capture the predefined long output parameter </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <returns> The long value of the parameter</returns>
        public long GetParameterValueLong(DbCommand command, string name)
        {
            return (long)database.GetParameterValue(command, name);
        }

        /// <summary> Capture the predefined unsigned long output parameter </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <returns> The unsigned long value of the parameter</returns>
        public ulong GetParameterValueULong(DbCommand command, string name)
        {
            return (ulong)database.GetParameterValue(command, name);
        }

        /// <summary>Capture the specified Guid Parameter</summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public Guid? GetParameterValueGuid(DbCommand command, string name)
        {
            return (Guid?)database.GetParameterValue(command, name);
        }

        /// <summary>Capture the specified VarBinary Parameter</summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public byte[] GetParameterValueVarBinary(DbCommand command, string name)
        {
            object obj = database.GetParameterValue(command, name);

            if (obj == System.DBNull.Value)
            {
                return null;
            }

            return (byte[])obj;
        }

        #endregion

        #region Static Methods

        /// <summary> Capture the column value for the current row </summary>
        /// <param name="reader"></param>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        public static byte? ReadTiny(IDataReader reader, int columnIndex)
        {
            return reader.IsDBNull(columnIndex) ? null : (byte?)reader.GetByte(columnIndex);
        }

        /// <summary> Capture the column value for the current row </summary>
        /// <param name="reader"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static byte? ReadTiny(IDataReader reader, string columnName)
        {
            return (reader[columnName] == DBNull.Value) ? null : (byte?)reader[columnName];
        }

        /// <summary> Capture the column value for the current row </summary>
        /// <param name="reader"></param>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        public static short? ReadShort(IDataReader reader, int columnIndex)
        {
            return reader.IsDBNull(columnIndex) ? null : (short?)reader.GetInt16(columnIndex);
        }

        /// <summary> Capture the column value for the current row </summary>
        /// <param name="reader"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static short? ReadShort(IDataReader reader, string columnName)
        {
            return (reader[columnName] == DBNull.Value) ? null : (short?)reader[columnName];
        }

        /// <summary> Capture the column value for the current row </summary>
        /// <param name="reader"></param>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        public static int? ReadInteger(IDataReader reader, int columnIndex)
        {
            return reader.IsDBNull(columnIndex) ? null : (int?)reader.GetInt32(columnIndex);
        }

        /// <summary> Capture the column value for the current row </summary>
        /// <param name="reader"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static int? ReadInteger(IDataReader reader, string columnName)
        {
            return (reader[columnName] == DBNull.Value) ? null : (int?)reader[columnName];
        }

        /// <summary> Capture the column value for the current row </summary>
        /// <param name="reader"></param>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        public static decimal? ReadDecimal(IDataReader reader, int columnIndex)
        {
            return reader.IsDBNull(columnIndex) ? null : (decimal?)reader.GetDecimal(columnIndex);
        }

        /// <summary> Capture the column value for the current row </summary>
        /// <param name="reader"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static decimal? ReadDecimal(IDataReader reader, string columnName)
        {
            return (reader[columnName] == DBNull.Value) ? null : (decimal?)reader[columnName];
        }

        /// <summary> Capture the column value for the current row </summary>
        /// <param name="reader"></param>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        public static double? ReadDouble(IDataReader reader, int columnIndex)
        {
            return reader.IsDBNull(columnIndex) ? null : (double?)reader.GetDouble(columnIndex);
        }

        /// <summary> Capture the column value for the current row </summary>
        /// <param name="reader"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static double? ReadDouble(IDataReader reader, string columnName)
        {
            return (reader[columnName] == DBNull.Value) ? null : (double?)reader[columnName];
        }

        /// <summary> Capture the column value for the current row </summary>
        /// <param name="reader"></param>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        public static string ReadString(IDataReader reader, int columnIndex)
        {
            return reader.IsDBNull(columnIndex) ? null : reader.GetString(columnIndex);
        }

        /// <summary> Capture the column value for the current row </summary>
        /// <param name="reader"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static string ReadString(IDataReader reader, string columnName)
        {
            return (reader[columnName] == DBNull.Value) ? null : (string)reader[columnName];
        }

        /// <summary> Capture the column value for the current row </summary>
        /// <param name="reader"></param>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        public static DateTime? ReadDateTime(IDataReader reader, int columnIndex)
        {
            return reader.IsDBNull(columnIndex) ? null : (DateTime?)reader.GetDateTime(columnIndex);
        }

        /// <summary> Capture the column value for the current row </summary>
        /// <param name="reader"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static DateTime? ReadDateTime(IDataReader reader, string columnName)
        {
            return (reader[columnName] == DBNull.Value) ? null : (DateTime?)reader[columnName];
        }

        /// <summary> Capture the column value for the current row </summary>
        /// <param name="reader"></param>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        public static TimeSpan? ReadTimeSpan(IDataReader reader, int columnIndex)
        {
            SqlDataReader IDataReader = ((RefCountingDataReader)reader).InnerReader as SqlDataReader;

            return reader.IsDBNull(columnIndex) ? null : (TimeSpan?)IDataReader.GetTimeSpan(columnIndex);
        }

        /// <summary> Capture the column value for the current row </summary>
        /// <param name="reader"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static TimeSpan? ReadTimeSpan(IDataReader reader, string columnName)
        {
            return (reader[columnName] == DBNull.Value) ? null : (TimeSpan?)reader[columnName];
        }

        /// <summary> Capture the column value for the current row </summary>
        /// <param name="reader"></param>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        public static bool? ReadBoolean(IDataReader reader, int columnIndex)
        {
            return reader.IsDBNull(columnIndex) ? null : (bool?)reader.GetBoolean(columnIndex);
        }

        /// <summary> Capture the column value for the current row </summary>
        /// <param name="reader"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static bool? ReadBoolean(IDataReader reader, string columnName)
        {
            return (reader[columnName] == DBNull.Value) ? null : (bool?)reader[columnName];
        }

        /// <summary> Capture the column value for the current row </summary>
        /// <param name="reader"></param>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        public static long? ReadLong(IDataReader reader, int columnIndex)
        {
            return reader.IsDBNull(columnIndex) ? null : (long?)reader.GetInt64(columnIndex);
        }

        /// <summary> Capture the column value for the current row </summary>
        /// <param name="reader"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static long? ReadLong(IDataReader reader, string columnName)
        {
            return (reader[columnName] == DBNull.Value) ? null : (long?)reader[columnName];
        }

        /// <summary> Capture the column value for the current row </summary>
        /// <param name="reader"></param>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        public static ulong? ReadULong(IDataReader reader, int columnIndex)
        {
            return reader.IsDBNull(columnIndex) ? null : (ulong?)reader.GetInt64(columnIndex);
        }

        /// <summary> Capture the column value for the current row </summary>
        /// <param name="reader"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static ulong? ReadULong(IDataReader reader, string columnName)
        {
            return (reader[columnName] == DBNull.Value) ? null : (ulong?)reader[columnName];
        }

        /// <summary> Capture the column value for the current row </summary>
        /// <param name="reader"></param>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        public static Guid? ReadGuid(IDataReader reader, int columnIndex)
        {
            return reader.IsDBNull(columnIndex) ? null : (Guid?)reader.GetGuid(columnIndex);
        }

        /// <summary> Capture the column value for the current row </summary>
        /// <param name="reader"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static Guid? ReadGuid(IDataReader reader, string columnName)
        {
            return reader[columnName] == DBNull.Value ? null : (Guid?)reader[columnName];
        }

        /// <summary> Capture the column value for the current row </summary>
        /// <param name="reader"></param>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        public static byte[] ReadImage(IDataReader reader, int columnIndex)
        {
            return reader.IsDBNull(columnIndex) ? null : (byte[])reader[columnIndex];
        }

        /// <summary> Capture the column value for the current row </summary>
        /// <param name="reader"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static byte[] ReadImage(IDataReader reader, string columnName)
        {
            return reader[columnName] == DBNull.Value ? null : (byte[])reader[columnName];
        }

        /// <summary> Called to produce the query string from the configured DBCommand object. Used for logging purposes </summary>
        /// <param name="dbCommand"></param>
        /// <returns></returns>
        public static string QueryString(DbCommand dbCommand)
        {
            StringBuilder query = new StringBuilder();
            List<StringBuilder> tableTypes = new List<StringBuilder>();

            try
            {
                // Add the procedure name      
                query.AppendFormat("EXEC {0} ", dbCommand.CommandText);

                // Add the parameters
                for (int parameter = 0; parameter < dbCommand.Parameters.Count; ++parameter)
                {
                    DbParameter dbParameter = dbCommand.Parameters[parameter];

                    switch (dbParameter.DbType)
                    {
                        case DbType.AnsiString:
                        case DbType.AnsiStringFixedLength:
                        case DbType.DateTime:
                        case DbType.DateTime2:
                        case DbType.DateTimeOffset:
                        case DbType.Time:
                        case DbType.Guid:
                        case DbType.String:
                        case DbType.StringFixedLength:
                            if (dbParameter.Direction == ParameterDirection.Input)
                            {
                                if (dbParameter.Value != DBNull.Value)
                                    query.AppendFormat("{0} = '{1}',", dbParameter.ParameterName, dbParameter.Value);
                                else
                                    query.AppendFormat("{0} = NULL,", dbParameter.ParameterName);
                            }
                            else
                                query.AppendFormat("{0} = {0} OUTPUT,", dbParameter.ParameterName);
                            break;
                        case DbType.Boolean:
                            if (dbParameter.Direction == ParameterDirection.Input)
                            {
                                if (dbParameter.Value != DBNull.Value)
                                    query.AppendFormat("{0} = {1},", dbParameter.ParameterName, ((bool)dbParameter.Value) == true ? "1" : "0");
                                else
                                    query.AppendFormat("{0} = NULL,", dbParameter.ParameterName);
                            }
                            else
                                query.AppendFormat("{0} = {0} OUTPUT,", dbParameter.ParameterName);
                            break;
                        case DbType.Object:
                            if (dbParameter.Value is IEnumerable<SqlDataRecord>)
                            {
                                query.AppendFormat("{0} = {1},", dbParameter.ParameterName, dbParameter.ParameterName);

                                StringBuilder typesList = new StringBuilder();
                                tableTypes.Add(typesList);

                                typesList.AppendFormat("DECLARE {0} {1};\r\n", dbParameter.ParameterName, dbParameter.Value.GetType().Name);
                                IEnumerable<SqlDataRecord> sqlDataRecords = (IEnumerable<SqlDataRecord>)dbParameter.Value;
                                foreach (SqlDataRecord dataRecord in sqlDataRecords)
                                {
                                    typesList.AppendFormat("INSERT {0} VALUES (", dbParameter.ParameterName);

                                    for (int field = 0; field < dataRecord.FieldCount; ++field)
                                    {
                                        switch (dataRecord[field].GetType().Name)
                                        {
                                            case "String":
                                            case "DateTime":
                                                typesList.AppendFormat("{0},", (dataRecord[field] == DBNull.Value) ? "NULL" : string.Format("'{0}'", dataRecord[field]));
                                                break;

                                            case "Boolean":
                                                typesList.AppendFormat("{0},", (dataRecord[field] == DBNull.Value) ? "NULL" : (((bool)dataRecord[field])) == true ? "1" : "0");
                                                break;

                                            default:
                                                typesList.AppendFormat("{0},", (dataRecord[field] == DBNull.Value) ? "NULL" : dataRecord[field]);
                                                break;
                                        }
                                    }

                                    typesList.Remove(typesList.Length - 1, 1);
                                    typesList.Append(");\r\n");
                                }
                            }
                            break;
                        default:
                            if (dbParameter.Direction == ParameterDirection.Input)
                                query.AppendFormat("{0} = {1},", dbParameter.ParameterName, (dbParameter.Value == DBNull.Value) ? "null" : dbParameter.Value);
                            else
                                query.AppendFormat("{0} = {0} OUTPUT,", dbParameter.ParameterName);
                            break;
                    }
                }
            }

            catch
            {
            }

            if (tableTypes.Count > 0)
            {
                StringBuilder final = new StringBuilder();
                for (int table = 0; table < tableTypes.Count; ++table)
                {
                    final.AppendFormat("{0}", tableTypes[table].ToString());
                }
                final.AppendFormat("{0}", query.ToString());

                return final.ToString().TrimEnd(',');
            }

            return query.ToString().TrimEnd(',');
        }

        #endregion
    }
}

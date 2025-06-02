using GH_LDM_PIIService.Helpers;
using NT.Integration.SharedKernel.OracleManagedHelper;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GH_LDM_PIIService.DSL
{
    public abstract class BaseDSL
    {
        protected readonly string ConnectionString;
        protected readonly File_Logger Logger;

        protected BaseDSL(string connectionString, string loggerName)
        {
            ConnectionString = connectionString;
            Logger = File_Logger.GetInstance(loggerName);
        }

        protected async Task ExecuteWithConnectionAsync(Func<OracleConnection, Task> action)
        {
            try
            {
                using var conn = new OracleConnection(ConnectionString);
                await conn.OpenAsync();
                Logger.WriteToLogFile(ActionTypeEnum.Information, $"Database connection opened at {DateTime.Now}");

                await action(conn);

                Logger.WriteToLogFile(ActionTypeEnum.Information, "Database operation completed.");
            }
            catch (Exception ex)
            {
                Logger.WriteToLogFile(ActionTypeEnum.Exception, $"Exception during DB operation: {ex}");
                throw;
            }
        }
    }
}

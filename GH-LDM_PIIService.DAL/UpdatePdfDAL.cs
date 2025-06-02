using GH_LDM_PIIService.Helpers;
using NT.Integration.SharedKernel.OracleManagedHelper;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GH_LDM_PIIService.DAL
{
    public class UpdatePdfDAL
    {
        public (string jsonData, int seqNum) GetAttachmentData(OracleConnection conn)
        {
            using var cmd = new OracleCommand("get_GH_ATTACHMENT_API", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            var jsonOut = new OracleParameter("P_JSON", OracleDbType.Clob)
            {
                Direction = ParameterDirection.Output
            };
            var seqOut = new OracleParameter("P_Seq_num", OracleDbType.Int32)
            {
                Direction = ParameterDirection.Output
            };

            cmd.Parameters.Add(jsonOut);
            cmd.Parameters.Add(seqOut);

            cmd.ExecuteNonQuery();
            string json = (jsonOut.Value is OracleClob clob) ? clob.Value : null;

            //string json = ((OracleClob)jsonOut.Value).Value;
            int seq = Convert.ToInt32(seqOut.Value);

            return (json, seq);
        }

        public void SetAttachmentData(OracleConnection conn, byte[] fileBlob, int seqNum, int status)
        {
            using var cmd = new OracleCommand("set_GH_ATTACHMENT_API", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("P_file", OracleDbType.Blob).Value = (object)fileBlob ?? DBNull.Value;
            cmd.Parameters.Add("P_seq_num", OracleDbType.Int32).Value = seqNum;
            cmd.Parameters.Add("P_status", OracleDbType.Int32).Value = status;

            cmd.ExecuteNonQuery();
        }
    }
}


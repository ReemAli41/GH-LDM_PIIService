using GH_LDM_PIIService.DAL;
using GH_LDM_PIIService.Entities.Request;
using GH_LDM_PIIService.Entities.Response;
using GH_LDM_PIIService.Helpers;
using System.Text.Json;

namespace GH_LDM_PIIService.DSL
{
    public class UpdatePdfDSL : BaseDSL
    {
        private readonly UpdatePdfDAL _updatePdfDAL;
        private readonly HttpHelper _httpHelper;
        private readonly ConfigManager _config;

        public UpdatePdfDSL(ConfigManager config)
            : base(config.ConnectionString, "GHAttachmentDSL")
        {
            _config = config;
            _updatePdfDAL = new UpdatePdfDAL();
            _httpHelper = new HttpHelper(config);
        }

        public async Task ProcessAttachmentAsync(CancellationToken stoppingToken)
        {
            await ExecuteWithConnectionAsync(async conn =>
            {
                try
                {
                    var (json, seqNum) = _updatePdfDAL.GetAttachmentData(conn);
                    Logger.WriteToLogFile(ActionTypeEnum.Information, $"Retrieved attachment with SeqNum: {seqNum}");

                    var request = JsonSerializer.Deserialize<PdfUpdateRequestDto>(json);
                   
                    var tokenResponse = await _httpHelper.PostFormUrlEncodedAsync();


                    var pdfResponse = await _httpHelper.PostJsonAsync(request, tokenResponse.AccessToken);

                    if (pdfResponse.Status == "success")
                    {
                        byte[] pdfBytes = Convert.FromBase64String(pdfResponse.ModifiedPdf);
                        _updatePdfDAL.SetAttachmentData(conn, pdfBytes, seqNum, 1); //1 => success
                        Logger.WriteToLogFile(ActionTypeEnum.Information, $"Successfully updated attachment: {seqNum}");
                    }
                    else
                    {
                        _updatePdfDAL.SetAttachmentData(conn, null, seqNum, 2); //2 => failure
                        Logger.WriteToLogFile(ActionTypeEnum.Information, $"PDF API failed for SeqNum: {seqNum}");
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteToLogFile(ActionTypeEnum.Exception, $"Error in ProcessAttachmentAsync: {ex}");
                    throw; 
                }
            });
        }
    }
}
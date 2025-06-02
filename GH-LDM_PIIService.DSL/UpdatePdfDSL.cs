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

        public UpdatePdfDSL(ConfigManager config): base(config.ConnectionString, "UpdatePdfDSL")
        {
            _config = config;
            _updatePdfDAL = new UpdatePdfDAL();
            _httpHelper = new HttpHelper(config);
        }

        public async Task ProcessAttachmentAsync(CancellationToken stoppingToken)
        {
            await ExecuteWithConnectionAsync(async conn =>
            {
                int seqNum = -1; //Track SeqNum For Error handling

                try
                {
                    //Get Data From DB
                    var (json, seq) = _updatePdfDAL.GetAttachmentData(conn);
                    seqNum = seq; //Store Error Cases

                    Logger.WriteToLogFile(ActionTypeEnum.Information, $"Retrieved attachment with SeqNum: {seqNum}");

                    //Deserialize Request
                    var request = JsonSerializer.Deserialize<PdfUpdateRequestDto>(json);
                    if (request?.Data == null || string.IsNullOrEmpty(request.Pdf))
                    {
                        throw new InvalidDataException("Invalid PDF request data");
                    }

                    //Get Auth Token
                    var tokenResponse = await _httpHelper.PostFormUrlEncodedAsync();

                    //Send To PDF Service
                    var pdfResponse = await _httpHelper.PostJsonAsync(request, tokenResponse.AccessToken);

                    if (pdfResponse.Status == "success")
                    {
                        //SUCCESS: Update DB With Status => 1
                        byte[] pdfBytes = Convert.FromBase64String(pdfResponse.ModifiedPdf);
                        _updatePdfDAL.SetAttachmentData(conn, pdfBytes, seqNum, 1);
                        Logger.WriteToLogFile(ActionTypeEnum.Information, $"Successfully updated attachment: {seqNum}");
                    }
                    else
                    {
                        //ERROR: Update DB With Status => 2
                        _updatePdfDAL.SetAttachmentData(conn, null, seqNum, 2);
                        Logger.WriteToLogFile(ActionTypeEnum.Error, $"PDF API failed for SeqNum {seqNum}: {pdfResponse.ErrorMessage}");
                    }
                }
                catch (Exception ex)
                {
                    // Update DB to 2 If There Is Another Failer
                    if (seqNum != -1) 
                    {
                        _updatePdfDAL.SetAttachmentData(conn, null, seqNum, 2);
                    }

                    Logger.WriteToLogFile(ActionTypeEnum.Exception, $"Error processing SeqNum {seqNum}: {ex.Message}");

                    throw;
                }
            });
        }
    }
}
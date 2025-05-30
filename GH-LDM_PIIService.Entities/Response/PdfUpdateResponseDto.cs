using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GH_LDM_PIIService.Entities.Response
{
    public class PdfUpdateResponseDto
    {
        public string Status { get; set; }
        public string ModifiedPdf { get; set; }
        public int? ErrorStatusCode { get; set; }
        public string ErrorMessage { get; set; }
    }
}

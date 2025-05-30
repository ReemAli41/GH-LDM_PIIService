using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GH_LDM_PIIService.Entities.Request
{
    public class PdfUpdateRequestDto
    {
        public string Pdf { get; set; }
        public PersonDataDto Data { get; set; }
        public string TemplateId { get; set; }
    }
}

using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using AdmissionsPortal.Models;

namespace AdmissionsPortal.ViewModels
{
    public class AdditionalDocumentViewModel
    {
        public int ApplicationId { get; set; }

        [Required]
        public DocumentType Type { get; set; }

        [Required]
        public IFormFile? File { get; set; }
    }
}

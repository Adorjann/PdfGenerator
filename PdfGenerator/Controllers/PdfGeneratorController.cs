using Balbarak.WeasyPrint;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace PdfGenerator.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PdfGeneratorController : ControllerBase
    {

        private readonly ILogger<PdfGeneratorController> _logger;
        private readonly PdfManager pdfManager;

        public PdfGeneratorController(PdfManager pdfManager)
        {
            this.pdfManager = pdfManager ?? throw new System.ArgumentNullException(nameof(pdfManager));
        }

        [HttpGet("GetPdf")]
        public async Task<ActionResult> GetPdfAsync()
        {
            var embeddedResources = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames();
            var htmlPath = embeddedResources.Where(x => x.Contains(".html")).First();
            var ttfPath = embeddedResources.Where(x => x.Contains(".ttf")).ToList();
            var imagePath = embeddedResources.Where(x => x.Contains(".png")).First();
            var pdfByteArray = await pdfManager.GetPDfAsync(htmlPath, imagePath, ttfPath).ConfigureAwait(false);
            return File(pdfByteArray, MediaTypeNames.Application.Pdf, "Test.pdf");
        }        
    }
}
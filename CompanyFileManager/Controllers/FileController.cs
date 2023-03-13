using Microsoft.AspNetCore.Mvc;

namespace CompanyFileManager.Controllers
{
    [Route("[controller]")]
    public class FileController : Controller
    {
        private IWebHostEnvironment _hostingEnvironment;
        public FileController(IWebHostEnvironment environment)
        {
            _hostingEnvironment = environment;
        }

        public IActionResult Index()
        {
            return Content("HELLO");
        }

        protected internal string Hello() => "Hello ASP.NET";

        [HttpGet("download")]
        [Route("[action]")]
        public IActionResult GetBlobDownload([FromQuery] string link)
        {
            var net = new System.Net.WebClient();
            var data = net.DownloadData(link);
            var content = new System.IO.MemoryStream(data);
            var contentType = "APPLICATION/octet-stream";
            var fileName = Path.GetFileName(link);
            return File(content, contentType, fileName);
        }

        [HttpGet]
        public IActionResult DownloadFile([FromQuery] string link)
        {
            // Since this is just and example, I am using a local file located inside wwwroot
            // Afterwards file is converted into a stream
            var path = Path.Combine(_hostingEnvironment.WebRootPath, link);
            var fs = new FileStream(path, FileMode.Open);

            // Return the file. A byte array can also be used instead of a stream
            return File(fs, "application/octet-stream", "Sample.xlsx");
        }

    }
}

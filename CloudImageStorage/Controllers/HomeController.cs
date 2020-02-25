using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CloudImageStorage.Models;
using Microsoft.AspNetCore.Http;
using Azure.Storage.Blobs;
using CloudImageStorage.Utils;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.EntityFrameworkCore.Internal;

namespace CloudImageStorage.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AzureBlobContainerClient _blobClient;
        private readonly ImagesDbContext _context;
        private readonly ComputerVisionClient _visionClient;

        public HomeController(
            ILogger<HomeController> logger, 
            AzureBlobContainerClient blobClient, 
            ImagesDbContext context,
            ComputerVisionClient visionClient)
        {
            _logger = logger;
            _blobClient = blobClient;
            _context = context;
            _visionClient = visionClient;
        }

        public IActionResult Index(string tag)
        {
            var model = _context.Images.ToList();

            if (!string.IsNullOrEmpty(tag))
                model = model.Where(t => t.Tags.Contains(tag)).ToList();

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile image)
        {
            if (image != null && image.Length > 0)
            {
                // load image to azure cloud based blob container
                var fileName = $"{Guid.NewGuid()}{image.FileName}";
                var stream = image.OpenReadStream();

                await _blobClient.UploadFileAsync(fileName, stream);

                // analyze loaded image via Computer Vision
                var visualtFeaturesList = Enum.GetValues(typeof(VisualFeatureTypes)).OfType<VisualFeatureTypes>().ToList();
                var blob = _blobClient.GetBlobByName(fileName);
                var url = blob.Uri.AbsoluteUri;

                ImageAnalysis result = null;

                try
                {
                    result = await _visionClient.AnalyzeImageAsync(url, visualtFeaturesList);
                }
                catch (Exception)
                {
                    ViewBag.ErrorTitle = "Error while uploading image";
                    ViewBag.ErrorMessage = "Uploade image must be less then 4mb and more then 50x50 px's";

                    return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
                }

                var isExplicit = result.Adult.IsAdultContent || result.Adult.IsRacyContent;

                if (isExplicit)
                {
                    ViewBag.ErrorTitle = "Error while uploading image";
                    ViewBag.ErrorMessage = "Uploade image contains adult content! Be respectful with other users!";

                    await _blobClient.DeleteFileAsync(fileName);

                    return View("Error");
                }

                List<string> tags = new List<string>();

                foreach (var tag in result.Tags)
                    tags.Add(tag.Name);

                await _context.Images.AddAsync(new Image
                {
                    Uri = url,
                    FileName = fileName,
                    Tags = tags.ToArray()
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

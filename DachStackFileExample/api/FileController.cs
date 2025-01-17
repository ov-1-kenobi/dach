using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;
using System.Text.RegularExpressions;

namespace DachStackApp.api
{
    [ApiController]
    [Route("api/file")]
    public class UploadController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;
        public UploadController(BlobServiceClient blobServiceClient, IConfiguration configuration, string containerName = "dach-file-controller-blobs")
        {
            _configuration = configuration;
            _containerName = containerName;
            _blobServiceClient = blobServiceClient;
            var _blobContainerClient = blobServiceClient.GetBlobContainerClient(_containerName);
            
            Console.WriteLine($"Account Name: {_blobServiceClient.AccountName}");
            try
            {
                if(!_blobContainerClient.Exists()) 
                {
                    var container = _blobServiceClient.CreateBlobContainer(_containerName);
                    Console.WriteLine($"Container: {container?.Value.Uri.ToString()??"Container Creation Failed"}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
            Console.WriteLine($"Using Container: {_containerName}");
        }
        [HttpGet("get-files")]
        public IActionResult GetFiles()
        {
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                var files = containerClient.GetBlobs();

                var retHTML = $"";
                foreach(var item in files)
                {
                    retHTML += $"""
                    <div id="file{item.GetUid()}" class="card card-compact bg-base-100 w-48 h-48 shadow-xl">
                        <figure class="h-48 w-full flex items-center justify-center bg-gray-200">
                            {renderItemContainer(containerClient, item)}
                        </figure>
                        <div class="card-body">
                            <h4 class="card-title truncate">{item.Name}</h4>
                            <div class="card-actions justify-end">
                                <button hx-get='/api/file/start-delete/?filename={item.Name}' hx-target='#confirm-modal-button' hx-trigger="click" hx-swap='innerHTML' class='btn btn-error btn-xs'>
                                    Delete
                                </button>
                                <button hx-swap='innerHTML' hx-get='/api/file/get-file/?filename={item.Name}' onclick="preview_modal.showModal();" hx-target='#modal_image_preview' class='btn btn-success btn-xs'>
                                    View
                                </button>
                            </div>
                        </div>
                    </div>
                    """;
                }

                return Ok(retHTML);
        }

        private string renderItemContainer(BlobContainerClient client, BlobItem item)
        {
            if (item.Name.EndsWith(".mp4") || item.Name.EndsWith(".mpeg"))
            {
                var vidVal = $"""
                    <video controls>
                        <source src="{client.GetBlobClient(item.Name).GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddMinutes(60))}"/>
                        Sorry, your browser busted
                    </video>
                """;
            
                return vidVal;
            }
            if (item.Name.EndsWith(".pdf"))
            {
                var docVal = $"""
                    <iframe src="{client.GetBlobClient(item.Name).GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddMinutes(60))}#page=1"></iframe>
                """;

                return docVal;
            }
            var imgVal = $"""<img class="h-full w-full object-cover" src="{client.GetBlobClient(item.Name).GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddMinutes(60))}"/>""";
            return imgVal;
        }

        [HttpGet("get-file")]
        public IActionResult GetFile(string filename)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);

            var blobClient = containerClient.GetBlobClient(filename);
            var retVal = blobClient.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddMinutes(15));
            var retHTML = $"""
                <img class='' src='{retVal.ToString()}' alt='No Image'/>
            """;
            return Ok(retHTML);
        }

        [HttpGet("start-delete")]
        public IActionResult StartDelete(string fileName)
        {
            var delModal = $"""
                <button type='submit' hx-delete='/api/file/?filename={fileName}' hx-target='#file{FileControllerHelpers.GetUid(fileName)}' hx-swap='delete' class='btn btn-error btn-xs'>
                    Delete
                </button>
            """;
            return Ok(delModal);
        }

        [HttpDelete()]
        public IActionResult DeleteFile(string filename)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);

            var blobClient = containerClient.GetBlobClient(filename);
            blobClient.Delete();
            return Ok();
        }

        [HttpGet("get-presigned-url")]
        public IActionResult GetPresignedUrl(string filename)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(filename);

            var sasUri = blobClient.GenerateSasUri(BlobSasPermissions.Write, DateTimeOffset.UtcNow.AddMinutes(15));

            return Ok(new { url = sasUri.ToString() });
        }

        [HttpGet("get-presigned-url-for-block")]
        public IActionResult GetPresignedUrlForBlock(string filename, string blockid)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(filename);

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerClient.Name,
                BlobName = blobClient.Name,
                Resource = "b",
                StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(15),
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Write);

            var sasToken = sasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential(_configuration["AzureAccountName"], _configuration["AzureAccountKey"]));

            var uriBuilder = new UriBuilder(blobClient.Uri)
            {
                Query = sasToken.ToString()
            };

            // Append block ID to the query
            uriBuilder.Query += $"&comp=block&blockid={Uri.EscapeDataString(blockid)}";

            return Ok(new { url = uriBuilder.Uri.ToString() });
        }

        [HttpGet("get-commit-url")]
        public IActionResult GetCommitUrl(string filename)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(filename);

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerClient.Name,
                BlobName = blobClient.Name,
                Resource = "b",
                StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(15),
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Write);

            var sasToken = sasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential(_configuration["AzureAccountName"], _configuration["AzureAccountKey"]));

            var uriBuilder = new UriBuilder(blobClient.Uri)
            {
                Query = sasToken.ToString()
            };

            // Append block list commit operation to the query
            uriBuilder.Query += "&comp=blocklist";

            return Ok(new { url = uriBuilder.Uri.ToString() });
        }
        // [HttpGet("get-iiif-image")]
        [HttpGet("get-iiif-image/{identifier}/{region}/{size}/{rotation}/{quality}.{format}")]
        public IActionResult GetIiifImage(
            string identifier,
            string region,
            string size,
            string rotation,
            string quality,
            string format
        )
        {
            //string size, rotation, quality, format = "";
            // 1) Load original image (e.g., from disk or blob)
            // string imagePath = Path.Combine("wwwroot", "images", identifier);
            // if (!System.IO.File.Exists(imagePath))
            // {
            //     return NotFound("Source image not found.");
            // }
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(identifier);
            using MemoryStream memoryStream = new MemoryStream();
            var inputData = blobClient.DownloadStreaming().Value.Content;// (identifier);//System.IO.File.ReadAllBytes(imagePath);
            using var originalBitmap = SKBitmap.Decode(inputData);
            if (originalBitmap == null)
            {
                return BadRequest("Failed to decode source image.");
            }

            // 2) Parse region
            //    region could be "full" or "x,y,w,h"
            SKBitmap regionBitmap = originalBitmap;
            if (!region.Equals("full", StringComparison.OrdinalIgnoreCase))
            {
                // Example: "100,50,400,300"
                var coords = region.Split(',');
                if (coords.Length == 4)
                {
                    int x = int.Parse(coords[0]);
                    int y = int.Parse(coords[1]);
                    int w = int.Parse(coords[2]);
                    int h = int.Parse(coords[3]);

                    if (x < 0 || y < 0 || w <= 0 || h <= 0 ||
                        x + w > originalBitmap.Width ||
                        y + h > originalBitmap.Height)
                    {
                        return BadRequest("Invalid region specified.");
                    }

                    // Crop
                    regionBitmap = new SKBitmap(w, h);
                    using (var canvas = new SKCanvas(regionBitmap))
                    {
                        var srcRect = new SKRectI(x, y, x + w, y + h);
                        var destRect = new SKRectI(0, 0, w, h);
                        canvas.DrawBitmap(originalBitmap, srcRect, destRect);
                    }
                }
            }

            // 3) Parse size
            //    examples: "full", "300,", ",200", "pct:50"
            var targetWidth = regionBitmap.Width;
            var targetHeight = regionBitmap.Height;
            if (!size.Equals("full", StringComparison.OrdinalIgnoreCase))
            {
                // quick parse examples
                if (size.StartsWith("pct:", StringComparison.OrdinalIgnoreCase))
                {
                    // "pct:50" => scale by 0.5
                    var pctStr = size.Substring(4);
                    if (float.TryParse(pctStr, out float pct))
                    {
                        float factor = pct / 100f;
                        targetWidth = (int)(regionBitmap.Width * factor);
                        targetHeight = (int)(regionBitmap.Height * factor);
                    }
                }
                else if (size.EndsWith(",")) 
                {
                    // "300," => width=300, height proportional
                    var wStr = size.Replace(",", "");
                    if (int.TryParse(wStr, out int w))
                    {
                        targetWidth = w;
                        float ratio = (float)w / regionBitmap.Width;
                        targetHeight = (int)(regionBitmap.Height * ratio);
                    }
                }
                else if (size.StartsWith(",")) 
                {
                    // ",300" => height=300, width proportional
                    var hStr = size.Replace(",", "");
                    if (int.TryParse(hStr, out int h))
                    {
                        targetHeight = h;
                        float ratio = (float)h / regionBitmap.Height;
                        targetWidth = (int)(regionBitmap.Width * ratio);
                    }
                }
                else
                {
                    // "300,400" => exact
                    var dims = size.Split(',');
                    if (dims.Length == 2 &&
                        int.TryParse(dims[0], out int w) &&
                        int.TryParse(dims[1], out int h))
                    {
                        targetWidth = w;
                        targetHeight = h;
                    }
                }
            }

            // 4) Resize if needed
            using var sizedBitmap = new SKBitmap(targetWidth, targetHeight);
            using (var canvas = new SKCanvas(sizedBitmap))
            {
                var srcRect = new SKRectI(0, 0, regionBitmap.Width, regionBitmap.Height);
                var destRect = new SKRect(0, 0, targetWidth, targetHeight);
                canvas.DrawBitmap(regionBitmap, srcRect, destRect);
            }

            // 5) Parse rotation (e.g. "0", "45", "90")
            if (float.TryParse(rotation, out float angleDegrees) && angleDegrees != 0)
            {
                // We'll create a new SKBitmap with bounding box for the rotation
                // (Simple approach for demonstration)
                float radians = angleDegrees * (float)(Math.PI / 180.0);

                // bounding box naive approach 
                var rotateWidth = sizedBitmap.Width;
                var rotateHeight = sizedBitmap.Height;
                using var rotatedBitmap = new SKBitmap(rotateWidth, rotateHeight);
                using (var rotCanvas = new SKCanvas(rotatedBitmap))
                {
                    rotCanvas.Clear(SKColors.Transparent);
                    rotCanvas.Translate(rotateWidth / 2f, rotateHeight / 2f);
                    rotCanvas.RotateDegrees(angleDegrees);
                    rotCanvas.Translate(-sizedBitmap.Width / 2f, -sizedBitmap.Height / 2f);
                    rotCanvas.DrawBitmap(sizedBitmap, new SKPoint(0, 0));
                }

                // override sizedBitmap with rotated content
                sizedBitmap.Dispose();
                // Copy the rotated result back or handle differently
                // For demo simplicity, let's rename:
                var finalBitmap = rotatedBitmap;
                
                // 6) Parse quality
                // ignoring for brevity, or maybe do color transformations
                // e.g. if (quality == "gray") => convert to grayscale
                // ... code ...
                
                // 7) Choose format
                var mime = "image/png";
                SKEncodedImageFormat skFormat = SKEncodedImageFormat.Png;
                if (format.Equals("jpg", StringComparison.OrdinalIgnoreCase) ||
                    format.Equals("jpeg", StringComparison.OrdinalIgnoreCase))
                {
                    skFormat = SKEncodedImageFormat.Jpeg;
                    mime = "image/jpeg";
                }
                else if (format.Equals("webp", StringComparison.OrdinalIgnoreCase))
                {
                    skFormat = SKEncodedImageFormat.Webp;
                    mime = "image/webp";
                }
                // else default to PNG

                // 8) Encode and return
                using var outStream = new MemoryStream();
                using var skImage = SKImage.FromBitmap(finalBitmap);
                using var data = skImage.Encode(skFormat, 90);
                data.SaveTo(outStream);
                return File(outStream.ToArray(), mime);
            }
            else
            {
                // No rotation or zero rotation
                // parse quality (omitted for brevity)
                // parse format
                var mime = "image/png";
                var skFormat = SKEncodedImageFormat.Png;
                if (format.Equals("jpg", StringComparison.OrdinalIgnoreCase) || 
                    format.Equals("jpeg", StringComparison.OrdinalIgnoreCase))
                {
                    skFormat = SKEncodedImageFormat.Jpeg;
                    mime = "image/jpeg";
                }
                else if (format.Equals("webp", StringComparison.OrdinalIgnoreCase))
                {
                    skFormat = SKEncodedImageFormat.Webp;
                    mime = "image/webp";
                }

                using var outStream = new MemoryStream();
                using var skImage = SKImage.FromBitmap(sizedBitmap);
                using var data = skImage.Encode(skFormat, 90);
                data.SaveTo(outStream);
                return File(outStream.ToArray(), mime);
            }
        }
    }
}

    // [Route("iiif/skia/{identifier}/{region}/{size}/{rotation}/{quality}.{format}")]
    // public class SkiaIiifController : ControllerBase
    // {
    //     private readonly IConfiguration _configuration;
    //     private readonly BlobServiceClient _blobServiceClient;
    //     private readonly string _containerName;
    //     public SkiaIiifController(BlobServiceClient blobServiceClient, IConfiguration configuration, string containerName = "dach-file-controller-blobs")
    //     {
    //         _configuration = configuration;
    //         _containerName = containerName;
    //         _blobServiceClient = blobServiceClient;
    //         var _blobContainerClient = blobServiceClient.GetBlobContainerClient(_containerName);
            
    //         Console.WriteLine($"Account Name: {_blobServiceClient.AccountName}");
    //         try
    //         {
    //             if(!_blobContainerClient.Exists()) 
    //             {
    //                 var container = _blobServiceClient.CreateBlobContainer(_containerName);
    //                 Console.WriteLine($"Container: {container?.Value.Uri.ToString()??"Container Creation Failed"}");
    //             }
    //         }
    //         catch (Exception ex)
    //         {
    //             Console.WriteLine($"{ex.Message}");
    //         }
    //         Console.WriteLine($"Using Container: {_containerName}");
    //     }
    //     /// <summary>
    //     /// Example IIIF-like endpoint using SkiaSharp.
    //     /// GET /iiif/skia/sample.jpg/full/300,/90/default.jpg
    //     /// </summary>
    //     [HttpGet]
    //     public IActionResult GetIiifImage(
    //         string identifier,
    //         string region,
    //         string size,
    //         string rotation,
    //         string quality,
    //         string format)
    //     {
    //         // 1) Load original image (e.g., from disk or blob)
    //         // string imagePath = Path.Combine("wwwroot", "images", identifier);
    //         // if (!System.IO.File.Exists(imagePath))
    //         // {
    //         //     return NotFound("Source image not found.");
    //         // }
    //         var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
    //         var blobClient = containerClient.GetBlobClient(identifier);
    //         using MemoryStream memoryStream = new MemoryStream();
    //         var inputData = blobClient.DownloadStreaming().Value.Content;// (identifier);//System.IO.File.ReadAllBytes(imagePath);
    //         using var originalBitmap = SKBitmap.Decode(inputData);
    //         if (originalBitmap == null)
    //         {
    //             return BadRequest("Failed to decode source image.");
    //         }

    //         // 2) Parse region
    //         //    region could be "full" or "x,y,w,h"
    //         SKBitmap regionBitmap = originalBitmap;
    //         if (!region.Equals("full", StringComparison.OrdinalIgnoreCase))
    //         {
    //             // Example: "100,50,400,300"
    //             var coords = region.Split(',');
    //             if (coords.Length == 4)
    //             {
    //                 int x = int.Parse(coords[0]);
    //                 int y = int.Parse(coords[1]);
    //                 int w = int.Parse(coords[2]);
    //                 int h = int.Parse(coords[3]);

    //                 if (x < 0 || y < 0 || w <= 0 || h <= 0 ||
    //                     x + w > originalBitmap.Width ||
    //                     y + h > originalBitmap.Height)
    //                 {
    //                     return BadRequest("Invalid region specified.");
    //                 }

    //                 // Crop
    //                 regionBitmap = new SKBitmap(w, h);
    //                 using (var canvas = new SKCanvas(regionBitmap))
    //                 {
    //                     var srcRect = new SKRectI(x, y, x + w, y + h);
    //                     var destRect = new SKRectI(0, 0, w, h);
    //                     canvas.DrawBitmap(originalBitmap, srcRect, destRect);
    //                 }
    //             }
    //         }

    //         // 3) Parse size
    //         //    examples: "full", "300,", ",200", "pct:50"
    //         var targetWidth = regionBitmap.Width;
    //         var targetHeight = regionBitmap.Height;
    //         if (!size.Equals("full", StringComparison.OrdinalIgnoreCase))
    //         {
    //             // quick parse examples
    //             if (size.StartsWith("pct:", StringComparison.OrdinalIgnoreCase))
    //             {
    //                 // "pct:50" => scale by 0.5
    //                 var pctStr = size.Substring(4);
    //                 if (float.TryParse(pctStr, out float pct))
    //                 {
    //                     float factor = pct / 100f;
    //                     targetWidth = (int)(regionBitmap.Width * factor);
    //                     targetHeight = (int)(regionBitmap.Height * factor);
    //                 }
    //             }
    //             else if (size.EndsWith(",")) 
    //             {
    //                 // "300," => width=300, height proportional
    //                 var wStr = size.Replace(",", "");
    //                 if (int.TryParse(wStr, out int w))
    //                 {
    //                     targetWidth = w;
    //                     float ratio = (float)w / regionBitmap.Width;
    //                     targetHeight = (int)(regionBitmap.Height * ratio);
    //                 }
    //             }
    //             else if (size.StartsWith(",")) 
    //             {
    //                 // ",300" => height=300, width proportional
    //                 var hStr = size.Replace(",", "");
    //                 if (int.TryParse(hStr, out int h))
    //                 {
    //                     targetHeight = h;
    //                     float ratio = (float)h / regionBitmap.Height;
    //                     targetWidth = (int)(regionBitmap.Width * ratio);
    //                 }
    //             }
    //             else
    //             {
    //                 // "300,400" => exact
    //                 var dims = size.Split(',');
    //                 if (dims.Length == 2 &&
    //                     int.TryParse(dims[0], out int w) &&
    //                     int.TryParse(dims[1], out int h))
    //                 {
    //                     targetWidth = w;
    //                     targetHeight = h;
    //                 }
    //             }
    //         }

    //         // 4) Resize if needed
    //         using var sizedBitmap = new SKBitmap(targetWidth, targetHeight);
    //         using (var canvas = new SKCanvas(sizedBitmap))
    //         {
    //             var srcRect = new SKRectI(0, 0, regionBitmap.Width, regionBitmap.Height);
    //             var destRect = new SKRect(0, 0, targetWidth, targetHeight);
    //             canvas.DrawBitmap(regionBitmap, srcRect, destRect);
    //         }

    //         // 5) Parse rotation (e.g. "0", "45", "90")
    //         if (float.TryParse(rotation, out float angleDegrees) && angleDegrees != 0)
    //         {
    //             // We'll create a new SKBitmap with bounding box for the rotation
    //             // (Simple approach for demonstration)
    //             float radians = angleDegrees * (float)(Math.PI / 180.0);

    //             // bounding box naive approach 
    //             var rotateWidth = sizedBitmap.Width;
    //             var rotateHeight = sizedBitmap.Height;
    //             using var rotatedBitmap = new SKBitmap(rotateWidth, rotateHeight);
    //             using (var rotCanvas = new SKCanvas(rotatedBitmap))
    //             {
    //                 rotCanvas.Clear(SKColors.Transparent);
    //                 rotCanvas.Translate(rotateWidth / 2f, rotateHeight / 2f);
    //                 rotCanvas.RotateDegrees(angleDegrees);
    //                 rotCanvas.Translate(-sizedBitmap.Width / 2f, -sizedBitmap.Height / 2f);
    //                 rotCanvas.DrawBitmap(sizedBitmap, new SKPoint(0, 0));
    //             }

    //             // override sizedBitmap with rotated content
    //             sizedBitmap.Dispose();
    //             // Copy the rotated result back or handle differently
    //             // For demo simplicity, let's rename:
    //             var finalBitmap = rotatedBitmap;
                
    //             // 6) Parse quality
    //             // ignoring for brevity, or maybe do color transformations
    //             // e.g. if (quality == "gray") => convert to grayscale
    //             // ... code ...
                
    //             // 7) Choose format
    //             var mime = "image/png";
    //             SKEncodedImageFormat skFormat = SKEncodedImageFormat.Png;
    //             if (format.Equals("jpg", StringComparison.OrdinalIgnoreCase) ||
    //                 format.Equals("jpeg", StringComparison.OrdinalIgnoreCase))
    //             {
    //                 skFormat = SKEncodedImageFormat.Jpeg;
    //                 mime = "image/jpeg";
    //             }
    //             else if (format.Equals("webp", StringComparison.OrdinalIgnoreCase))
    //             {
    //                 skFormat = SKEncodedImageFormat.Webp;
    //                 mime = "image/webp";
    //             }
    //             // else default to PNG

    //             // 8) Encode and return
    //             using var outStream = new MemoryStream();
    //             using var skImage = SKImage.FromBitmap(finalBitmap);
    //             using var data = skImage.Encode(skFormat, 90);
    //             data.SaveTo(outStream);
    //             return File(outStream.ToArray(), mime);
    //         }
    //         else
    //         {
    //             // No rotation or zero rotation
    //             // parse quality (omitted for brevity)
    //             // parse format
    //             var mime = "image/png";
    //             var skFormat = SKEncodedImageFormat.Png;
    //             if (format.Equals("jpg", StringComparison.OrdinalIgnoreCase) || 
    //                 format.Equals("jpeg", StringComparison.OrdinalIgnoreCase))
    //             {
    //                 skFormat = SKEncodedImageFormat.Jpeg;
    //                 mime = "image/jpeg";
    //             }
    //             else if (format.Equals("webp", StringComparison.OrdinalIgnoreCase))
    //             {
    //                 skFormat = SKEncodedImageFormat.Webp;
    //                 mime = "image/webp";
    //             }

    //             using var outStream = new MemoryStream();
    //             using var skImage = SKImage.FromBitmap(sizedBitmap);
    //             using var data = skImage.Encode(skFormat, 90);
    //             data.SaveTo(outStream);
    //             return File(outStream.ToArray(), mime);
    //         }
    //     }
    // }
    public static class FileControllerHelpers
    {
        public static string GetUid(this BlobItem item)
        {
            return item.Name.GetHashCode().ToString("x");
        }
        public static string GetUid(string item_name)
        {
            return item_name.GetHashCode().ToString("x");
        }
    }

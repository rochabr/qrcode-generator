using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QRCoder;
using static QRCoder.PayloadGenerator;

namespace Microsoft.Samples
{
    public static class QRCodeGenerator
    {
        [FunctionName("QRCodeGenerator")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string url = req.Query["url"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            url = url ?? data?.url;
            if (string.IsNullOrEmpty(url))
            {
                return new BadRequestResult();
            }
            var isAbsoluteUrl = Uri.TryCreate(url, UriKind.Absolute, out Uri resultUrl);
            if (!isAbsoluteUrl)
            {
                return new BadRequestResult();
            }

            var generator = new Url(resultUrl.AbsoluteUri);
            var payload = generator.ToString();

            using (var qrGenerator = new QRCoder.QRCodeGenerator())
            {
                var qrCodeData = qrGenerator.CreateQrCode(payload, QRCoder.QRCodeGenerator.ECCLevel.Q);
                var qrCode = new PngByteQRCode(qrCodeData);
                var qrCodeAsPng = qrCode.GetGraphic(20);
                return new FileContentResult(qrCodeAsPng, "image/png");
            }
        }
    }
}

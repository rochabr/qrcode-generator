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

            string reqData = req.Query["ReqData"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            reqData = reqData ?? data?.url;
            if (string.IsNullOrEmpty(reqData))
            {
                return new BadRequestResult();
            }

            var payload = reqData.ToString();
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

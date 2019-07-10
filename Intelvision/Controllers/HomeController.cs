using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Intelvision.Controllers
{
    public class HomeController : Controller
    {
        const string subscriptionKey = "830caf73d666465da310e997fe1d01b7";
        const string uriBase =
                   "https://intelvision.cognitiveservices.azure.com/vision/v2.0/ocr";
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult UploadFile()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> UploadFile(HttpPostedFileBase file)
        {
            try
            {
                byte[] data = null;
                if (file.ContentLength > 0)
                {
                    string _FileName = Path.GetFileName(file.FileName);
                    
                    using (Stream inputStream = file.InputStream)
                    {
                        MemoryStream memoryStream = inputStream as MemoryStream;
                        if (memoryStream == null)
                        {
                            memoryStream = new MemoryStream();
                            inputStream.CopyTo(memoryStream);
                        }
                        data = memoryStream.ToArray();
                    }

                }

               var result = await ProcessImage(data);

                ViewBag.OCRResponse = result.Data;
                ViewBag.ImageResponse = result.Data;
                return View();
            }
            catch
            {
                ViewBag.Message = "File upload failed!!";
                return View();
            }
        }

        [HttpPost]
        public async Task<JsonResult> ProcessImage(byte[] data)
        {
            JsonResult re = new JsonResult();
            try
            {
                HttpClient client = new HttpClient();
               
                // Request headers.
                client.DefaultRequestHeaders.Add(
                    "Ocp-Apim-Subscription-Key", subscriptionKey);

                // Request parameters. 
                // The language parameter doesn't specify a language, so the 
                // method detects it automatically.
                // The detectOrientation parameter is set to true, so the method detects and
                // and corrects text orientation before detecting text.
                string requestParameters = "language=unk&detectOrientation=true";

                // Assemble the URI for the REST API method.
                string uri = uriBase + "?" + requestParameters;

                HttpResponseMessage response;

          

                // Add the byte array as an octet stream to the request body.
                using (ByteArrayContent content = new ByteArrayContent(data))
                {
                    // This example uses the "application/octet-stream" content type.
                    // The other content types you can use are "application/json"
                    // and "multipart/form-data".
                    content.Headers.ContentType =
                        new MediaTypeHeaderValue("application/octet-stream");

                    // Asynchronously call the REST API method.
                    response = await client.PostAsync(uri, content);
                }

                // Asynchronously get the JSON response.
                string contentString = await response.Content.ReadAsStringAsync();
               
                re.ContentType = "application/json";
                re.Data = JToken.Parse(contentString);


                
            }
            catch (Exception e)
            {
                Console.WriteLine("\n" + e.Message);
            }
            return re;
        }

      

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
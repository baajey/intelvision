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
using Newtonsoft.Json;
using System.Text;

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
        public async Task<FileResult> UploadFile(HttpPostedFileBase file)
        {
            string _FileName = string.Empty;
            try
            {
                byte[] data = null;
                if (file.ContentLength > 0)
                {
                     _FileName = Path.GetFileName(file.FileName);
                    
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
                //return View("AutoUI", ViewBag);

                byte[] fileBytes = Encoding.ASCII.GetBytes(result); // result.Toc;
                string fileName = $"{_FileName + ".html"}";
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            }
            catch
            {
                ViewBag.Message = "File upload failed!!";
                return null;
            }
        }


        public ActionResult AutoUI()
        {
            return View();
        }

        [HttpPost]
        public async Task<string> ProcessImage(byte[] data)
        {
            string htmlresult = string.Empty;
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
                var obj2 = JsonConvert.SerializeObject(contentString);
               //var obj = new JsonSerializer().Serialize()
             
                
                //send data to api for html generation
                string apiURI = "https://demohtmlgenerator.azurewebsites.net/api/Values/DecryptOCR";
                HttpClient apiclient = new HttpClient();
                string apirequestjson = "{\"OCRValue\":" + obj2 + "}";
                var aicontent = new StringContent(apirequestjson, Encoding.UTF8, "application/json");
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls | System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls12;//System.Net.SecurityProtocolType.Ssl3;// Expect100Continue = false;
                var htmlResponse = await apiclient.PostAsync(apiURI, aicontent);
                htmlresult = await htmlResponse.Content.ReadAsStringAsync();
               
            }
            catch (Exception e)
            {
                Console.WriteLine("\n" + e.Message);
            }
            return htmlresult;
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
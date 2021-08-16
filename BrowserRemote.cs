using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PuppeteerSharp;

namespace FunctionBrowserRemote
{
    public static class BrowserRemote
    {
        [FunctionName("BrowserRemote")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try {
                var beginProcess = DateTime.Now;
                log.LogInformation ("C# HTTP trigger function processed a request.");
                await new BrowserFetcher ().DownloadAsync (BrowserFetcher.DefaultRevision);

                Core.Chromium step = new Core.Chromium();
                Dto.SuccessResponse resp = new Dto.SuccessResponse();

                resp.Date = DateTime.Now.ToString ("yyyy-MM-dd hh:mm:ss");
                resp.Response = "success";

                string body = await new StreamReader (req.Body).ReadToEndAsync ();
                dynamic data = JsonConvert.DeserializeObject (body);

                if (!body.Equals ("")) {
                    //Run puppeteer
                    using (var browser = await Puppeteer.LaunchAsync (new LaunchOptions {
                        Headless = true,
                            Args = new [] { "--no-sandbox" },
                            DefaultViewport = new ViewPortOptions { Width = 2000, Height = 800 }
                    }))
                    //page chromium
                    using (var page = await browser.NewPageAsync ()) {
                        string version = await browser.GetVersionAsync ();
                        string url = data.Url;

                        log.LogInformation ("Version: {0}", version);
                        log.LogInformation ("URL: {0}", url);

                        await page.GoToAsync (url);
                        foreach (var item in data.Steps) {
                            string botEvent = (string) item.BrowserEvent;
                            if (botEvent.Equals ("input"))
                                await step.InputSelectorAsync (page, (string) item.Selector, (string) item.ValueSelector, log);
                            if (botEvent.Equals ("click"))
                                await step.ClickSelectorAsync (page, (string) item.Selector, (bool) item.WaitForNavigation, log);
                            if (botEvent.Equals ("select"))
                                await step.SelectSelectorAsync (page, (string) item.Selector, (string) item.ValueSelector, log);
                            if (botEvent.Equals ("getText"))
                                await step.TextSelectorAsync (page, (string) item.Selector, log);
                            if (botEvent.Equals ("getHtml"))
                                await step.HtmlSelectorAsync (page, (string) item.Selector, log);
                            if (botEvent.Equals ("screenshot"))
                                await step.ScreenshotSelectorAsync (page, (string) item.Selector, (string) item.FullPathPNG, log);

                            if (data.Steps.IndexOf (item) == data.Steps.Count - 1) {
                                if (botEvent.Equals ("getText"))
                                    resp.Response = await step.TextSelectorAsync (page, (string) item.Selector, log);
                                if (botEvent.Equals ("getHtml"))
                                    resp.Response = await step.HtmlSelectorAsync (page, (string) item.Selector, log);
                            }
                        }
                        await browser.CloseAsync ();
                        //Response success
                        var EndProcess = DateTime.Now;
                        TimeSpan timerProcess = EndProcess - beginProcess;
                        resp.Timer = timerProcess.Seconds.ToString ();
                        resp.Version = version;

                        return (ActionResult) new OkObjectResult (resp);
                    }
                } else {
                    Dto.BadResponse respError = new Dto.BadResponse ();
                    respError.Mesage = "Please pass the request body";
                    return new BadRequestObjectResult (respError);
                }
                return (ActionResult) new OkObjectResult (resp);

            } catch (Exception ex) {

                Dto.BadResponse err = new Dto.BadResponse ();
                err.Mesage = ex.Message + " InnerException: " + ex.InnerException;
                return new BadRequestObjectResult (err);
            }
        }
    }
}

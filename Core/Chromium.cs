using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using PuppeteerSharp;

namespace FunctionBrowserRemote.Core
{
    public class Chromium
    {
        public async Task ScreenshotSelectorAsync (Page page, string selector, string pathFilePNG, ILogger log) {
            try {
                log.LogInformation ("Begin - ScreenshotSelectorAsync()");
                ElementHandle bodyHandle = await page.WaitForSelectorAsync ("body");
                BoundingBox bounding_box = await bodyHandle.BoundingBoxAsync ();

                await page.SetViewportAsync (new ViewPortOptions {
                    Width = Convert.ToInt32 (Math.Max (page.Viewport.Width, Math.Ceiling (bounding_box.Width))),
                        Height = Convert.ToInt32 (Math.Max (page.Viewport.Height, Math.Ceiling (bounding_box.Height))),
                });

                ElementHandle element = await page.WaitForSelectorAsync (selector);
                await element.ScreenshotAsync (pathFilePNG);
                await element.DisposeAsync ();
                log.LogInformation ("End - ScreenshotSelectorAsync()");
            } catch (Exception ex) {
                log.LogError ("Erro: {0}", ex.Message);
            }
        }

        public async Task InputSelectorAsync (Page page, string selector, string dataInput, ILogger log) {
            try {
                log.LogInformation ("Begin - InputSelectorAsync()");
                await page.WaitForSelectorAsync (selector);
                await page.FocusAsync (selector);
                await page.Keyboard.TypeAsync (dataInput);
                log.LogInformation ("End - InputSelectorAsync()");
            } catch (Exception ex) {
                log.LogError ("Erro: {0}", ex.Message);
            }
        }

        public async Task ClickSelectorAsync (Page page, string selector, bool WaitForNavigation, ILogger log) {
            try {
                log.LogInformation ("Begin - ClickSelectorAsync()");
                await page.ClickAsync (selector);
                if (WaitForNavigation) {
                    await page.WaitForNavigationAsync (new NavigationOptions { WaitUntil = new [] { WaitUntilNavigation.Load }, Timeout = 4000 });
                }
                log.LogInformation ("End - ClickSelectorAsync()");
            } catch (Exception ex) {
                log.LogError ("Erro: {0}", ex.Message);
            }
        }

        public async Task<string> TextSelectorAsync (Page page, string selector, ILogger log) {
            try {
                log.LogInformation ("Begin - TextSelectorAsync()");
                await page.WaitForSelectorAsync (selector);
                JToken text = await page.EvaluateExpressionAsync ("document.querySelector('" + selector + "').innerText");
                log.LogInformation ("End - TextSelectorAsync()");
                return text.ToString ();
            } catch (Exception ex) {
                log.LogError ("Erro: {0}", ex.Message);
                return ex.Message;
            }
        }

        public async Task<string> HtmlSelectorAsync (Page page, string selector, ILogger log) {
            try {
                log.LogInformation ("Begin - HtmlSelectorAsync()");
                await page.WaitForSelectorAsync (selector);
                JToken text = await page.EvaluateExpressionAsync ("document.querySelector('" + selector + "').innerHTML");
                log.LogInformation ("End - HtmlSelectorAsync()");
                return text.ToString ();
            } catch (Exception ex) {
                log.LogError ("Erro: {0}", ex.Message);
                return ex.Message;
            }
        }

        public async Task SelectSelectorAsync (Page page, string selector, string valueSelect, ILogger log) {
            try {
                log.LogInformation ("Begin - SelectSelectorAsync()");
                await page.SelectAsync (selector, valueSelect);
                log.LogInformation ("End - SelectSelectorAsync()");
            } catch (Exception ex) {
                log.LogError ("Erro: {0}", ex.Message);
            }
        }
    }
}
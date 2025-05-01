using PuppeteerSharp;

namespace CertificateMonitor.Services
{
    public class BrowserMonitor
    {
        public static async Task<string> GetBrowserUrl(string processName)
        {
            if (processName.ToLower().Contains("chrome") || processName.ToLower().Contains("msedge"))
            {
                try
                {
                    await new BrowserFetcher().DownloadAsync();
                    using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
                    var page = await browser.PagesAsync().ContinueWith(t => t.Result[0]);
                    var url = await page.GetContentAsync().ContinueWith(t => page.Url);
                    await browser.CloseAsync();
                    return url;
                }
                catch
                {
                    return "Unknown";
                }
            }
            return "Not a browser";
        }
    }
}
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Playwright;
using SkiaSharp;

namespace Map_Svg.Tests
{
    // Renders SVG files in Chromium, through Playwright: one headless browser for the whole run, started on first
    // use, and one page for each rendering, at one device pixel to each CSS pixel.
    public static class ChromiumRenderer
    {
        // The browser, once started.
        private static readonly Lazy<Task<IBrowser>> browser = new Lazy<Task<IBrowser>>(LaunchBrowserAsync);

        // The Playwright driver that started the browser.
        private static IPlaywright playwright;

        // Start Playwright and its Chromium.
        private static async Task<IBrowser> LaunchBrowserAsync()
        {
            playwright = await Playwright.CreateAsync();
            return await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        }

        // Render an SVG file as an image of the given size, on white: the document, whose own size may be in
        // millimetres, is scaled to fill the image.
        // svgPath: the path of the SVG file.
        // pixelSize: the size of the image in pixels.
        public static async Task<SKBitmap> RenderAsync(string svgPath, Size pixelSize)
        {
            IBrowser chromium = await browser.Value;
            IPage page = await chromium.NewPageAsync(new BrowserNewPageOptions {
                ViewportSize = new ViewportSize { Width = pixelSize.Width, Height = pixelSize.Height },
                DeviceScaleFactor = 1,
            });
            try {
                await page.GotoAsync(new Uri(WritePageShowing(svgPath, pixelSize)).AbsoluteUri);
                byte[] png = await page.ScreenshotAsync(new PageScreenshotOptions { Type = ScreenshotType.Png });
                return SKBitmap.Decode(png);
            }
            finally {
                await page.CloseAsync();
            }
        }

        // Write an HTML page that shows an SVG file as an image of the given size at its top left corner, as a browser
        // shows a standalone SVG document, and return the page's path.
        // svgPath: the path of the SVG file, whose name names the page.
        // pixelSize: the size of the image in pixels.
        private static string WritePageShowing(string svgPath, Size pixelSize)
        {
            string directory = Path.Combine(Path.GetTempPath(), "svg-loop-pages");
            Directory.CreateDirectory(directory);
            string pagePath = Path.Combine(directory, Path.GetFileNameWithoutExtension(svgPath) + ".html");
            File.WriteAllText(pagePath,
                "<!DOCTYPE html><html><head><style>html,body{margin:0;background:#fff}img{display:block}</style></head><body>" +
                "<img src=\"" + new Uri(svgPath).AbsoluteUri + "\" width=\"" + pixelSize.Width + "\" height=\"" + pixelSize.Height + "\">" +
                "</body></html>");
            return pagePath;
        }

        // Close the browser and the Playwright driver, if they were started.
        public static async Task CloseAsync()
        {
            if (!browser.IsValueCreated)
                return;

            IBrowser chromium = await browser.Value;
            await chromium.CloseAsync();
            playwright.Dispose();
        }
    }
}

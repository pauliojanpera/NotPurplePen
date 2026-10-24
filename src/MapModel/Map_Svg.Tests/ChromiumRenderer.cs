using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Security;
using System.Text.Json;
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

        // Start Playwright and its Chromium. On Linux the browser sees no font of the system's but the sentinel, whose
        // every glyph is a filled box: a document must draw its text in the faces it embeds, so a character that a
        // browser would draw in a font of the machine's shows as a box and fails the comparison, instead of passing in
        // a system font that happens to look the same (T-8). Chromium draws no text at all without one system font.
        // Chromium hints glyphs unless told not to, which moves them by up to three pixels off the positions the
        // shaping gives, so the browser draws text unhinted, in grayscale, and in sRGB, wherever the tests run: some
        // installations of the headless shell start it so, and the one GitHub's runners install does not.
        private static async Task<IBrowser> LaunchBrowserAsync()
        {
            playwright = await Playwright.CreateAsync();
            BrowserTypeLaunchOptions options = new BrowserTypeLaunchOptions {
                Headless = true,
                Args = new[] { "--font-render-hinting=none", "--disable-lcd-text", "--force-color-profile=srgb" },
            };
            if (OperatingSystem.IsLinux()) {
                Dictionary<string, string> environment = new Dictionary<string, string>(StringComparer.Ordinal);
                foreach (DictionaryEntry variable in Environment.GetEnvironmentVariables())
                    environment[(string)variable.Key] = (string)variable.Value;
                environment["FONTCONFIG_FILE"] = WriteSentinelFontConfig();
                options.Env = environment;
            }
            return await playwright.Chromium.LaunchAsync(options);
        }

        // Write a fontconfig file that offers the sentinel font alone, in a folder of this run's, and return its path.
        private static string WriteSentinelFontConfig()
        {
            string folder = Path.Combine(Path.GetTempPath(), "svg-tests-fonts-" + Environment.ProcessId.ToString(CultureInfo.InvariantCulture));
            string fonts = Path.Combine(folder, "fonts");
            Directory.CreateDirectory(fonts);
            File.Copy(Path.Combine(AppContext.BaseDirectory, "fonts", "Sentinel.ttf"), Path.Combine(fonts, "Sentinel.ttf"), true);
            string config = Path.Combine(folder, "fonts.conf");
            File.WriteAllText(config, "<?xml version=\"1.0\"?>\n<!DOCTYPE fontconfig SYSTEM \"fonts.dtd\">\n<fontconfig>\n" +
                                      "  <dir>" + SecurityElement.Escape(fonts) + "</dir>\n" +
                                      "  <cachedir>" + SecurityElement.Escape(Path.Combine(folder, "cache")) + "</cachedir>\n</fontconfig>\n");
            return config;
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

        // Read where Chromium puts each character of each text an SVG file draws, as it draws the file at an image's
        // size. The file is opened as a document of its own and sized to the image, its shapes are taken out, as the
        // texts do not depend on them, and each part it uses is put in place of its use, so that the texts are laid out
        // at the scale they are drawn at: a text inside a definition is laid out at the definition's own scale, where
        // Chromium caps its size. Each face is loaded before any text is measured. A line text, whose characters each
        // carry a position and a turn of their own (T-5), is left out: no shaping of its characters as one line holds.
        // svgPath: the path of the SVG file.
        // pixelSize: the size of the image in pixels.
        public static async Task<List<DrawnText>> ReadTextPositionsAsync(string svgPath, Size pixelSize)
        {
            IBrowser chromium = await browser.Value;
            IPage page = await chromium.NewPageAsync(new BrowserNewPageOptions {
                ViewportSize = new ViewportSize { Width = pixelSize.Width, Height = pixelSize.Height },
                DeviceScaleFactor = 1,
            });
            try {
                await page.GotoAsync(new Uri(svgPath).AbsoluteUri);
                string texts = await page.EvaluateAsync<string>(readTextPositions, new[] { pixelSize.Width, pixelSize.Height });
                return JsonSerializer.Deserialize<List<DrawnText>>(texts);
            }
            finally {
                await page.CloseAsync();
            }
        }

        // The script that reads the texts' positions, given the image's width and height, as JSON for DrawnText.
        private const string readTextPositions = @"async ([width, height]) => {
            const root = document.documentElement;
            root.setAttribute('width', width);
            root.setAttribute('height', height);
            for (const shape of Array.from(document.querySelectorAll('circle, ellipse, image, line, path, polygon, polyline, rect')))
                shape.remove();
            for (const use of Array.from(document.querySelectorAll('use'))) {
                const href = use.getAttribute('href') || use.getAttributeNS('http://www.w3.org/1999/xlink', 'href') || '';
                const definition = document.getElementById(href.replace(/^#/, ''));
                if (!definition || definition.localName !== 'g')
                    continue;
                const part = definition.closest('defs') ? definition : definition.cloneNode(true);
                part.removeAttribute('id');
                if (use.hasAttribute('transform'))
                    part.setAttribute('transform', use.getAttribute('transform'));
                use.replaceWith(part);
            }
            await Promise.all(Array.from(document.fonts).map(face => face.load()));
            await document.fonts.ready;
            const texts = [];
            for (const text of document.querySelectorAll('text')) {
                if (text.closest('defs') || text.hasAttribute('rotate'))
                    continue;
                const style = getComputedStyle(text);
                const matrix = text.getScreenCTM();
                const starts = [];
                for (let index = 0; index < text.getNumberOfChars(); ++index)
                    starts.push(text.getStartPositionOfChar(index).x);
                texts.push({ Text: text.textContent, Family: style.fontFamily, Size: parseFloat(style.fontSize),
                             Weight: parseInt(style.fontWeight, 10), Italic: style.fontStyle === 'italic',
                             Scale: Math.hypot(matrix.a, matrix.b), Starts: starts });
            }
            return JSON.stringify(texts);
        }";

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

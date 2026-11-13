using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Security;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;
using SkiaSharp;

namespace Map_Svg.Tests
{
    // The browsers that render SVG files: Chromium, which the rendering comparison renders in (F-1), and Firefox and
    // WebKit, which the browser check compares with it (F-5, V-6), each the build the Playwright package pins.
    public enum Browser
    {
        Chromium,
        Firefox,
        WebKit,
    }

    // Renders SVG files in the browsers, through Playwright: one headless browser of each kind for the whole run,
    // started on first use, and one page for each rendering, at one device pixel to each CSS pixel; and a Chromium of
    // its own for each locale a test reads a document in.
    public static class BrowserRenderer
    {
        // The Playwright driver that starts the browsers, once started.
        private static readonly Lazy<Task<IPlaywright>> playwright = new Lazy<Task<IPlaywright>>(Playwright.CreateAsync);

        // The fontconfig file that the browsers start with on Linux, once written.
        private static readonly Lazy<string> sentinelFontConfig = new Lazy<string>(WriteSentinelFontConfig);

        // The browsers, once started, by their kind and the locale each runs in: the empty name for the one of no locale
        // of its own.
        private static readonly ConcurrentDictionary<string, Lazy<Task<IBrowser>>> browsers =
            new ConcurrentDictionary<string, Lazy<Task<IBrowser>>>(StringComparer.Ordinal);

        // The Chromium that runs in a locale, started on first use.
        // locale: the locale, a language tag such as "ro-RO", or null for the browser of no locale of its own.
        private static Task<IBrowser> BrowserIn(string locale)
        {
            return browsers.GetOrAdd(locale ?? "", key => new Lazy<Task<IBrowser>>(() => LaunchBrowserAsync(key))).Value;
        }

        // A browser of a kind, of no locale of its own, started on first use.
        // browser: the kind of browser.
        private static Task<IBrowser> BrowserOf(Browser browser)
        {
            if (browser == Browser.Chromium)
                return BrowserIn(null);
            return browsers.GetOrAdd(browser.ToString(), key => new Lazy<Task<IBrowser>>(() => LaunchOtherBrowserAsync(browser))).Value;
        }

        // Start Chromium. On Linux the browser sees no font of the system's but the sentinel, whose every glyph is a
        // filled box: a document must draw its text in the faces it embeds, so a character that a browser would draw
        // in a font of the machine's shows as a box and fails the comparison, instead of passing in a system font that
        // happens to look the same (T-8). Chromium draws no text at all without one system font. Chromium hints glyphs
        // unless told not to, which moves them by up to three pixels off the positions the shaping gives, so the
        // browser draws text unhinted, in grayscale, and in sRGB, wherever the tests run: some installations of the
        // headless shell start it so, and the one GitHub's runners install does not. The fontconfig file turns hinting
        // off too, for Firefox; Chromium, told by its flag, draws alike with it and without.
        // A browser in a locale is started with --lang, and on Linux, where Chromium takes its locale from its
        // environment instead, with the locale's variables there, which need not name a locale the C library has.
        // locale: the locale, a language tag such as "ro-RO", or the empty string for none of its own.
        private static async Task<IBrowser> LaunchBrowserAsync(string locale)
        {
            IPlaywright driver = await playwright.Value;
            List<string> arguments = new List<string> { "--font-render-hinting=none", "--disable-lcd-text", "--force-color-profile=srgb" };
            if (locale.Length > 0)
                arguments.Add("--lang=" + locale);
            BrowserTypeLaunchOptions options = new BrowserTypeLaunchOptions { Headless = true, Args = arguments };
            if (OperatingSystem.IsLinux()) {
                Dictionary<string, string> environment = new Dictionary<string, string>(StringComparer.Ordinal);
                foreach (DictionaryEntry variable in Environment.GetEnvironmentVariables())
                    environment[(string)variable.Key] = (string)variable.Value;
                environment["FONTCONFIG_FILE"] = sentinelFontConfig.Value;
                if (locale.Length > 0) {
                    string localeName = locale.Replace('-', '_') + ".UTF-8";
                    environment["LANGUAGE"] = locale.Replace('-', '_');
                    environment["LC_ALL"] = localeName;
                    environment["LANG"] = localeName;
                }
                options.Env = environment;
            }
            return await driver.Chromium.LaunchAsync(options);
        }

        // Start Firefox or WebKit as Chromium is started, on Linux with the fontconfig file that offers the sentinel font
        // alone and turns hinting off. Firefox takes its hinting from fontconfig, and with fontconfig's defaults hints
        // glyphs as Chromium would without its flag. WebKit takes it from neither: it hints the faces a document embeds
        // whatever fontconfig, text-rendering or its own settings say.
        // browser: Firefox or WebKit.
        private static async Task<IBrowser> LaunchOtherBrowserAsync(Browser browser)
        {
            IPlaywright driver = await playwright.Value;
            BrowserTypeLaunchOptions options = new BrowserTypeLaunchOptions { Headless = true };
            if (OperatingSystem.IsLinux()) {
                Dictionary<string, string> environment = new Dictionary<string, string>(StringComparer.Ordinal);
                foreach (DictionaryEntry variable in Environment.GetEnvironmentVariables())
                    environment[(string)variable.Key] = (string)variable.Value;
                environment["FONTCONFIG_FILE"] = sentinelFontConfig.Value;
                options.Env = environment;
            }
            return await (browser == Browser.Firefox ? driver.Firefox : driver.Webkit).LaunchAsync(options);
        }

        // Write a fontconfig file that offers the sentinel font alone, with hinting off, in a folder of this run's, and
        // return its path.
        private static string WriteSentinelFontConfig()
        {
            string folder = Path.Combine(Path.GetTempPath(), "svg-tests-fonts-" + Environment.ProcessId.ToString(CultureInfo.InvariantCulture));
            string fonts = Path.Combine(folder, "fonts");
            Directory.CreateDirectory(fonts);
            File.Copy(Path.Combine(AppContext.BaseDirectory, "fonts", "Sentinel.ttf"), Path.Combine(fonts, "Sentinel.ttf"), true);
            string config = Path.Combine(folder, "fonts.conf");
            File.WriteAllText(config, "<?xml version=\"1.0\"?>\n<!DOCTYPE fontconfig SYSTEM \"fonts.dtd\">\n<fontconfig>\n" +
                                      "  <dir>" + SecurityElement.Escape(fonts) + "</dir>\n" +
                                      "  <cachedir>" + SecurityElement.Escape(Path.Combine(folder, "cache")) + "</cachedir>\n" +
                                      "  <match target=\"pattern\">\n" +
                                      "    <edit name=\"hinting\" mode=\"assign\"><bool>false</bool></edit>\n" +
                                      "    <edit name=\"hintstyle\" mode=\"assign\"><const>hintnone</const></edit>\n" +
                                      "    <edit name=\"autohint\" mode=\"assign\"><bool>false</bool></edit>\n" +
                                      "    <edit name=\"antialias\" mode=\"assign\"><bool>true</bool></edit>\n" +
                                      "    <edit name=\"rgba\" mode=\"assign\"><const>none</const></edit>\n" +
                                      "    <edit name=\"lcdfilter\" mode=\"assign\"><const>lcdnone</const></edit>\n" +
                                      "  </match>\n</fontconfig>\n");
            return config;
        }

        // Render an SVG file as an image of the given size, on white: the document, whose own size may be in
        // millimetres, is scaled to fill the image.
        // svgPath: the path of the SVG file.
        // pixelSize: the size of the image in pixels.
        // locale: the locale of the browser that renders it, a language tag such as "ro-RO", or null for the browser of
        // no locale of its own.
        // view: the id of a view of the document to show, as a link to the file with that fragment shows it, or null for
        // the document's own view box.
        public static async Task<SKBitmap> RenderAsync(string svgPath, Size pixelSize, string locale = null, string view = null)
        {
            return SKBitmap.Decode(await RenderPngAsync(await BrowserIn(locale), svgPath, pixelSize, view, 0));
        }

        // Render an SVG file in a browser as an image of the given size, on white, as RenderAsync renders it in Chromium,
        // and return the image as PNG data.
        // browser: the browser.
        // svgPath: the path of the SVG file.
        // pixelSize: the size of the image in pixels.
        // view: the id of a view of the document to show, or null for the document's own view box.
        public static async Task<byte[]> RenderPngAsync(Browser browser, string svgPath, Size pixelSize, string view = null)
        {
            return await RenderPngAsync(await BrowserOf(browser), svgPath, pixelSize, view, browser == Browser.Chromium ? 0 : FramesBeforeScreenshot);
        }

        // How many frames Firefox and WebKit draw a page in before its screenshot is taken. WebKit draws a pattern of a
        // few pixels a tile otherwise when the screenshot comes before the page's first frames, and otherwise again from
        // one rendering to the next; after two frames it draws it alike every time. Chromium draws alike either way, and
        // is shot as soon as it has loaded the page, as the rendering comparison has always shot it.
        private const int FramesBeforeScreenshot = 2;

        // How many times a rendering is tried before a browser's failure to take its screenshot fails it: Firefox now
        // and then answers that it is unable to capture one, while many pages are open at once.
        private const int ScreenshotAttempts = 3;

        // Render an SVG file in a started browser as an image of the given size, on white, and return it as PNG data. A
        // rendering whose screenshot the browser could not take is tried again on a page of its own.
        // browser: the browser.
        // svgPath: the path of the SVG file.
        // pixelSize: the size of the image in pixels.
        // view: the id of a view of the document to show, or null for the document's own view box.
        // frames: how many frames the browser draws the page in before the screenshot is taken.
        private static async Task<byte[]> RenderPngAsync(IBrowser browser, string svgPath, Size pixelSize, string view, int frames)
        {
            for (int attempt = 1; ; ++attempt) {
                try {
                    return await RenderPngOnceAsync(browser, svgPath, pixelSize, view, frames);
                }
                catch (PlaywrightException failure) when (attempt < ScreenshotAttempts && failure.Message.Contains("Unable to capture screenshot", StringComparison.Ordinal)) {
                    TestContext.Progress.WriteLine("Trying " + Path.GetFileName(svgPath) + " again: " + failure.Message.Split('\n')[0]);
                }
            }
        }

        // Render an SVG file in a started browser once, as RenderPngAsync renders it.
        // browser: the browser.
        // svgPath: the path of the SVG file.
        // pixelSize: the size of the image in pixels.
        // view: the id of a view of the document to show, or null for the document's own view box.
        // frames: how many frames the browser draws the page in before the screenshot is taken.
        private static async Task<byte[]> RenderPngOnceAsync(IBrowser browser, string svgPath, Size pixelSize, string view, int frames)
        {
            IPage page = await browser.NewPageAsync(new BrowserNewPageOptions {
                ViewportSize = new ViewportSize { Width = pixelSize.Width, Height = pixelSize.Height },
                DeviceScaleFactor = 1,
            });
            string pagePath = WritePageShowing(svgPath, pixelSize, view);
            try {
                await page.GotoAsync(new Uri(pagePath).AbsoluteUri);
                for (int frame = 0; frame < frames; ++frame)
                    await page.EvaluateAsync("() => new Promise(resolve => requestAnimationFrame(() => resolve()))");
                return await page.ScreenshotAsync(new PageScreenshotOptions { Type = ScreenshotType.Png });
            }
            finally {
                await page.CloseAsync();
                File.Delete(pagePath);
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
            IBrowser chromium = await BrowserIn(null);
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
        // shows a standalone SVG document, and return the page's path. Each rendering has a page of its own, as several
        // browsers may render documents of one name at once.
        // svgPath: the path of the SVG file, whose name names the page.
        // pixelSize: the size of the image in pixels.
        // view: the id of a view of the document to show, which the page's name carries too, or null for the document's
        // own view box.
        private static string WritePageShowing(string svgPath, Size pixelSize, string view)
        {
            string directory = Path.Combine(Path.GetTempPath(), "svg-loop-pages");
            Directory.CreateDirectory(directory);
            string pagePath = Path.Combine(directory, Path.GetFileNameWithoutExtension(svgPath) + (view == null ? "" : "-" + view) + "-" +
                                                      Guid.NewGuid().ToString("N") + ".html");
            string source = new Uri(svgPath).AbsoluteUri + (view == null ? "" : "#" + Uri.EscapeDataString(view));
            File.WriteAllText(pagePath,
                "<!DOCTYPE html><html><head><style>html,body{margin:0;background:#fff}img{display:block}</style></head><body>" +
                "<img src=\"" + source + "\" width=\"" + pixelSize.Width + "\" height=\"" + pixelSize.Height + "\">" +
                "</body></html>");
            return pagePath;
        }

        // Close the browsers and the Playwright driver, if they were started.
        public static async Task CloseAsync()
        {
            foreach (Lazy<Task<IBrowser>> browser in browsers.Values) {
                if (browser.IsValueCreated)
                    await (await browser.Value).CloseAsync();
            }
            if (playwright.IsValueCreated)
                (await playwright.Value).Dispose();
        }
    }
}

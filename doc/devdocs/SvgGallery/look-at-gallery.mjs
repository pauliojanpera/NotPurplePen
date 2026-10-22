// look-at-gallery.mjs <gallery url> <output folder>
//
// Looks at the SVG gallery before it is published: a desktop view in light and a phone view in dark, each
// scrolled through first so that every card loads its images, saved as full-page screenshots. Reports what a
// glance at them would miss: sideways overflow, details left blank, fonts that did not load, and errors.
// Serve the gallery folder first, for instance with `python3 -m http.server`. Playwright is imported from
// PLAYWRIGHT_MODULE, or else as "playwright".
const { chromium } = await import(process.env.PLAYWRIGHT_MODULE || "playwright");

const [url, out] = process.argv.slice(2);
const browser = await chromium.launch();

// Load the gallery in one browser context, scroll through it, and report and screenshot it.
async function look(name, options) {
  const context = await browser.newContext(options);
  const page = await context.newPage();
  const problems = [];
  page.on("console", message => { if (message.type() === "error" || message.type() === "warning") problems.push(`${message.type()}: ${message.text()}`); });
  page.on("pageerror", error => problems.push(`pageerror: ${error.message}`));
  page.on("requestfailed", request => problems.push(`requestfailed: ${request.url()}`));
  await page.goto(url, { waitUntil: "networkidle" });
  const height = await page.evaluate(() => document.documentElement.scrollHeight);
  for (let top = 0; top < height; top += 600) {
    await page.evaluate(y => window.scrollTo(0, y), top);
    await page.waitForTimeout(120);
  }
  await page.waitForTimeout(1500);
  await page.evaluate(() => window.scrollTo(0, 0));
  const report = await page.evaluate(() => {
    const canvases = [...document.querySelectorAll(".details canvas")];
    const blank = canvases.filter(canvas => {
      const data = canvas.getContext("2d").getImageData(0, 0, canvas.width, canvas.height).data;
      for (let i = 0; i < data.length; i += 4) if (data[i] < 250 || data[i + 1] < 250 || data[i + 2] < 250) return false;
      return true;
    }).map(canvas => canvas.getAttribute("aria-label"));
    return {
      overflow: document.documentElement.scrollWidth - document.documentElement.clientWidth,
      details: canvases.length, blank,
      fontsLoaded: [...document.fonts].filter(font => font.status === "loaded").length,
      fontsDeclared: [...document.fonts].length,
    };
  });
  await page.screenshot({ path: `${out}/${name}.png`, fullPage: true });
  console.log(name, JSON.stringify(report), problems.length ? "\n  " + problems.join("\n  ") : "");
  await context.close();
}

await look("desktop-light", { viewport: { width: 1280, height: 900 }, colorScheme: "light" });
await look("phone-dark", { viewport: { width: 390, height: 844 }, deviceScaleFactor: 2, colorScheme: "dark", isMobile: true, hasTouch: true });
await browser.close();

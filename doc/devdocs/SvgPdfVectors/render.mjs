// Render SVG files in Chromium, Firefox and WebKit as the SVG tests' BrowserRenderer renders them, and time each
// rendering.
//
// Usage: node render.mjs <jobs.json> <sentinel font>
//   jobs.json: [{"svg": path, "width": px, "height": px, "out": png path prefix, "browsers": ["chromium", ...]}, ...]
//   Each rendering goes to <out>.<browser>.png. One line of JSON per rendering is printed, with its seconds.
//
// As BrowserRenderer: a page that shows the SVG as an <img> of the given size at its top left corner, on white, at one
// device pixel to each CSS pixel; Chromium started with --font-render-hinting=none, --disable-lcd-text and
// --force-color-profile=srgb and shot as soon as the page has loaded; Firefox and WebKit shot after two frames; and on
// Linux a fontconfig file that offers the sentinel font alone, with hinting off. PLAYWRIGHT_MODULE names the module of
// the tests' Playwright package (its driver's playwright-core), whose browsers PLAYWRIGHT_BROWSERS_PATH holds.
import fs from "node:fs";
import os from "node:os";
import path from "node:path";
import { pathToFileURL } from "node:url";

const [jobsFile, sentinelFont] = process.argv.slice(2);
if (!jobsFile || !sentinelFont) {
    console.error("usage: node render.mjs <jobs.json> <sentinel font>");
    process.exit(2);
}
const jobs = JSON.parse(fs.readFileSync(jobsFile, "utf8"));
const playwright = await import(process.env.PLAYWRIGHT_MODULE || "playwright-core");

// Write the fontconfig file BrowserRenderer writes: the sentinel font alone, hinting off.
const folder = fs.mkdtempSync(path.join(os.tmpdir(), "pdf-vectors-render-"));
fs.mkdirSync(path.join(folder, "fonts"));
fs.copyFileSync(sentinelFont, path.join(folder, "fonts", "Sentinel.ttf"));
const fontConfig = path.join(folder, "fonts.conf");
fs.writeFileSync(fontConfig, `<?xml version="1.0"?>
<!DOCTYPE fontconfig SYSTEM "fonts.dtd">
<fontconfig>
  <dir>${path.join(folder, "fonts")}</dir>
  <cachedir>${path.join(folder, "cache")}</cachedir>
  <match target="pattern">
    <edit name="hinting" mode="assign"><bool>false</bool></edit>
    <edit name="hintstyle" mode="assign"><const>hintnone</const></edit>
    <edit name="autohint" mode="assign"><bool>false</bool></edit>
    <edit name="antialias" mode="assign"><bool>true</bool></edit>
    <edit name="rgba" mode="assign"><const>none</const></edit>
    <edit name="lcdfilter" mode="assign"><const>lcdnone</const></edit>
  </match>
</fontconfig>
`);
const env = { ...process.env, FONTCONFIG_FILE: fontConfig };

const launched = {};
// Start a browser of a kind once, as BrowserRenderer starts it.
async function browserOf(kind) {
    if (!launched[kind]) {
        const options = { headless: true, env };
        if (kind === "chromium")
            options.args = ["--font-render-hinting=none", "--disable-lcd-text", "--force-color-profile=srgb"];
        launched[kind] = await playwright[kind].launch(options);
    }
    return launched[kind];
}

// Render one SVG in one browser and return the seconds the page took to load and be shot.
async function render(kind, svg, width, height, out) {
    const browser = await browserOf(kind);
    const page = await browser.newPage({ viewport: { width, height }, deviceScaleFactor: 1 });
    const pagePath = path.join(folder, path.basename(svg) + "." + kind + ".html");
    fs.writeFileSync(pagePath, "<!DOCTYPE html><html><head><style>html,body{margin:0;background:#fff}img{display:block}</style></head><body>" +
        `<img src="${pathToFileURL(path.resolve(svg)).href}" width="${width}" height="${height}">` + "</body></html>");
    const start = process.hrtime.bigint();
    try {
        await page.goto(pathToFileURL(pagePath).href, { timeout: 600000 });
        if (kind !== "chromium") {
            for (let frame = 0; frame < 2; ++frame)
                await page.evaluate(() => new Promise(resolve => requestAnimationFrame(() => resolve())));
        }
        fs.writeFileSync(out, await page.screenshot({ type: "png", timeout: 600000 }));
    } finally {
        await page.close();
        fs.rmSync(pagePath, { force: true });
    }
    return Number(process.hrtime.bigint() - start) / 1e9;
}

try {
    for (const job of jobs) {
        for (const kind of job.browsers || ["chromium", "firefox", "webkit"]) {
            const out = `${job.out}.${kind}.png`;
            let seconds = null, error = null;
            try {
                seconds = await render(kind, job.svg, job.width, job.height, out);
            } catch (e) {
                error = String(e.message || e).split("\n")[0];
            }
            console.log(JSON.stringify({ svg: job.svg, browser: kind, out, seconds, error }));
        }
    }
} finally {
    for (const browser of Object.values(launched))
        await browser.close();
    fs.rmSync(folder, { recursive: true, force: true });
}

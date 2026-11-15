// Render an SVG file in Chromium, Firefox and WebKit as the tests' BrowserRenderer renders it, whole or a part of it,
// at its own size or magnified.
//
// Usage: node render.mjs <svg> <width>x<height> <out folder> [--scale <n>] [--crop <x,y,w,h>] [--browsers <list>]
//                        [--sentinel <font>]
//
// Writes <out folder>/<browser>.png. The document is shown as an image of <width>x<height> pixels, as the rendering
// comparison shows it; --scale draws it that many times larger, and --crop takes the given rectangle of the image,
// in pixels of the unmagnified image. A crop at a scale of ten resolves what one pixel of the rendering holds.
//
// PLAYWRIGHT_MODULE names the Playwright package's index.js (as in the tests' build output, or in the NuGet cache of
// Microsoft.Playwright 1.63.0), and PLAYWRIGHT_BROWSERS_PATH the folder its browsers are installed in. The browsers
// are started as the tests start them: Chromium's headless shell with --font-render-hinting=none, --disable-lcd-text
// and --force-color-profile=srgb, each page at one device pixel to a CSS pixel, Firefox and WebKit drawing two frames
// before the screenshot, and, with --sentinel, a fontconfig file that offers the sentinel font alone without hinting.
import fs from "node:fs";
import os from "node:os";
import path from "node:path";
import { pathToFileURL } from "node:url";

const argv = process.argv.slice(2);
if (argv.length < 3) {
    console.error("usage: node render.mjs <svg> <width>x<height> <out folder> [--scale <n>] [--crop <x,y,w,h>]" +
                  " [--browsers <list>] [--sentinel <font>]");
    process.exit(2);
}
const svgPath = path.resolve(argv[0]);
const [width, height] = argv[1].split("x").map(Number);
const outFolder = path.resolve(argv[2]);
const option = (name, fallback) => {
    const at = argv.indexOf(name);
    return at < 0 ? fallback : argv[at + 1];
};
const scale = Number(option("--scale", 1));
const crop = option("--crop", null) ? option("--crop").split(",").map(Number) : [0, 0, width, height];
const browsers = option("--browsers", "chromium,firefox,webkit").split(",");
const sentinel = option("--sentinel", null);
const playwright = (await import(process.env.PLAYWRIGHT_MODULE || "playwright")).default;

const work = fs.mkdtempSync(path.join(os.tmpdir(), "svg-pattern-sampling-"));
let environment = process.env;
if (sentinel) {
    const fonts = path.join(work, "fonts");
    fs.mkdirSync(fonts);
    fs.copyFileSync(sentinel, path.join(fonts, path.basename(sentinel)));
    const config = path.join(work, "fonts.conf");
    fs.writeFileSync(config,
        `<?xml version="1.0"?>\n<!DOCTYPE fontconfig SYSTEM "fonts.dtd">\n<fontconfig>\n` +
        `  <dir>${fonts}</dir>\n  <cachedir>${path.join(work, "cache")}</cachedir>\n` +
        `  <match target="pattern">\n` +
        `    <edit name="hinting" mode="assign"><bool>false</bool></edit>\n` +
        `    <edit name="hintstyle" mode="assign"><const>hintnone</const></edit>\n` +
        `    <edit name="autohint" mode="assign"><bool>false</bool></edit>\n` +
        `    <edit name="antialias" mode="assign"><bool>true</bool></edit>\n` +
        `    <edit name="rgba" mode="assign"><const>none</const></edit>\n` +
        `    <edit name="lcdfilter" mode="assign"><const>lcdnone</const></edit>\n` +
        `  </match>\n</fontconfig>\n`);
    environment = { ...process.env, FONTCONFIG_FILE: config };
}

// The page shows the image at the scale asked, inside a frame of the crop, as a browser shows a standalone document.
const [cropX, cropY, cropWidth, cropHeight] = crop;
const pagePath = path.join(work, "page.html");
fs.writeFileSync(pagePath,
    `<!DOCTYPE html><html><head><style>html,body{margin:0;background:#fff}` +
    `#frame{position:relative;width:${cropWidth * scale}px;height:${cropHeight * scale}px;overflow:hidden}` +
    `img{position:absolute;left:${-cropX * scale}px;top:${-cropY * scale}px;display:block}</style></head>` +
    `<body><div id="frame"><img src="${pathToFileURL(svgPath).href}" width="${width * scale}" ` +
    `height="${height * scale}"></div></body></html>`);

fs.mkdirSync(outFolder, { recursive: true });
for (const name of browsers) {
    const options = { headless: true, env: environment };
    if (name === "chromium")
        options.args = ["--font-render-hinting=none", "--disable-lcd-text", "--force-color-profile=srgb"];
    const browser = await playwright[name].launch(options);
    const page = await browser.newPage({ viewport: { width: cropWidth * scale, height: cropHeight * scale },
                                         deviceScaleFactor: 1 });
    await page.goto(pathToFileURL(pagePath).href);
    for (let frame = 0; frame < (name === "chromium" ? 0 : 2); ++frame)
        await page.evaluate(() => new Promise(resolve => requestAnimationFrame(() => resolve())));
    await page.screenshot({ path: path.join(outFolder, name + ".png"), type: "png" });
    await browser.close();
}
fs.rmSync(work, { recursive: true, force: true });

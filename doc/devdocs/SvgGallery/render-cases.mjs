// Render cases' SVG documents in Chromium as the SVG loop's tests do, for the gallery's steps.
//
// Usage: node render-cases.mjs <results copy> <reference folder> <case,case,...>
//
// The loop keeps Chromium's image of a case only when it falls outside the tolerance, so a step whose cases pass
// renders them here: each <results copy>/svg/<name>.svg into <results copy>/renderings/<name>.png, at the size of
// the case's reference image, taken from <results copy>/references/ or else from <reference folder>, the recorded
// svg_reference. The page, viewport and screenshot are ChromiumRenderer's, and the browser is the headless shell of
// the tests' Playwright package, named by CHROMIUM_EXECUTABLE; PLAYWRIGHT_MODULE names the Playwright module to
// drive it with.
import fs from "node:fs";
import path from "node:path";
import { pathToFileURL } from "node:url";

const playwrightModule = process.env.PLAYWRIGHT_MODULE || "playwright";
const chromiumExecutable = process.env.CHROMIUM_EXECUTABLE;
const [results, referenceFolder, caseList] = process.argv.slice(2);
if (!results || !referenceFolder || !caseList || !chromiumExecutable) {
    console.error("Usage: CHROMIUM_EXECUTABLE=<headless shell> node render-cases.mjs <results copy> <reference folder> <case,...>");
    process.exit(2);
}

// Read the width and height of a PNG from its header.
function readPngSize(pngPath) {
    const header = Buffer.alloc(24);
    const file = fs.openSync(pngPath, "r");
    try {
        fs.readSync(file, header, 0, 24, 0);
    } finally {
        fs.closeSync(file);
    }
    return { width: header.readUInt32BE(16), height: header.readUInt32BE(20) };
}

// Find a case's reference image: the run's own, else the recorded one.
function findReference(name) {
    for (const folder of [path.join(results, "references"), referenceFolder]) {
        const candidate = path.join(folder, name + ".png");
        if (fs.existsSync(candidate))
            return candidate;
    }
    throw new Error(`No reference image of ${name} in the results or in ${referenceFolder}.`);
}

// Write the page that shows an SVG file as an image of a size at its top left corner, as ChromiumRenderer writes it.
function writePageShowing(pagesFolder, svgPath, size) {
    const pagePath = path.join(pagesFolder, path.basename(svgPath, ".svg") + ".html");
    fs.writeFileSync(pagePath,
        "<!DOCTYPE html><html><head><style>html,body{margin:0;background:#fff}img{display:block}</style></head><body>" +
        `<img src="${pathToFileURL(svgPath).href}" width="${size.width}" height="${size.height}">` +
        "</body></html>");
    return pagePath;
}

const { chromium } = await import(playwrightModule);
// The flags ChromiumRenderer starts the browser with: text unhinted, in grayscale and in sRGB, whichever headless shell
// this is. For a case with text, FONTCONFIG_FILE has to name a fontconfig file offering the sentinel font alone, as
// ChromiumRenderer writes one.
const browser = await chromium.launch({
    headless: true, executablePath: chromiumExecutable,
    args: ["--font-render-hinting=none", "--disable-lcd-text", "--force-color-profile=srgb"],
});
const pagesFolder = fs.mkdtempSync(path.join(fs.realpathSync(process.env.TMPDIR || "/tmp"), "svg-gallery-pages-"));
fs.mkdirSync(path.join(results, "renderings"), { recursive: true });
try {
    for (const name of caseList.split(",")) {
        const size = readPngSize(findReference(name));
        const svgPath = path.resolve(results, "svg", name + ".svg");
        const page = await browser.newPage({ viewport: size, deviceScaleFactor: 1 });
        try {
            await page.goto(pathToFileURL(writePageShowing(pagesFolder, svgPath, size)).href);
            fs.writeFileSync(path.join(results, "renderings", name + ".png"), await page.screenshot({ type: "png" }));
        } finally {
            await page.close();
        }
        console.log(`${name}: ${size.width}x${size.height}`);
    }
} finally {
    await browser.close();
    fs.rmSync(pagesFolder, { recursive: true, force: true });
}

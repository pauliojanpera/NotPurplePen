// Whether Chromium, Firefox and WebKit form the union of a clip path's children exactly, or blend the children's
// smoothed edges as they blend separate shapes, which leaves a faint seam where two children only touch.
//
// Usage: node browser-union.mjs <output folder>
//
// Writes test documents into <output folder>/svg and each browser's drawing of them into
// <output folder>/<browser>/<document>.png; compare-union.py then measures them. Each document fills, or hatches,
// areas laid out as a map lays them out, in one of these ways:
//   merged    the areas' union written as one path, which the clip of the pattern is: the exact union, as reference;
//   union     a clip path whose children are the areas' outlines, the pattern drawn once under it;
//   ghosts    the areas' outlines drawn as invisible elements, fill:none, which a clip path's <use> children refer to,
//             the pattern drawn once under it: the design's ghosts;
//   each      each area filled or hatched under a clip of its own, as the export draws them today.
// The layouts: two triangles that touch along a diagonal, two rectangles that overlap, strips of 42 and 43 slices that
// touch along slanted edges, on either side of the 42 children up to which Chromium unites a clip's children with
// path operations rather than painting them into a mask, and 72 cells of a grid, each split into two triangles, 144
// children in all.
//
// PLAYWRIGHT_MODULE names the Playwright package's index.js (as in the tests' build output, or in the NuGet cache of
// Microsoft.Playwright 1.63.0), and PLAYWRIGHT_BROWSERS_PATH the folder its browsers are installed in. The browsers
// are started as the tests start them: Chromium's headless shell with --font-render-hinting=none, --disable-lcd-text
// and --force-color-profile=srgb, each page showing the SVG as an <img> of its size at one device pixel to a CSS pixel,
// and Firefox and WebKit drawing two frames before the screenshot.
import fs from "node:fs";
import path from "node:path";
import { pathToFileURL } from "node:url";

const output = process.argv[2];
if (!output) {
    console.error("Usage: node browser-union.mjs <output folder>");
    process.exit(2);
}
const playwright = (await import(process.env.PLAYWRIGHT_MODULE || "playwright")).default;

const SIZE = 240;

// The areas of a layout, each a list of [x, y] corners, in the document's units, which are pixels scaled by 1.13 and
// moved by a fraction, so that edges fall across pixels.
function layouts() {
    const grid = [];
    for (let row = 0; row < 6; ++row) {
        for (let column = 0; column < 12; ++column) {
            const x = 10 + column * 15, y = 10 + row * 30;
            grid.push([[x, y], [x + 15, y], [x, y + 30]]);
            grid.push([[x + 15, y], [x + 15, y + 30], [x, y + 30]]);
        }
    }
    // A strip of slices whose shared edges slant, as many as Chromium unites with path operations, and one more.
    function strip(count) {
        const slices = [];
        const width = 180 / count;
        for (let i = 0; i < count; ++i) {
            const x0 = 10 + i * width, x1 = x0 + width;
            slices.push([[x0, 10], [x1, 10], [x1 + 3, 200], [x0 + 3, 200]]);
        }
        return { areas: slices, merged: [[[10, 10], [190, 10], [193, 200], [13, 200]]] };
    }
    return {
        strip42: strip(42),
        strip43: strip(43),
        touching: { areas: [[[10, 200], [200, 10], [10, 10]], [[10, 200], [200, 200], [200, 10]]], merged: [[[10, 10], [200, 10], [200, 200], [10, 200]]] },
        overlapping: { areas: [[[10, 10], [130, 10], [130, 200], [10, 200]], [[70, 30], [200, 30], [200, 180], [70, 180]]],
                       merged: [[[10, 10], [130, 10], [130, 30], [200, 30], [200, 180], [130, 180], [130, 200], [10, 200]]] },
        grid: { areas: grid, merged: [[[10, 10], [190, 10], [190, 190], [10, 190]]] },
    };
}

// Path data of polygons.
function pathData(polygons) {
    return polygons.map(polygon => "M" + polygon.map(([x, y]) => x + " " + y).join("L") + "Z").join("");
}

// The document of a layout drawn one way with one paint: "fill", a solid fill, or "hatch", a pattern of lines.
function documentOf(layout, way, paint) {
    const style = paint === "fill" ? "fill:#00b9f2" : "fill:url(#p)";
    const defs = ['<pattern id="p" patternUnits="userSpaceOnUse" width="200" height="4.4" viewBox="0 0 200 4.4">' +
                  '<path d="M0 2.2H200" style="stroke:#00b9f2;stroke-width:1.3"/></pattern>'];
    const body = [];
    const box = `<rect x="0" y="0" width="220" height="220" style="${style}"/>`;
    if (way === "merged") {
        defs.push(`<clipPath id="c"><path d="${pathData(layout.merged)}"/></clipPath>`);
        body.push(`<g clip-path="url(#c)">${box}</g>`);
    }
    else if (way === "union") {
        defs.push('<clipPath id="c">' + layout.areas.map(area => `<path d="${pathData([area])}"/>`).join("") + "</clipPath>");
        body.push(`<g clip-path="url(#c)">${box}</g>`);
    }
    else if (way === "ghosts") {
        defs.push('<clipPath id="c">' + layout.areas.map((area, i) => `<use xlink:href="#g${i + 1}"/>`).join("") + "</clipPath>");
        body.push('<g style="fill:none">' + layout.areas.map((area, i) => `<path id="g${i + 1}" data-o="${i + 1}" d="${pathData([area])}"/>`).join("") + "</g>");
        body.push(`<g clip-path="url(#c)">${box}</g>`);
    }
    else {
        layout.areas.forEach((area, i) => {
            defs.push(`<clipPath id="c${i + 1}"><path d="${pathData([area])}"/></clipPath>`);
            body.push(`<g clip-path="url(#c${i + 1})">${box}</g>`);
        });
    }
    return '<svg xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" ' +
           `width="${SIZE}" height="${SIZE}" viewBox="0 0 ${SIZE} ${SIZE}">\n<defs>\n${defs.join("\n")}\n</defs>\n` +
           `<g transform="translate(3.37 2.61) scale(1.13)">\n${body.join("\n")}\n</g>\n</svg>\n`;
}

const svgFolder = path.join(output, "svg");
fs.mkdirSync(svgFolder, { recursive: true });
const names = [];
for (const [layoutName, layout] of Object.entries(layouts())) {
    for (const paint of ["fill", "hatch"]) {
        for (const way of ["merged", "union", "ghosts", "each"]) {
            const name = `${layoutName}-${paint}-${way}`;
            fs.writeFileSync(path.join(svgFolder, name + ".svg"), documentOf(layout, way, paint));
            names.push(name);
        }
    }
}

const pages = fs.mkdtempSync(path.join(fs.realpathSync(process.env.TMPDIR || "/tmp"), "browser-union-"));
try {
    for (const browserName of ["chromium", "firefox", "webkit"]) {
        const options = { headless: true };
        if (browserName === "chromium")
            options.args = ["--font-render-hinting=none", "--disable-lcd-text", "--force-color-profile=srgb"];
        const browser = await playwright[browserName].launch(options);
        const folder = path.join(output, browserName);
        fs.mkdirSync(folder, { recursive: true });
        try {
            for (const name of names) {
                const pagePath = path.join(pages, name + ".html");
                fs.writeFileSync(pagePath, "<!DOCTYPE html><html><head><style>html,body{margin:0;background:#fff}img{display:block}</style></head><body>" +
                                 `<img src="${pathToFileURL(path.resolve(svgFolder, name + ".svg")).href}" width="${SIZE}" height="${SIZE}"></body></html>`);
                const page = await browser.newPage({ viewport: { width: SIZE, height: SIZE }, deviceScaleFactor: 1 });
                try {
                    await page.goto(pathToFileURL(pagePath).href);
                    if (browserName !== "chromium") {
                        for (let frame = 0; frame < 2; ++frame)
                            await page.evaluate(() => new Promise(resolve => requestAnimationFrame(() => resolve())));
                    }
                    fs.writeFileSync(path.join(folder, name + ".png"), await page.screenshot({ type: "png" }));
                }
                finally {
                    await page.close();
                }
            }
            console.log(`${browserName} ${browser.version()}: ${names.length} documents`);
        }
        finally {
            await browser.close();
        }
    }
}
finally {
    fs.rmSync(pages, { recursive: true, force: true });
}

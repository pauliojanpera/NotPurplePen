// What pdf.js, the reader a TypeScript port in the browser would have, keeps of a PDF map's first page: the operators
// of its operator list by name, the form of the colors they set, the blend modes, soft masks and overprinting of its
// graphics states, and its fonts as pdf.js hands them to the browser.
//
// Usage: node pdfjs-ops.mjs <pdf>...   with PDFJS_MODULE naming pdfjs-dist's build/pdf.mjs (npm install pdfjs-dist).
// Prints one line of JSON a PDF.
import fs from "node:fs";
import path from "node:path";
import { pathToFileURL } from "node:url";

const pdfjs = await import(process.env.PDFJS_MODULE || "pdfjs-dist/build/pdf.mjs");
const names = Object.fromEntries(Object.entries(pdfjs.OPS).map(([name, id]) => [id, name]));
const dist = path.dirname(path.dirname(new URL(import.meta.resolve(process.env.PDFJS_MODULE || "pdfjs-dist/build/pdf.mjs")).pathname));

for (const file of process.argv.slice(2)) {
    const data = new Uint8Array(fs.readFileSync(file));
    const task = pdfjs.getDocument({
        data, verbosity: 0, isEvalSupported: false, disableFontFace: true, fontExtraProperties: true, useSystemFonts: false,
        standardFontDataUrl: pathToFileURL(path.join(dist, "standard_fonts") + "/").href,
        cMapUrl: pathToFileURL(path.join(dist, "cmaps") + "/").href, cMapPacked: true,
        iccUrl: pathToFileURL(path.join(dist, "iccs") + "/").href,
        wasmUrl: pathToFileURL(path.join(dist, "wasm") + "/").href,
    });
    const document = await task.promise;
    const page = await document.getPage(1);
    const start = performance.now();
    const list = await page.getOperatorList();
    const seconds = (performance.now() - start) / 1000;
    const counts = {}, colorForms = {}, gstate = {}, fonts = new Set();
    for (let i = 0; i < list.fnArray.length; ++i) {
        const name = names[list.fnArray[i]] || String(list.fnArray[i]);
        counts[name] = (counts[name] || 0) + 1;
        const args = list.argsArray[i];
        if (/Color/.test(name)) {
            const form = args === null ? "none" : args.map(a => typeof a === "string" ? (/^#[0-9a-f]{6}$/i.test(a) ? "hex-rgb" : "string") :
                ArrayBuffer.isView(a) ? a.constructor.name + "[" + a.length + "]" : typeof a).join(",");
            const key = name + ":" + form;
            colorForms[key] = (colorForms[key] || 0) + 1;
        }
        if (name === "setGState") {
            for (const [key, value] of args[0]) {
                const shown = key === "SMask" ? (value ? "mask" : "none") : JSON.stringify(value);
                gstate[key + "=" + shown] = (gstate[key + "=" + shown] || 0) + 1;
            }
        }
        if (name === "setFont")
            fonts.add(args[0]);
    }
    const fontReport = [];
    for (const id of fonts) {
        try {
            const font = page.commonObjs.get(id);
            fontReport.push({ name: font.name, loadedName: font.loadedName, type: font.type, mimetype: font.mimetype,
                              bytes: font.data ? font.data.length : 0, missingFile: font.missingFile });
        } catch (e) {
            fontReport.push({ id, error: String(e.message || e) });
        }
    }
    console.log(JSON.stringify({ pdf: file, pdfjs: pdfjs.version, seconds: Math.round(seconds * 1000) / 1000,
                                 operators: list.fnArray.length, counts, colorForms, gstate, fonts: fontReport }));
    await task.destroy();
}

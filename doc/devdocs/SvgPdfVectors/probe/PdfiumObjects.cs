using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using PDFiumCore;

namespace PdfVectorsProbe
{
    // What PDFium's page-object API, which PDFiumCore binds and the application already ships, gives of a page: the
    // objects by type, through form XObjects; the colors, which it gives as RGBA only; whether a text object's font
    // data can be read; and the image filters. It also lists the API's functions whose names speak of color, blending,
    // overprinting or marked content, so that what it lacks shows.
    internal static class PdfiumObjects
    {
        private const int FPDF_PAGEOBJ_TEXT = 1, FPDF_PAGEOBJ_PATH = 2, FPDF_PAGEOBJ_IMAGE = 3, FPDF_PAGEOBJ_SHADING = 4, FPDF_PAGEOBJ_FORM = 5;

        // Report on the first page of a PDF.
        // pdf: the PDF file.
        public static int Report(string pdf)
        {
            fpdfview.FPDF_InitLibrary();
            FpdfDocumentT document = fpdfview.FPDF_LoadDocument(pdf, null);
            if (document == null) {
                Console.Error.WriteLine("PDFium cannot load " + pdf);
                return 1;
            }
            FpdfPageT page = fpdfview.FPDF_LoadPage(document, 0);
            SortedDictionary<string, int> counts = new SortedDictionary<string, int>(StringComparer.Ordinal);
            HashSet<string> rgbaColors = new HashSet<string>(StringComparer.Ordinal);
            int count = fpdf_edit.FPDFPageCountObjects(page);
            for (int i = 0; i < count; ++i)
                Walk(fpdf_edit.FPDFPageGetObject(page, i), counts, rgbaColors);
            fpdfview.FPDF_ClosePage(page);
            fpdfview.FPDF_CloseDocument(document);

            string[] interesting = { "Color", "Blend", "Overprint", "Mark", "FontData", "ImageFilter", "ClipPath", "Dash", "Transparency" };
            List<string> functions = typeof(fpdf_edit).Assembly.GetTypes()
                .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                .Select(method => method.Name).Where(name => interesting.Any(word => name.Contains(word, StringComparison.Ordinal)))
                .Distinct().OrderBy(name => name, StringComparer.Ordinal).ToList();

            Console.WriteLine("{\"command\":\"pdfium-objects\",\"pdf\":" + Program.Json(pdf) + ",\"counts\":{" +
                              string.Join(",", counts.Select(pair => Program.Json(pair.Key) + ":" + pair.Value.ToString(CultureInfo.InvariantCulture))) +
                              "},\"distinct_rgba_colors\":" + rgbaColors.Count + ",\"functions\":[" + string.Join(",", functions.Select(Program.Json)) + "]}");
            return 0;
        }

        // Count an object, and the objects of a form inside it.
        private static void Walk(FpdfPageobjectT obj, SortedDictionary<string, int> counts, HashSet<string> rgbaColors)
        {
            int type = fpdf_edit.FPDFPageObjGetType(obj);
            string name = type == FPDF_PAGEOBJ_TEXT ? "text" : type == FPDF_PAGEOBJ_PATH ? "path" : type == FPDF_PAGEOBJ_IMAGE ? "image" :
                          type == FPDF_PAGEOBJ_SHADING ? "shading" : type == FPDF_PAGEOBJ_FORM ? "form" : "unknown";
            Add(counts, name);
            uint r = 0, g = 0, b = 0, a = 0;
            if (fpdf_edit.FPDFPageObjGetFillColor(obj, ref r, ref g, ref b, ref a) != 0)
                rgbaColors.Add(string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3}", r, g, b, a));
            else if (type == FPDF_PAGEOBJ_PATH)
                Add(counts, "path:fill-color-unreadable");
            if (fpdf_edit.FPDFPageObjHasTransparency(obj) != 0)
                Add(counts, name + ":has-transparency");
            if (type == FPDF_PAGEOBJ_TEXT) {
                FpdfFontT font = fpdf_edit.FPDFTextObjGetFont(obj);
                ulong length = 0;
                if (font != null && FontDataLength(font, ref length) && length > 0)
                    Add(counts, "text:font-data-readable");
            }
            if (type == FPDF_PAGEOBJ_IMAGE) {
                int filters = fpdf_edit.FPDFImageObjGetImageFilterCount(obj);
                Add(counts, "image:filters=" + filters.ToString(CultureInfo.InvariantCulture));
            }
            if (type == FPDF_PAGEOBJ_FORM) {
                int inner = fpdf_edit.FPDFFormObjCountObjects(obj);
                for (int i = 0; i < inner; ++i)
                    Walk(fpdf_edit.FPDFFormObjGetObject(obj, (ulong)i), counts, rgbaColors);
            }
        }

        // Whether a font's data can be read, with its length.
        private static unsafe bool FontDataLength(FpdfFontT font, ref ulong length)
        {
            return fpdf_edit.FPDFFontGetFontData(font, (byte*)0, 0, ref length) != 0;
        }

        // Count one more of a kind.
        private static void Add(SortedDictionary<string, int> counts, string key)
        {
            int count;
            counts.TryGetValue(key, out count);
            counts[key] = count + 1;
        }
    }
}

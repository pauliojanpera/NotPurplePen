using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Diagnostics;

using PurplePen.MapModel;
using PurplePen.Graphics2D;
using TestingUtils;
using System.IO;
using System.Threading;
using PdfSharp.Pdf;
using PdfSharp.Drawing;

namespace Map_PDF.Tests
{
    static class PdfCreation
    {
        public static void CreatePdfAndPng(string pdfFileName, string pngFileName, int pixelWidth, int pixelHeight, bool useCmyk, Action<IGraphicsTarget> draw)
        {
            DeletePdfAndPng(pdfFileName, pngFileName);

            // Create PDF at 100 pixel per inch.
            IPdfWriter pdfWriter = new PdfWriter();
            IPdfDocumentWriter pdfDocumentWriter = pdfWriter.CreateDocument(pdfFileName, Path.GetFileNameWithoutExtension(pdfFileName), useCmyk);
            IGraphicsTarget graphicsTarget = pdfDocumentWriter.BeginPage(new SizeF(pixelWidth / 100F, pixelHeight / 100F));
            draw(graphicsTarget);
            pdfDocumentWriter.EndPage(graphicsTarget);
            pdfDocumentWriter.Save();

            // Start PDF viewer
            //Process.Start(pdfFileName);

            // Copy to PNG
            ConvertPdfToPng(pdfFileName, pngFileName);

        }

        public static void CreatePdfAndPngUsingCopiedPage(string pdfFileName, string pngFileName, string pdfImport, int pageImport, Action<IGraphicsTarget> draw)
        {
            DeletePdfAndPng(pdfFileName, pngFileName);

            // Create PDF at 100 pixel per inch.
            IPdfWriter pdfWriter = new PdfWriter();
            IPdfDocumentWriter pdfDocumentWriter = pdfWriter.CreateDocument(pdfFileName, Path.GetFileNameWithoutExtension(pdfFileName), false);
            IGraphicsTarget graphicsTarget = pdfDocumentWriter.BeginCopiedPage(pdfImport, pageImport);
            draw(graphicsTarget);
            pdfDocumentWriter.EndPage(graphicsTarget);
            pdfDocumentWriter.Save();

            // Start PDF viewer
            //Process.Start(pdfFileName);

            // Copy to PNG
            ConvertPdfToPng(pdfFileName, pngFileName);

        }

        public static void CreatePdfAndPngUsingCopiedPartialPage(string pdfFileName, string pngFileName, string pdfImport, int pageImport, 
                                                                 SizeF sizeInInches, RectangleF partialPageInInches, RectangleF destRectangleInInches, Action<IGraphicsTarget> draw)
        {
            DeletePdfAndPng(pdfFileName, pngFileName);

            // Create PDF at 100 pixel per inch.
            IPdfWriter pdfWriter = new PdfWriter();
            IPdfDocumentWriter pdfDocumentWriter = pdfWriter.CreateDocument(pdfFileName, Path.GetFileNameWithoutExtension(pdfFileName), false);
            IGraphicsTarget graphicsTarget = pdfDocumentWriter.BeginCopiedPartialPage(pdfImport, pageImport, sizeInInches, partialPageInInches, destRectangleInInches);
            draw(graphicsTarget);
            pdfDocumentWriter.EndPage(graphicsTarget);
            pdfDocumentWriter.Save();

            // Start PDF viewer
            //Process.Start(pdfFileName);

            // Copy to PNG
            ConvertPdfToPng(pdfFileName, pngFileName);

        }

        // How long to keep trying to delete a file another process still has open.
        // Ghostscript has written the PNG and exited, but on a loaded machine the
        // handle can outlive the process by seconds.
        private static readonly TimeSpan DeleteTimeout = TimeSpan.FromSeconds(30);

        // Delete the PDF and PNG files, retrying if either file is temporarily unavailable.
        private static void DeletePdfAndPng(string pdfFileName, string pngFileName)
        {
            Random random = new Random();
            Stopwatch elapsed = Stopwatch.StartNew();
            bool collected = false;

            while (true) {
                try {
                    File.Delete(pdfFileName);
                    File.Delete(pngFileName);
                    return;
                }
                catch (IOException) {
                    if (elapsed.Elapsed >= DeleteTimeout)
                        throw;

                    // A decoder that reached the file through a finalizable handle holds
                    // it until that finalizer runs, which nothing else here makes happen.
                    if (!collected) {
                        collected = true;
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                    }

                    Thread.Sleep(random.Next(30, 301));
                }
            }
        }

        public static void ConvertPdfToPng(string pdfFileName, string pngFileName)
        {
            string arguments = String.Format(
                "-dSTRICT -dSAFER -dBATCH -dNOPAUSE -r100  -dTextAlphaBits=4 -dGraphicsAlphaBits=4 -sDEVICE=png16m -sOutputFile=\"{1}\" \"{0}\"",
                pdfFileName, pngFileName);

            ProcessStartInfo startInfo = new ProcessStartInfo(TestUtil.GetToolFullPath("gswin32c.exe"), arguments);
            startInfo.CreateNoWindow = true;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            Process process = Process.Start(startInfo);
            process.WaitForExit();
        }
    }
}

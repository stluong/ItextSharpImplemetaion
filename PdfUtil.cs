using System.IO;
using System.Collections.Generic;
using System.Linq;
using Telerik.Windows.Documents.Fixed.FormatProviders.Pdf;
using Telerik.Windows.Documents.Fixed.Model;
using System.Drawing;
using System.Runtime.CompilerServices;
using AAON.Domain.Extension;
using Telerik.Windows.Documents.Fixed.FormatProviders.Pdf.Export;
using iTextSharp.text.pdf;
using Telerik.Windows.Documents.Fixed.FormatProviders.Pdf.Streaming;
using Telerik.Windows.Documents.Fixed.Model.Data;
using Size = System.Windows.Size;
using iTextSharp.text;
using iTextSharp.text.pdf.parser;
using Path = System.IO.Path;
using System.Diagnostics;
using System;
using System.Text;
using AAON.FeatureConditioning.FeatureService;

namespace AAON.Platform.Services
{
    public class PdfUtil
    {
        public static void MergePdf(List<string> Sourcefiles, string DestinationPath, string DestinationFileName, bool OpenFinalDocument = true)
        {
            string outFile = Path.Combine(DestinationPath, DestinationFileName);
            RadFixedDocument finaldoc = new RadFixedDocument();
            List<RadFixedDocument> lstfinaldoc = new List<RadFixedDocument>();
            foreach (string str in Sourcefiles.OrderBy(x => x).ToList())
            {
                if (!string.IsNullOrEmpty(str))
                {
                    byte[] bytes = System.IO.File.ReadAllBytes(str);
                    PdfFormatProvider provider = new PdfFormatProvider();
                    RadFixedDocument document = new RadFixedDocument();

                    document = provider.Import(bytes);
                    int documentPages = document.Pages.Count;
                    PdfReader reader = null;

                    for (int i = 0; i < documentPages; i++)
                    {
                        var currentPage = document.Pages[i];
                        var documentContentLines = currentPage.Content.Count;
                        var elementType = currentPage.Content[0].ToString();
                        var isDrawingPage = documentContentLines == 1 && elementType == "Telerik.Windows.Documents.Fixed.Model.Objects.Image";

                        if (!isDrawingPage && documentContentLines <= 1)
                        {
                            int currentPDFPage = i + 1;                              
                            int selectStartpage = currentPDFPage + 1;
                            int selectEndpage = documentPages;
                            if (selectStartpage > selectEndpage)
                            {
                                File.Delete(str);
                                Sourcefiles.Remove(str);
                            }
                            else
                            {
                                string range = string.Format("{0}-{1}", selectStartpage, selectEndpage);
                                string newDrawingPDFName = str;
                                int pos = newDrawingPDFName.IndexOf(".pdf");
                                newDrawingPDFName = newDrawingPDFName.Insert(pos, "_mod");

                                using (PdfReader PDFreader = new PdfReader(str))
                                {
                                    PDFreader.SelectPages(range);
                                    using (PdfStamper stamper =
                                        new PdfStamper(PDFreader, File.Create(newDrawingPDFName)))
                                    {
                                        stamper.Close();
                                    }
                                }

                                File.Copy(newDrawingPDFName, str, true);
                                File.Delete(newDrawingPDFName);
                            }
                        }
                    }
                }
            }

            if (Sourcefiles.Count > 0)
            {

                using (FileStream stream =
                    new FileStream(DestinationFileName, FileMode.Create))
                {
                    Document document = new Document();
                    PdfCopy pdf = new PdfCopy(document, stream);
                    PdfReader reader = null;

                    document.Open();

                    foreach (string file in Sourcefiles.OrderBy(x => x).ToList())
                    {
                        if (!string.IsNullOrEmpty(file))
                        {
                            reader = new PdfReader(file);
                            pdf.AddDocument(reader);
                            reader.Close();
                        }
                    }

                    document.Close();
                }

            }
        }

    }
}
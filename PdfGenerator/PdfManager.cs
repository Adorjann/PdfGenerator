using Balbarak.WeasyPrint;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using PdfGenerator.Controllers;
using System.Linq;
using System.Net.Mime;
using System.Reflection;

namespace PdfGenerator
{
    public class PdfManager
    {

        public PdfManager()
        {
        }

        public async Task<byte[]> GetPDfAsync(string htmlResourceName, string imageResourceName, List<string> ttfResourceNames)
        {
            using (WeasyPrintClient client = new WeasyPrintClient())
            {
                client.OnDataError += OnDataError;
                client.OnDataOutput += OnDataOutput;

                var htmlContent = GetStringContentFromResource(htmlResourceName);
                

                foreach (var font in ttfResourceNames)
                { 
                    var ttfBase64 = Convert.ToBase64String(GetByteContentFromResource(font));

                    //var fontName = font.Substring(font.LastIndexOf(@"\")).TakeWhile(x => !x.Equals('.')).ToString();
                    var fontName = font.Split('.')[font.Split('.').Length-2];  
                    
                    htmlContent = InsertFontFace(ttfBase64, htmlContent, fontName);
                }

                var base64Image = Convert.ToBase64String(GetByteContentFromResource(imageResourceName));
                htmlContent = InsertImage(base64Image, htmlContent);

                return await client.GeneratePdfAsync(htmlContent).ConfigureAwait(false);
            }
        }

        private byte[] GetByteContentFromResource(string fileName)
        {
            var assembly = Assembly.GetEntryAssembly();
            using (var stream = assembly.GetManifestResourceStream(fileName))
            {
                if (stream == null)
                    throw new ArgumentException($"Resource {fileName} not found in assembly {assembly.GetName()}. Make sure the Build Action is set to Embedded Resource.", nameof(fileName));

                byte[] fileValue = new byte[stream.Length];
                stream.Read(fileValue, 0, fileValue.Length);

                return fileValue;
            }
        }

        private string GetStringContentFromResource(string fileName)
        {
            var assembly = Assembly.GetEntryAssembly();
            using (var stream = assembly.GetManifestResourceStream(fileName))
            {
                if (stream == null)
                    throw new ArgumentException($"Resource {fileName} not found in assembly {assembly.GetName()}. Make sure the Build Action is set to Embedded Resource.", nameof(fileName));

                StreamReader reader = new StreamReader(stream);
                return reader.ReadToEnd();

            }
        }

        private void OnDataOutput(OutputEventArgs e)
        {
             var log = "Data output: " + e.Data;
        }

        private void OnDataError(OutputEventArgs e)
        {
            var log = "Error: " + e.Data;
            throw new Exception("Error: " + e.Data);
        }


        private string InsertFontFace(string base64Font, string htmlContent, string fontFamily)
        {
            var fontFace = "@font-face {\n" +
                            "  font-family: \"" + fontFamily + "\";\n" +
                            "  src: url(data:application/font-sfnt;charset=utf-8;base64," + base64Font + ") format(\"truetype\");\n" +
                            "}\n";

            return htmlContent.Replace("<style>", $"<style>\n\n{fontFace}");
        }

        private string InsertImage(string base64Image, string htmlContent)
        {
            var src = $"data:image/png;base64,{base64Image}";

            return htmlContent.Replace("%%image%%", src);
        }
    }
}
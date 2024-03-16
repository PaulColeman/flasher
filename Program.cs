using System.Diagnostics;

namespace flasher
{
    internal class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var file = args.FirstOrDefault();
            if (file == null)
            {
                Console.WriteLine("flasher <filename.pdf>");
                return -1;
            }

            if (File.Exists(file)) 
            {
                Console.WriteLine($"Can't find {file}");
                return -1;
            }

            var title = Path.GetFileNameWithoutExtension(file);
            var htmlFilename = Path.Combine(Path.GetDirectoryName(file)!, $"{title}.html");

            var images = await GetImages(file);

            var html = HtmlGen.GetHtml(title, images, file);
            await File.WriteAllTextAsync(htmlFilename, html);

            new Process() { StartInfo = new ProcessStartInfo(htmlFilename) { UseShellExecute = true } }.Start();
            return 0;
        }

        public static async Task<IReadOnlyList<string>> GetImages(string pdfFile)
        {
            using var fs = new FileStream(pdfFile, FileMode.Open, FileAccess.Read);

            var images = PDFtoImage.Conversion.ToImagesAsync(fs);

            var base64Images = new List<string>();
            await foreach (var image in images)
            {
                var ms = new MemoryStream();
                image.Encode(ms, SkiaSharp.SKEncodedImageFormat.Jpeg, 50);
                var bytes = ms.ToArray();
                var base64 = Convert.ToBase64String(bytes);
                base64Images.Add(base64);
            }

            return base64Images;
        }
    }
}
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace CodePoster
{
    class Program
    {
        static void Main(string[] args)
        {

            if (args.Length < 4)
            {
                Console.WriteLine(@"Usage: CodePoster.exe c:\code\project *.cs c:\input\image.png c:\output\image.png");
                return;
            }
            var directory = args[0];
            var searchpattern = args[1];
            var imageFile = args[2];
            var fontSize = 18;
            var output = args[3];
            var invert = false;
            var stretch = true;




            var font = new Font(FontFamily.GenericMonospace, fontSize, FontStyle.Bold);
            var fontG = Graphics.FromImage(new Bitmap(100, 100));
            var size = fontG.MeasureString("X", font);


            var content = DirSearch(directory, searchpattern);
            content = Sanitize(content);

            int width;
            int height;

            if (stretch)
            {
                width = (int)(Math.Ceiling(Math.Sqrt(content.Length) * (16.0 / 9.0)));
                height = (int)Math.Ceiling((content.Length / (double)width));

            }
            else
            {
                width = (int)Math.Ceiling(Math.Sqrt(content.Length));
                height = (int)Math.Ceiling(Math.Sqrt(content.Length));
            }

            var pieces = new ImageChar[width, height];

            Bitmap image = (Bitmap)Image.FromFile(imageFile);
            image = ResizeImage(image, width, height);

            int x = 0, y = 0;

            foreach (var c in content)
            {
                pieces[x, y] = new ImageChar(c, new SolidBrush(image.GetPixel(x, y)));
                x++;
                if (x == width)
                {
                    x = 0;
                    y++;
                }
            }
            var square = size.Height * .7f;


            Bitmap bm = new Bitmap((int)Math.Ceiling((width * square)), (int)Math.Ceiling((height * square)));
            Graphics g = Graphics.FromImage(bm);
            g.FillRectangle(invert ? Brushes.Black : Brushes.White, 0, 0, bm.Width, bm.Height);
            for (int x_ = 0; x_ < width; x_++)
            {
                for (int y_ = 0; y_ < height; y_++)
                {
                    var imageChar = pieces[x_, y_];
                    if (imageChar == null) continue;
                    g.DrawString(imageChar.Character, font, imageChar.Brush, x_ * square, y_ * square);
                }
            }
            bm.Save(output, ImageFormat.Png);

            Console.WriteLine("Done");
        }

        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
        private static string Sanitize(string content)
        {
            content = content.Replace("\t", " ");
            content = content.Replace("\r\n", " ");


            var blockComments = @"/\*(.*?)\*/";
            var lineComments = @"//(.*?)\r?\n?";
            var tripleComments = @"///(.*?)\r?\n?";
            content = Regex.Replace(content,
                blockComments + "|" + tripleComments + "|" + lineComments,
                me => "",
                RegexOptions.Singleline);


            while (content.Contains("  "))
            {
                content = content.Replace("  ", " ");
            }


            content = Regex.Replace(content, "\\s?\\*\\s?", me => "*");
            content = Regex.Replace(content, "\\s?\\+\\s?", me => "+");
            content = Regex.Replace(content, "\\s?\\+=\\s?", me => "+=");
            content = Regex.Replace(content, "\\s?\\-=\\s?", me => "-=");
            content = Regex.Replace(content, "\\s?-\\s?", me => "-");
            content = Regex.Replace(content, "\\s?/\\s?", me => "/");
            content = Regex.Replace(content, "\\s?;\\s?", me => ";");
            content = Regex.Replace(content, "\\s?,\\s?", me => ",");
            content = Regex.Replace(content, "\\s?:\\s?", me => ":");
            content = Regex.Replace(content, "\\s?%\\s?", me => "%");
            content = Regex.Replace(content, "\\s?!=\\s?", me => "!=");
            content = Regex.Replace(content, "\\s?!==\\s?", me => "!==");
            content = Regex.Replace(content, "\\s?===\\s?", me => "===");
            content = Regex.Replace(content, "\\s?>=\\s?", me => ">=");
            content = Regex.Replace(content, "\\s?=>\\s?", me => "=>");
            content = Regex.Replace(content, "\\s?=\\s?", me => "=");
            content = Regex.Replace(content, "\\s?<\\s?", me => "<");
            content = Regex.Replace(content, "\\s?>\\s?", me => ">");
            content = Regex.Replace(content, "\\s?&&\\s?", me => "&&");
            content = Regex.Replace(content, "\\s?\\|\\|\\s?", me => "||");
            content = Regex.Replace(content, "\\s?\\|\\s?", me => "|");
            content = Regex.Replace(content, "\\s?&\\s?", me => "&");
            content = Regex.Replace(content, "\\s?\\?\\s?", me => "?");
            content = Regex.Replace(content, "\\s?\\)\\s?", me => ")");
            content = Regex.Replace(content, "\\s?\\(\\s?", me => "(");
            content = Regex.Replace(content, "\\s?\\^\\s?", me => "^");
            content = Regex.Replace(content, "\\s?{\\s?", me => "{");
            content = Regex.Replace(content, "\\s?}\\s?", me => "}");
            content = Regex.Replace(content, "\\s?if\\s?", me => "if");
            content = Regex.Replace(content, "\\s?case\\s?", me => "case");
            content = Regex.Replace(content, "\\s?for\\s?", me => "for");
            content = Regex.Replace(content, "\\s?while\\s?", me => "while");
            content = Regex.Replace(content, "\\s?return", me => "return");
            return content;
        }

        static string DirSearch(string sDir, string searchpattern)
        {
            StringBuilder sb = new StringBuilder();

            foreach (string d in Directory.GetDirectories(sDir))
            {
                if (d.Contains(".git") || d.Contains(".idea") || d.Contains("node_modules")) continue;
                foreach (string f in Directory.GetFiles(d, searchpattern))
                {
                    if (f.Contains(".d.ts")) continue;
                    sb.Append(File.ReadAllText(f));
                }
                sb.Append(DirSearch(d, searchpattern));
            }
            return sb.ToString();
        }
    }

    internal class ImageChar
    {
        public ImageChar(char character, Brush brush)
        {
            Character = character.ToString();
            Brush = brush;
        }

        public string Character { get; set; }
        public Brush Brush { get; set; }

    }
}

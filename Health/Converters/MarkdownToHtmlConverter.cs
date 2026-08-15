using System.Globalization;
using System.Text.RegularExpressions;

namespace Health.Converters
{
    public class MarkdownToHtmlConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value as string;
            if (string.IsNullOrEmpty(text)) return string.Empty;

            string html = Regex.Replace(text, @"\*\*(.*?)\*\*", "<b>$1</b>", RegexOptions.Singleline);

            html = Regex.Replace(html, @"\*(.*?)\*", "<i>$1</i>", RegexOptions.Singleline);

            html = html.Replace("\n", "<br>");

            return html;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
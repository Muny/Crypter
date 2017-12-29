using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Crypter
{
    public class BoolToFontFamilyConverter : BoolToValueConverter<FontFamily> { }
    public class BoolToDoubleConverter : BoolToValueConverter<double> { }
    public class BoolToBrushConverter : BoolToValueConverter<Brush> { }
    public class BoolToStringConverter : BoolToValueConverter<string> { }

    /*public class SelectedItemsToValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            List<CrypterFile> selectedFiles = (List<CrypterFile>)value;

            if (selectedFiles.Count == 0)
            {
                switch ((string)parameter)
                {
                    case "typeText":
                        return "NO FILE SELECTED";
                    case "nameText":
                    case "sizeText":
                    case "encryptionDateText":
                        return "-";

                    case "encryptSelectedButton":
                    case "decryptSelectedButton":
                        return false;

                }
            }
            else if (selectedFiles.Count == 1)
            {
                switch ((string)parameter)
                {
                    case "typeText":
                        return selectedFiles[0].FileType;
                    case "nameText":
                        return selectedFiles[0].OGName;
                    case "sizeText":
                        return selectedFiles[0].HumanReadableSize;
                    case "encryptionDateText":
                        return "-";

                    case "encryptSelectedButton":
                        return !selectedFiles[0].IsEncrypted;
                    case "decryptSelectedButton":
                        return false;

                }
            }
            else if (selectedFiles.Count == 2)
            {

            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }*/
}

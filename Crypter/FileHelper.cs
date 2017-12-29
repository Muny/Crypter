using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypter
{
    public static class FileTypeConsts
    {
        public static string FileTypeFromExt(string ext)
        {
            string type = "";

            if (ext == "")
                return "File";

            switch (ext)
            {
                case ".exe":
                    type = "Executable";
                    break;

                // Images: 
                case ".png":
                case ".jpg":
                case ".jpeg":
                case ".gif":
                case ".JPG":
                case ".PNG":
                case ".JPEG":
                case ".GIF":
                case ".ICO":
                case ".ico":
                    type = ext.Replace(".", "").ToUpper() + " image";
                    break;

                case ".xcf":
                    type = "GIMP work file";
                    break;

                case ".txt":
                    type = "Text file";
                    break;

                case ".lnk":
                    type = "Shortcut";
                    break;

                case ".url":
                    type = "Internet shortcut";
                    break;

                case ".log":
                    type = "Log file";
                    break;

                case ".html":
                    type = "HTML file";
                    break;

                case ".ini":
                    type = "INI configuration file";
                    break;

                case ".jar":
                    type = "Executable jar file";
                    break;

                case ".js":
                    type = "Javascript file";
                    break;

                default:
                    type = ext.Replace(".", "").ToUpper() + " file";
                    break;
            }

            return type;
        }
    }

    public class FileHelper
    {
        public static string GetSizeReadable(long i)
        {
            if (i == 0)
                return "Empty files cannot be encrypted";

            string sign = (i < 0 ? "-" : "");
            double readable = (i < 0 ? -i : i);
            string suffix;
            if (i >= 0x1000000000000000) // Exabyte
            {
                suffix = "EB";
                readable = (double)(i >> 50);
            }
            else if (i >= 0x4000000000000) // Petabyte
            {
                suffix = "PB";
                readable = (double)(i >> 40);
            }
            else if (i >= 0x10000000000) // Terabyte
            {
                suffix = "TB";
                readable = (double)(i >> 30);
            }
            else if (i >= 0x40000000) // Gigabyte
            {
                suffix = "GB";
                readable = (double)(i >> 20);
            }
            else if (i >= 0x100000) // Megabyte
            {
                suffix = "MB";
                readable = (double)(i >> 10);
            }
            else if (i >= 0x400) // Kilobyte
            {
                suffix = "KB";
                readable = (double)i;
            }
            else
            {
                return i.ToString(sign + "0 B"); // Byte
            }
            readable = readable / 1024;

            return (sign + String.Format("{0:n0} ", readable) + suffix);
        }
    }
}

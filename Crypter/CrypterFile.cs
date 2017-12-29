using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Crypter
{
    public class CrypterFile : INotifyPropertyChanged
    {
        private MainWindow mainWindowInstance;

        /// PropertyChanged event handler
        public event PropertyChangedEventHandler PropertyChanged;

        /// Property changed Notification        
        public void RaisePropertyChanged(string propertyName)
        {
            // take a copy to prevent thread issues
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string _path;

        public string Path {
            get {
                return _path;
            }
            set
            {
                _path = value;
                RaisePropertyChanged("Path");
            }
        }

        public string TrimmedPath
        {
            get
            {
                return PathShortener(Path);
            }
        }

        public string OGName {
            get
            {
                if (IsEncrypted)
                    return System.IO.Path.GetFileNameWithoutExtension(Path);
                else
                    return System.IO.Path.GetFileName(Path);
            }
        }

        private bool _is_crypting;

        public bool IsCrypting
        {
            get
            {
                return _is_crypting;
            }
            set
            {
                _is_crypting = value;
                RaisePropertyChanged("IsCrypting");
            }
        }

        private bool _is_encrypted;

        public bool IsEncrypted
        {
            get
            {
                return _is_encrypted;
            }
            set
            {
                _is_encrypted = value;
                if (_is_encrypted)
                    CryptProgress = 1;
                RaisePropertyChanged("IsEncrypted");
            }
        }

        public string FileType
        {
            get
            {
                if (IsEncrypted)
                    return FileTypeConsts.FileTypeFromExt(System.IO.Path.GetExtension(OGName));
                else
                    return FileTypeConsts.FileTypeFromExt(System.IO.Path.GetExtension(Path));
            }
        }

        private double _crypt_progress = -.05;

        public double CryptProgress
        {
            get
            {
                return _crypt_progress + .05;
            }
            set
            {
                _crypt_progress = value;
                RaisePropertyChanged("CryptProgress");
                //StateChange();
            }
        }

        public long Size
        {
            get
            {
                if (File.Exists(Path))
                {
                    FileInfo fi = new FileInfo(Path);
                    return fi.Length;
                }
                else
                    return 0;
            }
        }

        public string HumanReadableSize
        {
            get
            {
                return FileHelper.GetSizeReadable(Size);
            }
        }

        [DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
        static extern bool PathCompactPathEx([Out] StringBuilder pszOut, string szPath, int cchMax, int dwFlags);

        string PathShortener(string path)
        {     

            double fullStringWidth = MeasureString(path).Width;
            double widthAvailable = mainWindowInstance.fileList.ActualWidth-(8+20+6+80);

            if (fullStringWidth > widthAvailable)
            {
                int length = path.Length-1;

                while((MeasureString(path.Substring(0, length))).Width > widthAvailable)
                {
                    length--;
                }

                return ShortPath(path, length);
            }
            else
            {
                return path;
            }
        }

        string ShortPath(string path, int length)
        {
            StringBuilder sb = new StringBuilder(length);
            PathCompactPathEx(sb, path, length, 0);
            return sb.ToString();
        }

        private Size MeasureString(string candidate)
        {
            var formattedText = new FormattedText(
                candidate,
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                new Typeface(mainWindowInstance.fileList.FontFamily, mainWindowInstance.fileList.FontStyle, mainWindowInstance.fileList.FontWeight, mainWindowInstance.fileList.FontStretch),
                mainWindowInstance.fileList.FontSize,
                Brushes.Black);

            return new Size(formattedText.Width, formattedText.Height);
        }

        public BackgroundWorker worker = new BackgroundWorker();

        // For designer...
        public CrypterFile() : this("C:/Users/Kevin/Desktop/bmc_firmware/bmcfl32l", null) { }

        public CrypterFile(string path, MainWindow mainWindowInstance)
        {
            this.mainWindowInstance = mainWindowInstance;

            this._path = path;

            if (System.IO.Path.GetExtension(Path) == ".crypter")
                IsEncrypted = true;
            else
                IsEncrypted = false;

            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
        }

        public void StateChange()
        {
            mainWindowInstance.fileList_SelectionChanged(null, null);
        }

        public void Encrypt(SecureString password)
        {
            if (Size != 0)
            {
                IsCrypting = true;
                worker.RunWorkerAsync(new CryptoAction(CryptoActionType.Encrypt, password));
                StateChange();
            }
        }

        public void Decrypt(SecureString password)
        {
            if (Size != 0)
            {
                IsCrypting = true;
                worker.RunWorkerAsync(new CryptoAction(CryptoActionType.Decrypt, password));
                StateChange();
            }
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                // System.Security.Cryptography.CryptographicException: Padding is invalid and cannot be removed.
                if (!(e.Error is CryptographicException))
                {
                    MessageBox.Show(e.Error.ToString());
                }
                    
            }
            else
            {
                CryptoAction action = (CryptoAction)e.Result;
                

                if (action.ActionType == CryptoActionType.Encrypt)
                {
                    IsEncrypted = true;
                }
                else if (action.ActionType == CryptoActionType.Decrypt)
                {
                    IsEncrypted = false;
                }
            }

            IsCrypting = false;

            if (!IsEncrypted)
                CryptProgress = -0.05;

            StateChange();
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            CryptoAction action = (CryptoAction)e.Argument;

            if(action.ActionType == CryptoActionType.Encrypt)
            {
                string newPath = Path + ".crypter";

                using (FileStream encryptedFS = new FileStream(newPath, FileMode.Create))
                {
                    using (RijndaelManaged AES = new RijndaelManaged())
                    {
                        AES.KeySize = 256;
                        AES.BlockSize = 128;

                        var key = new Rfc2898DeriveBytes(Crypto.GetSecureStringBytes(action.Password), Crypto.saltBytes, 1000);
                        AES.Key = key.GetBytes(AES.KeySize / 8);
                        AES.IV = key.GetBytes(AES.BlockSize / 8);

                        AES.Mode = CipherMode.CBC;

                        using (var cs = new CryptoStream(encryptedFS, AES.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            using (FileStream fsIn = new FileStream(Path, FileMode.Open))
                            {
                                int data;
                                long finishedBytes = 0;

                                long reportProgressEvery_bytes = fsIn.Length / 500;

                                long report_per_cent_Every_bytes = fsIn.Length / 100;

                                if (report_per_cent_Every_bytes < 1)
                                    report_per_cent_Every_bytes = 1;

                                if (reportProgressEvery_bytes < 1)
                                    reportProgressEvery_bytes = 1;

                                while ((data = fsIn.ReadByte()) != -1)
                                {
                                    cs.WriteByte((byte)data);
                                    finishedBytes++;
                                    //worker.ReportProgress(0, (double)((double)finishedBytes / (double)fsIn.Length));
                                    if (finishedBytes % reportProgressEvery_bytes == 0)
                                        CryptProgress = (double)((double)finishedBytes / (double)fsIn.Length);

                                    if (finishedBytes % report_per_cent_Every_bytes == 0)
                                        StateChange();
                                }

                                fsIn.Close();
                            }

                            cs.Close();
                        }

                        encryptedFS.Close();
                    }
                }

                File.Delete(Path);
                Path = newPath;
            }
            else if (action.ActionType == CryptoActionType.Decrypt)
            {
                string newPath = System.IO.Path.GetDirectoryName(Path) + "\\" + OGName;

                try
                {
                    using (FileStream decryptedFS = new FileStream(newPath, FileMode.Create))
                    {
                        using (RijndaelManaged AES = new RijndaelManaged())
                        {
                            AES.KeySize = 256;
                            AES.BlockSize = 128;

                            var key = new Rfc2898DeriveBytes(Crypto.GetSecureStringBytes(action.Password), Crypto.saltBytes, 1000);
                            AES.Key = key.GetBytes(AES.KeySize / 8);
                            AES.IV = key.GetBytes(AES.BlockSize / 8);

                            AES.Mode = CipherMode.CBC;

                            using (var cs = new CryptoStream(decryptedFS, AES.CreateDecryptor(), CryptoStreamMode.Write))
                            {
                                using (FileStream fsIn = new FileStream(Path, FileMode.Open))
                                {
                                    int data;
                                    long finishedBytes = 0;

                                    long reportProgressEvery_bytes = fsIn.Length / 500;

                                    long report_per_cent_Every_bytes = fsIn.Length / 100;

                                    if (report_per_cent_Every_bytes < 1)
                                        report_per_cent_Every_bytes = 1;

                                    if (reportProgressEvery_bytes < 1)
                                        reportProgressEvery_bytes = 1;

                                    while ((data = fsIn.ReadByte()) != -1)
                                    {
                                        cs.WriteByte((byte)data);
                                        finishedBytes++;
                                        //worker.ReportProgress(0, (double)((double)finishedBytes / (double)fsIn.Length));
                                        if (finishedBytes % reportProgressEvery_bytes == 0)
                                            CryptProgress = 0.95 - (double)((double)finishedBytes / (double)fsIn.Length);

                                        if (finishedBytes % report_per_cent_Every_bytes == 0)
                                            StateChange();
                                    }

                                    fsIn.Close();
                                }

                                cs.Close();
                            }
                        }

                        decryptedFS.Close();
                    }

                    File.Delete(Path);

                    Path = newPath;
                }
                catch (CryptographicException cex)
                {
                    if (cex.Message == "Padding is invalid and cannot be removed.")
                    {
                        File.Delete(newPath);
                        MessageBox.Show("Invalid padding exception.\n\n(This most likely means your key is incorrect.)");
                    }
                    else
                        MessageBox.Show(cex.ToString());

                    throw cex;
                }
            }

            e.Result = action;
        }
    }
}

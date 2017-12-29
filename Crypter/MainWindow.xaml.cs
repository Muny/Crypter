using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Crypter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<CrypterFile> CrypterFiles = new ObservableCollection<CrypterFile>();


        public MainWindow()
        {
            InitializeComponent();

            fileList.Items.Clear();

            CrypterFiles.CollectionChanged += CrypterFiles_CollectionChanged;

            fileList.ItemsSource = CrypterFiles;

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            /*string[] files = Directory.GetFiles(".");

            foreach (string file in files)
            {
                CrypterFiles.Add(new CrypterFile(file, this));
            }*/
        }

        private void fileList_Drop(object sender, DragEventArgs e)
        {
            string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string path in paths)
            {
                if (CrypterFiles.Where(val => path == val.Path).Count() == 0)
                {
                    if (File.GetAttributes(path).HasFlag(FileAttributes.Directory))
                    {
                        string[] files = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly);
                        foreach (string file in files)
                        {
                            CrypterFiles.Add(new CrypterFile(file, this));
                        }
                    }
                    else
                        CrypterFiles.Add(new CrypterFile(path, this));
                }
            }
        }

        private void fileList_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;
        }

        void ui_selectedItemIsEncrypted(bool? isencrypted, bool iscrypting, double percent)
        {
            if (percent < 0)
                percent = 0;
            else if (percent > 1)
                percent = 1;

            if ((isencrypted != null && isencrypted == true) && iscrypting)
            {
                selectedFileIsEncrypted.Text = "DECRYPTING " + (100 - (int)(percent * 100)) + "%";
                selectedFileIsEncrypted.Background = new SolidColorBrush(Color.FromArgb(51, 193, 209, 39));
            }
            else if ((isencrypted != null && isencrypted == false) && iscrypting)
            {
                selectedFileIsEncrypted.Text = "ENCRYPTING " + (int)(percent * 100) + "%";
                selectedFileIsEncrypted.Background = new SolidColorBrush(Color.FromArgb(51, 193, 209, 39));
            }
            else if (isencrypted == null)
            {
                selectedFileIsEncrypted.Text = "N/A";
                selectedFileIsEncrypted.Background = new SolidColorBrush(Color.FromArgb(51, 193, 209, 39));
            }
            else if ((bool)isencrypted)
            {
                selectedFileIsEncrypted.Text = "ENCRYPTED";
                selectedFileIsEncrypted.Background = new SolidColorBrush(Color.FromArgb(51, 0, 255, 174));
            }
            else
            {
                selectedFileIsEncrypted.Text = "NOT ENCRYPTED";
                selectedFileIsEncrypted.Background = new SolidColorBrush(Color.FromArgb(51, 44, 44, 66));
            }
        }

        private void CrypterFiles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            fileList_SelectionChanged(null, null);
        }

        public void fileList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (CrypterFiles.Count > 0)
                {
                    buttonEncryptAll.IsEnabled = true;
                    buttonDecryptAll.IsEnabled = true;
                }
                else
                {
                    buttonEncryptAll.IsEnabled = false;
                    buttonDecryptAll.IsEnabled = false;
                    buttonEncryptSelected.IsEnabled = false;
                    buttonDecryptSelected.IsEnabled = false;
                }

                if (fileList.SelectedItems.Count == 0)
                {
                    selectedFileType.Text = "NO FILE SELECTED";
                    selectedFileName.Text = "-";
                    selectedFileSize.Text = "-";
                    selectedFileEncryptionDate.Text = "-";
                    ui_selectedItemIsEncrypted(null, false, 0);

                    buttonEncryptSelected.IsEnabled = false;
                    buttonDecryptSelected.IsEnabled = false;
                }
                else if (fileList.SelectedItems.Count == 1)
                {
                    CrypterFile cf = ((CrypterFile)fileList.SelectedItem);
                    selectedFileType.Text = cf.FileType;
                    selectedFileName.Text = cf.OGName;
                    selectedFileSize.Text = cf.HumanReadableSize;
                    ui_selectedItemIsEncrypted(cf.IsEncrypted, cf.IsCrypting, cf.CryptProgress);

                    if (cf.IsEncrypted)
                    {
                        buttonEncryptSelected.IsEnabled = false;
                        buttonDecryptSelected.IsEnabled = !cf.IsCrypting;
                    }
                    else
                    {
                        buttonEncryptSelected.IsEnabled = !cf.IsCrypting;
                        buttonDecryptSelected.IsEnabled = false;
                    }
                }
                else
                {
                    int numEncrypted = fileList.SelectedItems.Cast<CrypterFile>().Where(val => val.IsEncrypted == true).Count();
                    int numNotEncrypted = fileList.SelectedItems.Count - numEncrypted;

                    selectedFileType.Text = numEncrypted + " ENCRYPTED";
                    selectedFileName.Text = fileList.SelectedItems.Count + " SELECTED FILES";
                    selectedFileSize.Text = numNotEncrypted + " NOT ENCRYPTED";

                    buttonEncryptSelected.IsEnabled = true;
                    buttonDecryptSelected.IsEnabled = true;

                    ui_selectedItemIsEncrypted(null, false, 0);
                }

                if (passwordBoxKey.SecurePassword.Length == 0)
                {
                    buttonEncryptSelected.IsEnabled = false;
                    buttonDecryptSelected.IsEnabled = false;

                    buttonEncryptAll.IsEnabled = false;
                    buttonDecryptAll.IsEnabled = false;
                }
                
            });
        }

        private void buttonEncryptSelected_Click(object sender, RoutedEventArgs e)
        {
            foreach (CrypterFile cf in fileList.SelectedItems)
            {
                if (!cf.IsEncrypted && !cf.IsCrypting)
                    cf.Encrypt(passwordBoxKey.SecurePassword);
            }
        }

        private void buttonDecryptSelected_Click(object sender, RoutedEventArgs e)
        {
            foreach (CrypterFile cf in fileList.SelectedItems)
            {
                if (cf.IsEncrypted && !cf.IsCrypting)
                    cf.Decrypt(passwordBoxKey.SecurePassword);
            }
        }

        private void buttonEncryptAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (CrypterFile cf in CrypterFiles)
            {
                if (!cf.IsEncrypted && !cf.IsCrypting)
                    cf.Encrypt(passwordBoxKey.SecurePassword);
            }
        }

        private void buttonDecryptAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (CrypterFile cf in CrypterFiles)
            {
                if (cf.IsEncrypted && !cf.IsCrypting)
                    cf.Decrypt(passwordBoxKey.SecurePassword);
            }
        }

        private void buttonKeyHamburger_Click(object sender, RoutedEventArgs e)
        {
            KeyOptions.IsManipulationEnabled = !KeyOptions.IsManipulationEnabled;
        }

        private void Grid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            KeyOptions.IsManipulationEnabled = false;
        }

        private void passwordBoxKey_PasswordChanged(object sender, RoutedEventArgs e)
        {
            fileList_SelectionChanged(null, null);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (CrypterFile cf in CrypterFiles)
            {
                if (cf.IsCrypting)
                {
                    if (MessageBox.Show("Are you sure you want to exit Crypter? One or more files are still being crypted.", "Crypter", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No) != MessageBoxResult.Yes)
                        e.Cancel = true;
                }
            }
        }

        private void fileList_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            FireEvent(CrypterFiles, "CollectionChanged", this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public static void FireEvent(object onMe, string invokeMe, params object[] eventParams)
        {
            MulticastDelegate eventDelagate =
                  (MulticastDelegate)onMe.GetType().GetField(invokeMe,
                   System.Reflection.BindingFlags.Instance |
                   System.Reflection.BindingFlags.NonPublic).GetValue(onMe);

            Delegate[] delegates = eventDelagate.GetInvocationList();

            foreach (Delegate dlg in delegates)
            {
                dlg.Method.Invoke(dlg.Target, eventParams);
            }
        }
    }
}

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
using System.Threading;
using System.ComponentModel;
using FileChecker.Utils;

// Creating aliased for namespace for winforms
using winForms = System.Windows.Forms;
using System.Reflection;

namespace FileChecker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Checker _file1Checker;
        Checker _file2Checker;
        Compare _fileCompare;

        public BackgroundWorker Worker { get; set; }

        public Checker FileChecker { get; set; }

        public Compare FileCompare { get; set; }

        public string DataSaveDirectory { get; set; }

        public string ResultsSaveDirectory { get; set; }

        public string OutputSaveDirectory { get; set; }

        public string SelectFolderOpeningDirectory { get; set; }

        public string File1OpeningPath { get; set; }

        public string File2OpeningPath { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            Worker = new BackgroundWorker();
            Worker.WorkerReportsProgress = true;

            Worker.DoWork += backgroundWorker_DoWork;
            Worker.ProgressChanged += backgroundWorker_ProgressChanged;
            Worker.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;

            FileChecker = new Checker(Worker);

            progressBar.Value = 0;
            lblGenerateFilePath.Content = "Working file path";

            DataSaveDirectory = AppDomain.CurrentDomain.BaseDirectory + "Data\\MD5";
            Utils.ImportExportUtils.EnsureDirectory(DataSaveDirectory);

            ResultsSaveDirectory = AppDomain.CurrentDomain.BaseDirectory + "Data\\Comparison";
            Utils.ImportExportUtils.EnsureDirectory(ResultsSaveDirectory);

            OutputSaveDirectory = AppDomain.CurrentDomain.BaseDirectory + "Output";
            Utils.ImportExportUtils.EnsureDirectory(OutputSaveDirectory);           

            dgvResults.IsReadOnly = true;
            cbFile1Loaded.IsEnabled = false;
            cbFile2Loaded.IsEnabled = false;

            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            Title = Title + " " + version.Major + "." + version.Minor + "." + version.Build + " RC" + version.Revision;
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                FileChecker.GenerateHashList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //Update UI
            progressBar.Value = FileChecker.WorkingProgress;//e.ProgressPercentage;
            lblGenerateFilePath.Content = FileChecker.LastWorkFilePath;
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (FileChecker.JobDone)
                {
                    var fileName = "MD5_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss").ToString();
                    Utils.ImportExportUtils.WriteToXml<Checker>(FileChecker, fileName, DataSaveDirectory);
                    MessageBox.Show($"Completed!\nFile save as {DataSaveDirectory}\\{fileName}.xml", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    //dgvResults.ItemsSource = FileChecker.CheckerList;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnGenerate.IsEnabled = true;
            }
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.btnBrowse.IsEnabled = false;
                using (var folderBrowser = new winForms::FolderBrowserDialog())
                {
                    // Set opening directory for the Folder Browser
                    if (!string.IsNullOrEmpty(SelectFolderOpeningDirectory) && Directory.Exists(SelectFolderOpeningDirectory))
                    {
                        folderBrowser.SelectedPath = SelectFolderOpeningDirectory;
                    }
                    winForms::DialogResult result = folderBrowser.ShowDialog();
                    if (result == winForms.DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowser.SelectedPath))
                    {
                        this.tbSelectFolder.Text = SelectFolderOpeningDirectory = folderBrowser.SelectedPath;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                this.btnBrowse.IsEnabled = true;
            }
        }

        private void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.btnGenerate.IsEnabled = false;

                if (string.IsNullOrEmpty(this.tbSelectFolder.Text) || !Directory.Exists(this.tbSelectFolder.Text))
                {
                    MessageBox.Show("Please select a valid folder", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.btnGenerate.IsEnabled = true;
                    return;
                }
                FileChecker.SelectedFolderPath = SelectFolderOpeningDirectory = this.tbSelectFolder.Text;
                Worker.RunWorkerAsync();
                //dgvResults.ItemsSource = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnBrowseFile1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnBrowseFile1.IsEnabled = false;
                //_file1Checker = null;
                //cbFile1Loaded.IsChecked = false;

                OpenFileDialog fileDialog = new OpenFileDialog
                {
                    Title = "Browse File",
                    CheckFileExists = true,
                    CheckPathExists = true,

                    DefaultExt = "xml",
                    Filter = "xml files (*.xml)|*.xml",
                    FilterIndex = 2,
                    RestoreDirectory = true,

                    ReadOnlyChecked = true,
                    ShowReadOnly = true
                };

                // Set opening directory for the File Browser
                if (!string.IsNullOrEmpty(File1OpeningPath) && Directory.Exists(File1OpeningPath))
                {
                    fileDialog.InitialDirectory = File1OpeningPath;
                }
                else
                {
                    fileDialog.InitialDirectory = DataSaveDirectory;
                }

                if (fileDialog.ShowDialog() == true)
                {
                    tbSelectFile1.Text = fileDialog.FileName;
                    File1OpeningPath = new FileInfo(tbSelectFile1.Text).Directory.FullName;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                this.btnBrowseFile1.IsEnabled = true;
            }
        }

        private void BtnBrowseFile2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnBrowseFile2.IsEnabled = false;
                //_file2Checker = null;
                //cbFile2Loaded.IsChecked = false;

                OpenFileDialog fileDialog = new OpenFileDialog
                {
                    Title = "Browse File",
                    CheckFileExists = true,
                    CheckPathExists = true,

                    DefaultExt = "xml",
                    Filter = "xml files (*.xml)|*.xml",
                    FilterIndex = 2,
                    RestoreDirectory = true,

                    ReadOnlyChecked = true,
                    ShowReadOnly = true
                };

                // Set opening directory for the File Browser
                if (!string.IsNullOrEmpty(File2OpeningPath) && Directory.Exists(File2OpeningPath))
                {
                    fileDialog.InitialDirectory = File2OpeningPath;
                }
                else
                {
                    fileDialog.InitialDirectory = DataSaveDirectory;
                }

                if (fileDialog.ShowDialog() == true)
                {
                    tbSelectFile2.Text = fileDialog.FileName;
                    File2OpeningPath = new FileInfo(tbSelectFile2.Text).Directory.FullName;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                this.btnBrowseFile2.IsEnabled = true;
            }
        }

        private void BtnLoadFile1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnLoadFile1.IsEnabled = false;
                if (string.IsNullOrEmpty(tbSelectFile1.Text) || !File.Exists(tbSelectFile1.Text) || System.IO.Path.GetExtension(tbSelectFile1.Text) != ".xml")
                {
                    MessageBox.Show("Please select a valid MD5 Data xml file1", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    btnCompare.IsEnabled = true;
                    return;
                }
                _file1Checker = Utils.ImportExportUtils.ReadFromXml<Checker>(tbSelectFile1.Text);
                File1OpeningPath = new FileInfo(tbSelectFile1.Text).Directory.FullName;
                cbFile1Loaded.IsChecked = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("MD5 xml File1 load failed!" + ex, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnLoadFile1.IsEnabled = true;
            }
        }

        private void BtnLoadFile2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnLoadFile2.IsEnabled = false;
                if (string.IsNullOrEmpty(tbSelectFile2.Text) || !File.Exists(tbSelectFile2.Text) || System.IO.Path.GetExtension(tbSelectFile2.Text) != ".xml")
                {
                    MessageBox.Show("Please select a valid data xml file2", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    btnCompare.IsEnabled = true;
                    return;
                }
                _file2Checker = Utils.ImportExportUtils.ReadFromXml<Checker>(tbSelectFile2.Text);
                File2OpeningPath = new FileInfo(tbSelectFile2.Text).Directory.FullName;
                cbFile2Loaded.IsChecked = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("MD5 xml File2 load failed!" + ex, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnLoadFile2.IsEnabled = true;
            }
        }

        private void TbSelectFile1_TextChanged(object sender, TextChangedEventArgs e)
        {
            _file1Checker = null;
            cbFile1Loaded.IsChecked = false;
        }

        private void TbSelectFile2_TextChanged(object sender, TextChangedEventArgs e)
        {
            _file2Checker = null;
            cbFile2Loaded.IsChecked = false;
        }

        private void BtnCompare_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnCompare.IsEnabled = false;
                dgvResults.ItemsSource = null;
                _fileCompare = null;

                if (cbFile1Loaded.IsChecked != true && cbFile2Loaded.IsChecked != true)
                {
                    MessageBox.Show("MD5 xml File1 and File2 are not loaded! \nPlease load them.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else if (cbFile1Loaded.IsChecked != true)
                {
                    MessageBox.Show("MD5 xml File1 is not loaded! \nPlease load it.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else if (cbFile2Loaded.IsChecked != true)
                {
                    MessageBox.Show("MD5 xml File2 is not loaded! \nPlease load it.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _fileCompare = new Compare(_file1Checker, _file2Checker);
                _fileCompare.DoCompare();
                var fileName = "Comparison_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss").ToString();
                Utils.ImportExportUtils.WriteToXml<Compare>(_fileCompare, fileName, ResultsSaveDirectory);

                // Cloned and pass the list for csv extension, since inside we are replacing ',' with '.'
                List<CompareMember> clonedList = _fileCompare.FullResult.ConvertAll(x => x.Clone() as CompareMember);
                Utils.ImportExportUtils.WriteToCSV(clonedList, fileName, OutputSaveDirectory);

                lbFolder1Directory.Content = _fileCompare.MD5File1SelectedFolderPath;
                lbFolder2Directory.Content = _fileCompare.MD5File2SelectedFolderPath;
                dgvResults.ItemsSource = _fileCompare.FullResult;

                MessageBox.Show($"Completed!\nFile save as {OutputSaveDirectory}\\{fileName}.csv", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnCompare.IsEnabled = true;
            }
        }
    }
}

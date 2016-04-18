using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PhoneApp5.Resources;
using System.IO.IsolatedStorage;
using System.IO;
using System.Collections.ObjectModel;
using Coding4Fun.Toolkit.Controls;

namespace PhoneApp5
{
    public partial class MainPage : PhoneApplicationPage
    {
        IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
        IsolatedStorageFile fileStorage = IsolatedStorageFile.GetUserStoreForApplication();
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            InitializeSettings();
        }  

        private void InitializeSettings() 
        {
            //Tworzenie katalogów
            fileStorage.CreateDirectory("tempFiles");
            fileStorage.CreateDirectory("anotherFolder");
            //Utworzenie pliku newTempFile.txt
            StreamWriter fileWriter = new StreamWriter(new IsolatedStorageFileStream(@"tempFiles\newTempFile.txt", System.IO.FileMode.OpenOrCreate, fileStorage));
            //Wpisanie linijki do powyższego pliku
            fileWriter.WriteLine("To jest nowa linijka w pliku");
            fileWriter.Close();
            fillDirs();
        }

        private void fillDirs()
        {
            //Listing katalogów
            var dirs = GetAllDirectories("*", fileStorage);
            //Pomijanie domyślnych folderów
            List<string> exclude = new List<string>();
            exclude.Add("Shared");
            exclude.Add("Shared/Media");
            exclude.Add("Shared/ShellContent");
            exclude.Add("Shared/Transfers");
            
            dirs = dirs.Except(exclude).ToList();
            //Uzupełnienie comboboxa
            DirectoriesCombo.ItemsSource = dirs;
            DirectoriesCombo.SelectedIndex = -1;
        }

        int i = 1;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (filesCombo.SelectedIndex != -1)
            {
                var dir = DirectoriesCombo.SelectedItem.ToString();
                var file = filesCombo.SelectedItem.ToString();
                var path = dir + "/" + file;
                var fileReader = new StreamReader(new IsolatedStorageFileStream(path, FileMode.Open, fileStorage));
                string data = fileReader.ReadToEnd();
                fileReader.Close();
                teksty.Text = data + " " + i++.ToString();
            }
            else
            {
                MessageBox.Show("Wybierz katalog, a następnie plik");
            }
        }

        int cnt = 1;

        public static List<String> GetAllDirectories(string pattern, IsolatedStorageFile storeFile)
        {
            // Get the root of the search string.
            string root = Path.GetDirectoryName(pattern);

            if (root != "")
            {
                root += "/";
            }

            // Retrieve directories.
            List<String> directoryList = new List<String>(storeFile.GetDirectoryNames(pattern));

            // Retrieve subdirectories of matches.
            for (int i = 0, max = directoryList.Count; i < max; i++)
            {
                string directory = directoryList[i] + "/";
                List<String> more = GetAllDirectories(root + directory + "*", storeFile);

                // For each subdirectory found, add in the base path.
                for (int j = 0; j < more.Count; j++)
                {
                    more[j] = directory + more[j];
                }

                // Insert the subdirectories into the list and
                // update the counter and upper bound.
                directoryList.InsertRange(i + 1, more);
                i += more.Count;
                max += more.Count;
            }

            return directoryList;
        }

        public static List<String> GetAllFiles(string pattern, IsolatedStorageFile storeFile)
        {
            // Get the root and file portions of the search string.
            string fileString = Path.GetFileName(pattern);

            List<String> fileList = new List<String>(storeFile.GetFileNames(pattern));

            // Loop through the subdirectories, collect matches,
            // and make separators consistent.
            foreach (string directory in GetAllDirectories("*", storeFile))
            {
                foreach (string file in storeFile.GetFileNames(directory + "/" + fileString))
                {
                    fileList.Add((directory + "/" + file));
                }
            }

            return fileList;
        } // End of GetFiles.

        public static List<String> GetFilesFromDirectory(string pattern, IsolatedStorageFile storeFile)
        {
            string fileString = Path.GetFileName(pattern);

            List<String> fileList = new List<String>(storeFile.GetFileNames(pattern));
            return fileList;
        }

        private void DirectoriesCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var t = DirectoriesCombo.SelectedItem.ToString() + "/";
                var list = GetFilesFromDirectory(t, fileStorage);
                filesCombo.ItemsSource = list;
            }
            catch
            {
                fillDirs();
            }
        }

        private void FilesCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void btn_newDirectory_Click(object sender, RoutedEventArgs e)
        {
            InputPrompt input = new InputPrompt();
            input.Completed += newDirectoryInputCompleted;
            input.Title = "Wybór nazwy";
            input.Message = "Wybierz nazwę dla nowego katalogu";
            input.Show();
        }

        private void newDirectoryInputCompleted(object sender, PopUpEventArgs<string, PopUpResult> e)
        {
            try
            {
                var t = e.Result.ToString();
                fileStorage.CreateDirectory(t);
            }
            catch
            {
                MessageBox.Show("Nie utworzono katalogu");
            }
            fillDirs();
        }

        private void btn_newFile_Click(object sender, RoutedEventArgs e)
        {
            InputPrompt input = new InputPrompt();
            input.Completed += newFileInputCompleted;
            input.Title = "Wybór nazwy";
            input.Message = "Wybierz nazwę dla nowego katalogu";
            input.Show();
        }

        private void newFileInputCompleted(object sender, PopUpEventArgs<string, PopUpResult> e)
        {
            var t = e.Result.ToString();
            var dir = DirectoriesCombo.SelectedItem.ToString() + "/";
            t = dir + t;
            try
            {
                fileStorage.CreateFile(t);
            }
            catch
            {
                MessageBox.Show("Wystąpił błąd podczas tworzenia katalogu");
            }
            var tt = DirectoriesCombo.SelectedItem.ToString() + "/";
            var list = GetFilesFromDirectory(tt, fileStorage);
            filesCombo.ItemsSource = list;

        }

        private void btn_remDir_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var t = DirectoriesCombo.SelectedItem.ToString();
                DeleteDirectory(t, fileStorage);
                fillDirs();
            }
            catch
            {
                MessageBox.Show("Błąd podczas usuwania.");
            }
        }

        public void DeleteDirectory(string dir, IsolatedStorageFile isf)
        {
            foreach (var file in isf.GetFileNames(dir + "\\*"))
            {
                var t = dir + "\\" + file;
                try
                {
                    isf.DeleteFile(t);
                }
                catch
                {
                    MessageBox.Show("Błąd podczas usuwania");
                }
            }

            foreach (var subdir in isf.GetDirectoryNames(dir + "\\*"))
            {
                var t = dir +"\\"+ subdir;
                isf.DeleteDirectory(t);
            }

            isf.DeleteDirectory(dir.TrimEnd('\\'));
            filesCombo.ItemsSource = null;
            fillDirs();
        }

        private void btn_remFIle_Click(object sender, RoutedEventArgs e)
        {
            if (DirectoriesCombo.SelectedIndex != -1 && filesCombo.SelectedIndex != -1)
            {
                var file = filesCombo.SelectedItem.ToString();
                var dir = DirectoriesCombo.SelectedItem.ToString();
                var path = dir + "\\" + file;
                fileStorage.DeleteFile(path);
                DirectoriesCombo_SelectionChanged(null, null);
            }
            else
            {
                MessageBox.Show("Nie wybrałeś pliku");
            }
        }

        private void btn_fileedit_Click(object sender, RoutedEventArgs e)
        {
            if (DirectoriesCombo.SelectedIndex != -1 && filesCombo.SelectedIndex != -1)
            {
                InputPrompt input = new InputPrompt();
                input.Completed += editFileInputCompleted;
                input.Title = "Edycja: " + filesCombo.SelectedItem.ToString();

                var dir = DirectoriesCombo.SelectedItem.ToString();
                var file = filesCombo.SelectedItem.ToString();
                var path = dir + "/" + file;
                var fileReader = new StreamReader(new IsolatedStorageFileStream(path, FileMode.Open, fileStorage));
                string data = fileReader.ReadToEnd();
                fileReader.Close();
                input.Value = data;
                input.Tag = filesCombo.SelectedItem.ToString();
                input.Show();
            }
            else
            {
                MessageBox.Show("Nie wybrałeś pliku");
            }
        }

        private void editFileInputCompleted(object sender, PopUpEventArgs<string, PopUpResult> e)
        {
            var file = (sender as InputPrompt).Tag.ToString();
            var dir = DirectoriesCombo.SelectedItem.ToString();
            var path = dir + "\\" + file;
            using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(path, FileMode.Truncate, fileStorage))
            {
                using (StreamWriter writer = new StreamWriter(isoStream))
                {
                    writer.WriteLine(e.Result);
                    writer.Close();
                }
            }

        }

        private void btn_readVar_Click(object sender, RoutedEventArgs e)
        {
            InputPrompt input = new InputPrompt();
            input.Completed += addVarInputCompleted;
            input.Title = "Podaj nazwę zmiennej";
            input.Show();
        }

        private void addVarInputCompleted(object sender, PopUpEventArgs<string, PopUpResult> e)
        {
            try
            {
                MessageBox.Show(settings[e.Result].ToString());
            }
            catch
            {
                MessageBox.Show("Taka zmienna nie istnieje");
            }
            
        }

        private void btn_saveVar_Click(object sender, RoutedEventArgs e)
        {
            InputPrompt input = new InputPrompt();
            input.Completed += getNewNameVarInputCompleted;
            input.Title = "Podaj nazwę zmiennej";
            input.Show();
        }

        private void getNewNameVarInputCompleted(object sender, PopUpEventArgs<string, PopUpResult> e)
        {
            InputPrompt input = new InputPrompt();
            input.Completed += saveVarInputCompleted;
            input.Title = "Podaj wartość zmiennej " + e.Result.ToString();
            input.Tag = e.Result.ToString();
            input.Show();
        }

        private void saveVarInputCompleted(object sender, PopUpEventArgs<string, PopUpResult> e)
        {
            settings[(sender as InputPrompt).Tag.ToString()] = e.Result.ToString();
        }
    }
}

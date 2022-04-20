using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
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
using Path = System.IO.Path;

namespace RegRenameWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        
        #region Public View Properties
        private bool _IsDraggingOver = false;
        public bool IsDraggingOver { get => _IsDraggingOver; set => SetField(ref _IsDraggingOver, value); }
        
        private string _pattern;
        public string Pattern { get => _pattern; set => SetField(ref _pattern, value); }
        
        private string _replacement;
        public string Replacement { get => _replacement; set => SetField(ref _replacement, value); }
        
        private ObservableCollection<string> _filePaths;
        public ObservableCollection<string> FilePaths { get => _filePaths; set => SetField(ref _filePaths, value); }
        
        private ObservableCollection<string> _oldFileNames;
        public ObservableCollection<string> OldFileNames { get => _oldFileNames; set => SetField(ref _oldFileNames, value); }
        
        private ObservableCollection<string> _newFileNames;
        public ObservableCollection<string> NewFileNames { get => _newFileNames; set => SetField(ref _newFileNames, value); }
        #endregion
        
        private Dictionary<string, string> ReplacementMapping { get; set; }

        #region Events
        private void MainWindow_OnDragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                IsDraggingOver = false;
            }
            else
            {
                IsDraggingOver = true;
            }
        }

        private void MainWindow_OnDragLeave(object sender, DragEventArgs e)
        {
            IsDraggingOver = false;
        }

        private void MainWindow_OnDragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) IsDraggingOver = false;
            else IsDraggingOver = true;
        }
        private void MainWindow_OnDrop(object sender, DragEventArgs e)
        {
            string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop);
            UpdateFilePaths(paths);

            IsDraggingOver = false;
            e.Handled = true;
        }
        private void RenameButton_OnClick(object sender, RoutedEventArgs e)
        {
            UpdateFilePaths(FilePaths.ToArray());
            Replace();
        }
        private void PreviewButton_OnClick(object sender, RoutedEventArgs e)
        {
            UpdateFilePaths(FilePaths.ToArray());
        }
        #endregion
        
        #region Routine
        private void UpdateFilePaths(string[] paths)
        {
            FilePaths = new ObservableCollection<string>(paths ?? Array.Empty<string>());
            ReplacementMapping = new Dictionary<string, string>();
            OldFileNames = new ObservableCollection<string>();
            NewFileNames = new ObservableCollection<string>();
            
            foreach (string filePath in FilePaths)
            {
                string oldName = Path.GetFileName(filePath);
                OldFileNames.Add(oldName);

                string newName = oldName;
                if (!string.IsNullOrEmpty(Pattern) && !string.IsNullOrEmpty(Replacement))
                {
                    try
                    {
                        newName = Regex.Replace(oldName, Pattern, Replacement);
                    }
                    catch (Exception e)
                    {
                        newName = $"Invalid pattern/replacement ({e.Message})";
                    }
                }
                NewFileNames.Add(newName);
                
                ReplacementMapping.Add(filePath, Path.Combine(Path.GetDirectoryName(filePath), newName));
            }
        }

        private void Replace()
        {
            if (ReplacementMapping != null 
                && ReplacementMapping.Count != 0
                && ReplacementMapping.Values.All(nfp => !File.Exists(nfp)))
            {
                foreach (KeyValuePair<string,string> keyValuePair in ReplacementMapping)
                {
                    string oldPath = keyValuePair.Key;
                    string newPath = keyValuePair.Value;

                    if (File.Exists(oldPath) && !File.Exists(newPath))
                    {
                        File.Move(oldPath, newPath);
                    }
                }
            }
        }
        #endregion

        #region Data Binding
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private bool SetField<TType>(ref TType field, TType value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<TType>.Default.Equals(field, value)) return false;
            field = value;
            NotifyPropertyChanged(propertyName);
            return true;
        }
        #endregion
    }
}
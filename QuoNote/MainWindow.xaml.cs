using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using QuoNote;
using System.Threading.Tasks;

namespace QuoNote
{
    public sealed partial class MainWindow : Window
    {
        private DataManagement _dataManagement = new DataManagement();
        public ObservableCollection<Note> Notes { get; set; } = new();
        public MainWindow()
        {
            InitializeComponent();
            LoadNotesAsync();

        }

        public async Task LoadNotesAsync()
        {
            Notes.Clear();

            var notesDb = await _dataManagement.GetNotesAsync();

            foreach(var el in notesDb)
            {
                Notes.Add(el);
            }
        }
        
        private async void AddProject_Click(object sender, RoutedEventArgs e)
        {
            var newNote = new Note()
            {
                Title = $"Notatka #{Notes.Count + 1}",
                DateOfCreation = DateTime.Now,
                DateOfModification = DateTime.Now
            };

            await _dataManagement.AddNoteAsync(newNote);

            await LoadNotesAsync();
        }
    }
}

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
using Microsoft.UI.Input;

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

        private async void NotesList_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var properties = e.GetCurrentPoint((UIElement)sender).Properties;
            if (properties.IsRightButtonPressed)
            {
                var dataContext = ((FrameworkElement)e.OriginalSource).DataContext;
                Note note = (Note)dataContext;
                MenuFlyout menuFlyout = new();

                MenuFlyoutItem deleteFlyoutItem = new();
                MenuFlyoutItem nameFlyoutItem = new();
                MenuFlyoutItem pinFlyoutItem = new();

                if (note.Pinned == true)
                {
                    pinFlyoutItem.Text = "Unpin";
                }
                else
                {
                    pinFlyoutItem.Text = "Pin";
                }
                deleteFlyoutItem.Text = $"Delete";
                nameFlyoutItem.Text = $"Change name";
                

                menuFlyout.Items.Add(pinFlyoutItem);
                menuFlyout.Items.Add(nameFlyoutItem);
                menuFlyout.Items.Add(deleteFlyoutItem);

                pinFlyoutItem.Click += async (sender, args) =>
                {
                    if (note.Pinned == false)
                    {
                        note.Pinned = true;
                    }
                    else
                    {
                        note.Pinned = false;
                    }

                    await _dataManagement.UpdateNoteAsync(note);
                    await LoadNotesAsync();
                };

                deleteFlyoutItem.Click += async (sender, args) =>
                {
                    await _dataManagement.RemoveNoteAsync(((Note)dataContext).Id);
                    await LoadNotesAsync();
                };
                nameFlyoutItem.Click += async (sender, args) =>
                {

                    TextBox tx = new()
                    {
                        Text = note.Title,
                        PlaceholderText = note.Title,
                        AcceptsReturn = false
                    };

                    ContentDialog dialog = new();
                    dialog.Title = "Change note's title:";
                    dialog.Content = tx;
                    dialog.PrimaryButtonText = "Change";
                    dialog.SecondaryButtonText = "Cancel";
                    dialog.XamlRoot = this.Content.XamlRoot;

                    ContentDialogResult result = await dialog.ShowAsync();

                    if (result == ContentDialogResult.Primary)
                    {
                        if (String.IsNullOrWhiteSpace(tx.Text))
                        {
                            return;
                        }
                        note.Title = tx.Text;
                        await _dataManagement.UpdateNoteAsync(note);
                        await LoadNotesAsync();
                    };
                };


                PointerPoint pp = e.GetCurrentPoint((UIElement)sender);
                Point ptElement = new Point(pp.Position.X, pp.Position.Y);
                menuFlyout.ShowAt((FrameworkElement)sender, ptElement);
            }
        }

        private void NotesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NotesList.SelectedItem is Note selectedNote)
            {
                TitleOfChosen.Text = selectedNote.Title;
            }
        }

        private void SearchingBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (String.IsNullOrWhiteSpace(SearchingBox.Text))
            {
                NotesList.ItemsSource = Notes;
            }
            else
            {
                var filteredN = new ObservableCollection<Note>(
                    Notes.Where(n => n.Title.Contains(SearchingBox.Text, StringComparison.OrdinalIgnoreCase))
                );
                NotesList.ItemsSource = filteredN;
            }
        }
    }
}

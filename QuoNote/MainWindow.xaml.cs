using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using QuoNote;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI;

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
                    ContentDialog dialog = new();
                    dialog.Title = "Are you sure?";
                    dialog.Content = $"Do you want to remove {note.Title}?";
                    dialog.PrimaryButtonText = "Yes";
                    dialog.SecondaryButtonText = "Cancel";
                    dialog.XamlRoot = this.Content.XamlRoot;

                    ContentDialogResult result = await dialog.ShowAsync();

                    if (result == ContentDialogResult.Primary)
                    {
                        ContentGroup.Visibility = Visibility.Collapsed;
                        await _dataManagement.RemoveNoteAsync(((Note)dataContext).Id);
                        await LoadNotesAsync();
                    }
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
                        ContentGroup.Visibility = Visibility.Collapsed;
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

        private async void NotesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NotesList.SelectedItem is Note selectedNote)
            {
                using IRandomAccessStream stream = new InMemoryRandomAccessStream();
                using StreamWriter writer = new(stream.AsStreamForWrite());
                await writer.WriteAsync(selectedNote.MarkdownContent);
                await writer.FlushAsync();
                stream.Seek(0);

                ContentGroup.Visibility = Visibility.Visible;
                TitleOfChosen.Text = selectedNote.Title;

                ContentBox.Document.LoadFromStream(Microsoft.UI.Text.TextSetOptions.FormatRtf, stream);
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

        private void ContentBlock_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var properties = e.GetCurrentPoint((UIElement)sender).Properties;
            if (properties.IsLeftButtonPressed)
            {
                ContentBox.Visibility = Visibility.Visible;
            }
        }

        private async void ContentBox_TextChanged(object sender, RoutedEventArgs e)
        {
            if (NotesList.SelectedItem is Note selectedNote)
            {
                using IRandomAccessStream stream = new InMemoryRandomAccessStream();
                ContentBox.Document.SaveToStream(Microsoft.UI.Text.TextGetOptions.FormatRtf, stream);
                stream.Seek(0);
                using StreamReader reader = new(stream.AsStreamForRead());
                selectedNote.MarkdownContent = await reader.ReadToEndAsync();
                await _dataManagement.UpdateNoteAsync(selectedNote);
            }
        }

        private void RichTextBlock_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var properties = e.GetCurrentPoint((UIElement)sender).Properties;
            if (properties.IsLeftButtonPressed)
            {

            }
        }

        private void Bold_Text_Click(object sender, RoutedEventArgs e)
        {
            var format = ContentBox.Document.Selection.CharacterFormat;

            format.Bold = format.Bold == FormatEffect.On ? FormatEffect.Off : FormatEffect.On;
        }

        private void Italic_Text_Click(object sender, RoutedEventArgs e)
        {
            var format = ContentBox.Document.Selection.CharacterFormat;

            format.Italic = format.Italic == FormatEffect.On ? FormatEffect.Off : FormatEffect.On;
        }

        private void Underline_Text_Click(object sender, RoutedEventArgs e)
        {
            var format = ContentBox.Document.Selection.CharacterFormat;

            format.Underline = format.Underline == UnderlineType.None ? UnderlineType.Single : UnderlineType.None;
        }

        private void Copy_Text_Click(object sender, RoutedEventArgs e)
        {
            ContentBox.Document.Selection.Copy();
        }

        private void Paste_Text_Click(object sender, RoutedEventArgs e)
        {
            ContentBox.Document.Selection.Paste(0);
        }

        private void Select_All_Click(object sender, RoutedEventArgs e)
        {
            ContentBox.Document.Selection.Expand(TextRangeUnit.Story);
        }

        private void Cut_Text_Click(object sender, RoutedEventArgs e)
        {
            ContentBox.Document.Selection.Cut();
        }

        private void Highlight_Text_Click(object sender, RoutedEventArgs e)
        {
            var selection = ContentBox.Document.Selection;

            if (selection.Length == 0)
            {
                return;
            }

            var range = ContentBox.Document.GetRange(selection.StartPosition, selection.EndPosition);

            bool hasHL = false;

            for (int i = selection.StartPosition; i < selection.EndPosition; i++)
            {
                var character = ContentBox.Document.GetRange(i, i + 1);

                if (character.CharacterFormat.BackgroundColor == Colors.Gray)
                {
                    hasHL = true;
                    break;
                }
            }

            range.CharacterFormat.BackgroundColor = hasHL ? Colors.Transparent : Colors.Gray;
        }
    }
}

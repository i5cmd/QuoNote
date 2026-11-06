using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace QuoNote
{
    public class Note
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required DateTime DateOfCreation { get; set; }
        public required DateTime DateOfModification { get; set; } 
        public string MarkdownContent { get; set; } = String.Empty;
        public bool Pinned { get; set; } = false;

        [SetsRequiredMembers]
        public Note()
        {
            Title = String.Empty;
            DateOfCreation = DateTime.Now;
            DateOfModification = DateTime.Now;
        }

    }
}

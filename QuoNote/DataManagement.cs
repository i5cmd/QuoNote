using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuoNote
{
    public class DataManagement
    {
        public async Task<List<Note>> GetNotesAsync()
        {
            using var db = new AppDbContext();
            return await db.Notes
                .OrderByDescending(n => n.Pinned)
                .ThenByDescending(n => n.DateOfModification)
                .ToListAsync();
        } 
        
        public async Task AddNoteAsync(Note note)
        {
            using var db = new AppDbContext();
            await db.Notes.AddAsync(note);
            await db.SaveChangesAsync();
        }

        public async Task UpdateNoteAsync(Note note)
        {
            using var db = new AppDbContext();
            db.Notes.Update(note);
            await db.SaveChangesAsync();
        }

        public async Task RemoveNoteAsync(int noteId)
        {
            using var db = new AppDbContext();
            var note = await db.Notes.FindAsync(noteId);
            if (note != null)
            {
                db.Notes.Remove(note);
                await db.SaveChangesAsync();
            }
        }
    }
}

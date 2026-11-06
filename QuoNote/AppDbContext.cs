using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace QuoNote
{
    public class AppDbContext : DbContext
    {
        public DbSet<Note> Notes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appPath = Path.Combine(appDataPath, "QuoNotes");

            Directory.CreateDirectory(appPath);

            string dbPath = Path.Combine(appPath, "quonotedata.db");

            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
    }
}

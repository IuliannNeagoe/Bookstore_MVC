﻿using BookstoreWeb_Temp.Models;
using Microsoft.EntityFrameworkCore;

namespace BookstoreWeb_Temp.Data
{
    public class ApplicationDbContext: DbContext
    {
        public DbSet<Category> Categories { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

    }
}

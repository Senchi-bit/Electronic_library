using System;
using System.Collections.Generic;
using Library.Entities;
using Microsoft.EntityFrameworkCore;

namespace Library.Context;

public partial class MyDbContext : DbContext
{
    public MyDbContext()
    {
    }

    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Author> Authors { get; set; }

    public virtual DbSet<Book> Books { get; set; }

    public virtual DbSet<Exhibition> Exhibitions { get; set; }

    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    // {
    //     //File for connection string
    //     var config = new ConfigurationBuilder()
    //         .AddJsonFile("appsettingsdb.json")
    //         .SetBasePath(Directory.GetCurrentDirectory())
    //         .Build();
    //
    //     optionsBuilder.UseNpgsql(config.GetConnectionString("DefaultConnection"));
    // }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Author>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Authors_pkey");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.FullName).HasColumnName("fullName");

            entity.HasMany(d => d.Books).WithMany(p => p.Authors)
                .UsingEntity<Dictionary<string, object>>(
                    "AuthorBook",
                    r => r.HasOne<Book>().WithMany()
                        .HasForeignKey("BookId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("bookId_fk"),
                    l => l.HasOne<Author>().WithMany()
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("authorId_fk"),
                    j =>
                    {
                        j.HasKey("AuthorId", "BookId").HasName("AuthorBook_pkey");
                        j.ToTable("AuthorBook");
                        j.IndexerProperty<int>("AuthorId").HasColumnName("authorId");
                        j.IndexerProperty<int>("BookId").HasColumnName("bookId");
                    });
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Books_pkey");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.ReleaseYear).HasColumnName("releaseYear");
            entity.Property(e => e.Title).HasColumnName("title");
        });

        modelBuilder.Entity<Exhibition>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("exhibitions_pkey");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.YearBased).HasColumnName("yearBased");
            //entity.HasOne(e => Exhibitions).WithMany(e => e.Books).HasForeignKey("ExhibitionId");
        });
        

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

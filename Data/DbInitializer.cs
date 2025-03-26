// Data/DbInitializer.cs
using Libri.Models;

public static class DbInitializer
{
    public static void Initialize(ApplicationDbContext context)
    {
        context.Database.EnsureCreated();

        if (context.Autori.Any())
        {
            return; // DB già popolato
        }

        var autori = new Autore[]
        {
            new Autore { Cognome = "Rowling", Nome = "J.K." },
            new Autore { Cognome = "Tolkien", Nome = "J.R.R." },
            new Autore { Cognome = "Martin", Nome = "George R.R." },
            new Autore { Cognome = "King", Nome = "Stephen" }
        };

        context.Autori.AddRange(autori);
        context.SaveChanges();

        var libri = new Libro[]
        {
            new Libro { Titolo = "Harry Potter e la Pietra Filosofale", AnnoPubblicazione = 1997 },
            new Libro { Titolo = "Il Signore degli Anelli", AnnoPubblicazione = 1954 },
            new Libro { Titolo = "Il trono di spade", AnnoPubblicazione = 1996 }
        };

        context.Libri.AddRange(libri);
        context.SaveChanges();

        // Aggiungi relazioni libro-autore
        var libroAutori = new LibroAutore[]
        {
            new LibroAutore { LibroId = 1, AutoreId = 1 },
            new LibroAutore { LibroId = 2, AutoreId = 2 },
            new LibroAutore { LibroId = 3, AutoreId = 3 }
        };

        context.LibroAutori.AddRange(libroAutori);
        context.SaveChanges();
    }
}

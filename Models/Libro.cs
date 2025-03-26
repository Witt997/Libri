using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Libri.Models
{
    public class Libro
    {
        public int Id { get; set; }
        public string Titolo { get; set; } = "";
        public int AnnoPubblicazione { get; set; }

        public ICollection<LibroAutore> Autori { get; set; } = new List<LibroAutore>();
        public ICollection<Recensione> Recensioni { get; set; } = new List<Recensione>();
    }

    // Models/Autore.cs
    public class Autore
    {
        public int Id { get; set; }
        public string Cognome { get; set; } = "";
        public string Nome { get; set; } = "";

        public ICollection<LibroAutore> Libri { get; set; } = new List<LibroAutore>();
    }

    // Models/Recensione.cs
    public class Recensione
    {
        public int Id { get; set; }
        public int Voto { get; set; }
        public DateTime Data { get; set; }

        public int LibroId { get; set; }
        [ValidateNever]
        public Libro Libro { get; set; }
    }

    // Models/LibroAutore.cs (tabella di join per relazione molti-a-molti)
    public class LibroAutore
    {
        public int LibroId { get; set; }
        public int AutoreId { get; set; }
        [ValidateNever]
        public Libro Libro { get; set; }
        [ValidateNever]
        public Autore Autore { get; set; }
    }
}

namespace Libri.Models
{
    public class Libro
    {
        public int Id { get; set; }
        public string Titolo { get; set; }
        public int AnnoPubblicazione { get; set; }

        public ICollection<LibroAutore> Autori { get; set; }
        public ICollection<Recensione> Recensioni { get; set; }
    }

    // Models/Autore.cs
    public class Autore
    {
        public int Id { get; set; }
        public string Cognome { get; set; }
        public string Nome { get; set; }

        public ICollection<LibroAutore> Libri { get; set; }
    }

    // Models/Recensione.cs
    public class Recensione
    {
        public int Id { get; set; }
        public int Voto { get; set; }
        public DateTime Data { get; set; }

        public int LibroId { get; set; }
        public Libro Libro { get; set; }
    }

    // Models/LibroAutore.cs (tabella di join per relazione molti-a-molti)
    public class LibroAutore
    {
        public int LibroId { get; set; }
        public Libro Libro { get; set; }

        public int AutoreId { get; set; }
        public Autore Autore { get; set; }
    }
}

using System.Diagnostics;
using Libri.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class LibriController : Controller
{
    private readonly ApplicationDbContext _context;

    public IActionResult Details(int id)
    {
        var libro = _context.Libri
            .Include(l => l.Autori)
            .ThenInclude(la => la.Autore)
            .Include(l => l.Recensioni)
            .FirstOrDefault(l => l.Id == id);

        if (libro == null)
        {
            return NotFound();
        }

        return View(libro);
    }

    public LibriController(ApplicationDbContext context)
    {
        _context = context;
    }

    // 1. Pagina per l'inserimento di un libro con autori
    public IActionResult Create()
    {
        ViewBag.Autori = _context.Autori.ToList() ?? new List<Autore>();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Libro libro, List<int> autoriSelezionati, string? nuovoAutoreNome, string? nuovoAutoreCognome)
    {
        if (!ModelState.IsValid)
        {
            // Log the errors to see what's going wrong
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Debug.WriteLine(error.ErrorMessage);
            }
            return View(libro); // or return the view with your model for AddRecensione
        }
        if (ModelState.IsValid)
        {
            _context.Add(libro);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(nuovoAutoreNome) && !string.IsNullOrWhiteSpace(nuovoAutoreCognome))
            {
                Autore nuovoAutore = new Autore { Nome = nuovoAutoreNome, Cognome = nuovoAutoreCognome };
                _context.Autori.Add(nuovoAutore);
                await _context.SaveChangesAsync();

                // Optionally, add the new author to the list of selected authors.
                if (autoriSelezionati == null)
                {
                    autoriSelezionati = new List<int>();
                }
                autoriSelezionati.Add(nuovoAutore.Id);
            }

            // Ensure autoriSelezionati is not null.
            autoriSelezionati ??= new List<int>();

            // Limit to a maximum of 5 authors and add relations.
            foreach (var autoreId in autoriSelezionati.Take(5))
            {
                _context.LibroAutori.Add(new LibroAutore { LibroId = libro.Id, AutoreId = autoreId });
            }
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log the exception message (or use Debug.WriteLine/ex.Message)
                Debug.WriteLine("Error saving changes: " + ex.Message);
                throw;
            }

            return RedirectToAction("Details", new { id = libro.Id });

        }
        ViewBag.Autori = _context.Autori.ToList() ?? new List<Autore>();
        return View(libro);
    }

    // 2. Inserimento recensione per un libro
    public IActionResult AddRecensione(int libroId)
    {
        var libro = _context.Libri.Find(libroId);
        if (libro == null)
        {
            return NotFound();
        }
        // Prepopulate the model with the book id
        var recensione = new Recensione { LibroId = libroId };
        ViewBag.LibroTitolo = libro.Titolo;
        return View(recensione);
    }

    [HttpPost]
    public async Task<IActionResult> AddRecensione(Recensione recensione)
    {
        if (!ModelState.IsValid)
        {
            // Log the errors to see what's going wrong
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Debug.WriteLine(error.ErrorMessage);
            }
            return View(recensione); // or return the view with your model for AddRecensione
        }
        if (ModelState.IsValid)
        {
            recensione.Data = DateTime.Now;
            _context.Add(recensione);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log the exception message (or use Debug.WriteLine/ex.Message)
                Debug.WriteLine("Error saving changes: " + ex.Message);
                throw;
            }
            return RedirectToAction(nameof(Details), new { id = recensione.LibroId });
        }
        // If there’s an error, retrieve the book title for display
        var libro = _context.Libri.Find(recensione.LibroId);
        ViewBag.LibroTitolo = libro?.Titolo;
        return View(recensione);
    }

    // 3. Elenco libri per anno
    public IActionResult ByYear(int? anno)
    {
        if (!anno.HasValue)
        {
            return View(new List<Libro>());
        }

        var libri = _context.Libri
            .Include(l => l.Autori)
            .ThenInclude(la => la.Autore)
            .Where(l => l.AnnoPubblicazione == anno.Value)
            .ToList();

        return View(libri);
    }

    // 4. Elenco autori con conteggio libri
    public IActionResult AutoriConConteggio()
    {
        var autori = _context.Autori
            .Select(a => new AutoreConteggioLibri
            {
                Autore = a,
                NumeroLibri = a.Libri.Count
            })
            .ToList();

        return View(autori);
    }

    // 5. Libri con recensioni sopra una soglia
    public IActionResult WithReviews(int minRec = 0)
    {
        var libri = _context.Libri
            .Include(l => l.Recensioni)
            .Select(l => new LibroRecensioniViewModel
            {
                Libro = l,
                NumeroRecensioni = l.Recensioni.Count,
                VotoMedio = l.Recensioni.Any() ? l.Recensioni.Average(r => r.Voto) : 0
            })
            .Where(l => l.NumeroRecensioni > minRec)
            .ToList();

        return View(libri);
    }
    public IActionResult Search()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Search(int id)
    {
        return RedirectToAction("Details", new { id });
    }
}

// ViewModels per le viste complesse
public class AutoreConteggioLibri
{
    public Autore Autore { get; set; }
    public int NumeroLibri { get; set; }
}

public class LibroRecensioniViewModel
{
    public Libro Libro { get; set; }
    public int NumeroRecensioni { get; set; }
    public double VotoMedio { get; set; }
}

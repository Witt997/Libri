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
    public async Task<IActionResult> Create(Libro libro, List<int> autoriSelezionati)
    {
        if (ModelState.IsValid)
        {
            _context.Add(libro);
            await _context.SaveChangesAsync();

            autoriSelezionati ??= new List<int>();
            {
                foreach (var autoreId in autoriSelezionati.Take(5)) // Massimo 5 autori
                {
                    _context.LibroAutori.Add(new LibroAutore { LibroId = libro.Id, AutoreId = autoreId });
                }
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
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
        ViewBag.LibroTitolo = libro.Titolo;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AddRecensione(int libroId, Recensione recensione)
    {
        if (ModelState.IsValid)
        {
            recensione.LibroId = libroId;
            recensione.Data = DateTime.Now;
            _context.Add(recensione);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = libroId });
        }
        var libro = _context.Libri.Find(libroId);
        ViewBag.LibroTitolo = libro.Titolo;
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

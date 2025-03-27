using System.Diagnostics;
using Libri.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore;

public class LibriController : Controller
{
    private readonly DataAccess _dataAccess;

    public LibriController(DataAccess dataAccess)
    {
        _dataAccess = dataAccess;
    }
    public IActionResult Details(int id)
    {
        //var libro = _dataAccess.Libri
        //    .Include(l => l.Autori)
        //    .ThenInclude(la => la.Autore)
        //    .Include(l => l.Recensioni)
        //    .FirstOrDefault(l => l.Id == id);

        //if (libro == null)
        //{
        //    return NotFound();
        //}

        //return View(libro);

        var libro = _dataAccess.GetLibroById(id);
        if (libro == null)
        {
            return NotFound();
        }
        return View(libro);
    }


    // 1. Pagina per l'inserimento di un libro con autori
    public IActionResult Create()
    {
        ViewBag.Autori = _dataAccess.GetAllAutori();
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
            int libroId = await _dataAccess.InsertLibro(libro);

            // Handle new author creation if provided.
            if (!string.IsNullOrWhiteSpace(nuovoAutoreNome) && !string.IsNullOrWhiteSpace(nuovoAutoreCognome))
            {
                Autore nuovoAutore = new Autore { Nome = nuovoAutoreNome, Cognome = nuovoAutoreCognome };
                int newAutoreId = await _dataAccess.InsertAutore(nuovoAutore);
                if (autoriSelezionati == null)
                {
                    autoriSelezionati = new List<int>();
                }
                autoriSelezionati.Add(newAutoreId);
            }

            // Ensure autoriSelezionati is not null and insert the relationships (limit to 5 authors).
            if (autoriSelezionati != null)
            {
                foreach (var autoreId in autoriSelezionati.Take(5))
                {
                    await _dataAccess.InsertLibroAutore(libroId, autoreId);
                }
            }

            //return RedirectToAction("Details", new { id = libroId });
            var updatedLibro = _dataAccess.GetLibroDetails(libroId);

            return View("Details", updatedLibro);
        }
        ModelState.Clear();
        ViewBag.Autori = _dataAccess.GetAllAutori();
            return View(libro);
    }

    // 2. Inserimento recensione per un libro
    public IActionResult AddRecensione(int libroId)
    {
        var libro = _dataAccess.GetLibroById(libroId);
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
            await _dataAccess.InsertRecensione(recensione);
            ModelState.Clear();
            //return RedirectToAction(nameof(Details), new { id = recensione.LibroId });
            var updatedLibro = _dataAccess.GetLibroDetails(recensione.LibroId);

            return View("Details", updatedLibro);
        }
        var libro = _dataAccess.GetLibroById(recensione.LibroId);
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

        var libri = _dataAccess.GetLibriByYear(anno.Value);

        return View(libri);
    }

    // 4. Elenco autori con conteggio libri
    public IActionResult AutoriConConteggio()
    {
        var autori = _dataAccess.GetAutoriConConteggio();
        return View(autori);
    }

    // 5. Libri con recensioni sopra una soglia
    public IActionResult WithReviews(int minRec = 0)
    {
        var libri = _dataAccess.GetLibriWithReviews(minRec);
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

using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Libri.Models; // Ensure this namespace contains your models (Libro, Autore, etc.)

public class DataAccess
{
    private readonly string _connectionString;

    public DataAccess(string connectionString)
    {
        _connectionString = connectionString;
    }

    // 1. Get detailed info for a book (including its authors and reviews)
    public Libro GetLibroDetails(int id)
    {
        Libro libro = null;
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            // Get the book info
            string libroQuery = "SELECT Id, Titolo, AnnoPubblicazione FROM Libri WHERE Id = @Id";
            using (var cmdLibro = new SqlCommand(libroQuery, connection))
            {
                cmdLibro.Parameters.AddWithValue("@Id", id);
                using (var reader = cmdLibro.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        libro = new Libro
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Titolo = reader["Titolo"].ToString(),
                            AnnoPubblicazione = Convert.ToInt32(reader["AnnoPubblicazione"])
                        };
                    }
                }
            }
            if (libro == null)
                return null;

            // Get associated authors (join via LibroAutori table)
            string autoriQuery = @"SELECT a.Id, a.Nome, a.Cognome
                                   FROM Autori a
                                   INNER JOIN LibroAutori la ON a.Id = la.AutoreId
                                   WHERE la.LibroId = @LibroId";
            libro.Autori = new List<LibroAutore>();
            using (var cmdAutori = new SqlCommand(autoriQuery, connection))
            {
                cmdAutori.Parameters.AddWithValue("@LibroId", id);
                using (var reader = cmdAutori.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Autore autore = new Autore
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Nome = reader["Nome"].ToString(),
                            Cognome = reader["Cognome"].ToString()
                        };
                        libro.Autori.Add(new LibroAutore
                        {
                            LibroId = libro.Id,
                            AutoreId = autore.Id,
                            Autore = autore,
                            Libro = libro
                        });
                    }
                }
            }
            // Get associated reviews
            string recensioniQuery = "SELECT Id, LibroId, Voto, Data FROM Recensioni WHERE LibroId = @LibroId";
            libro.Recensioni = new List<Recensione>();
            using (var cmdRecensioni = new SqlCommand(recensioniQuery, connection))
            {
                cmdRecensioni.Parameters.AddWithValue("@LibroId", id);
                using (var reader = cmdRecensioni.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Recensione recensione = new Recensione
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            LibroId = Convert.ToInt32(reader["LibroId"]),
                           
                            Voto = Convert.ToInt32(reader["Voto"]),
                            Data = Convert.ToDateTime(reader["Data"])
                        };
                        libro.Recensioni.Add(recensione);
                    }
                }
            }
        }
        return libro;
    }

    // 2. Retrieve all authors
    public List<Autore> GetAllAutori()
    {
        var autori = new List<Autore>();
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            string query = "SELECT Id, Nome, Cognome FROM Autori";
            using (var cmd = new SqlCommand(query, connection))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    autori.Add(new Autore
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Nome = reader["Nome"].ToString(),
                        Cognome = reader["Cognome"].ToString()
                    });
                }
            }
        }
        return autori;
    }

    // 3. Insert a new Libro and return its new Id
    public async Task<int> InsertLibro(Libro libro)
    {
        int newId;
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            string query = @"INSERT INTO Libri (Titolo, AnnoPubblicazione)
                         VALUES (@Titolo, @AnnoPubblicazione);
                         SELECT SCOPE_IDENTITY();";
            using (var cmd = new SqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@Titolo", libro.Titolo);
                cmd.Parameters.AddWithValue("@AnnoPubblicazione", libro.AnnoPubblicazione);
                newId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }
        }
        return newId;
    }

    // 4. Insert a new Autore and return its new Id
    public async Task<int> InsertAutore(Autore autore)
    {
        int newId;
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            string query = @"INSERT INTO Autori (Nome, Cognome)
                             VALUES (@Nome, @Cognome);
                             SELECT SCOPE_IDENTITY();";
            using (var cmd = new SqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@Nome", autore.Nome);
                cmd.Parameters.AddWithValue("@Cognome", autore.Cognome);
                newId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }
        }
        return newId;
    }

    // 5. Insert a relationship between Libro and Autore in LibroAutori
    public async Task InsertLibroAutore(int libroId, int autoreId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            string query = @"INSERT INTO LibroAutori (LibroId, AutoreId)
                             VALUES (@LibroId, @AutoreId)";
            using (var cmd = new SqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@LibroId", libroId);
                cmd.Parameters.AddWithValue("@AutoreId", autoreId);
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }

    // 6. Get a simple Libro (without related data)
    public Libro GetLibroById(int libroId)
    {
        Libro libro = null;
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            string query = "SELECT Id, Titolo, AnnoPubblicazione FROM Libri WHERE Id = @Id";
            using (var cmd = new SqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@Id", libroId);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        libro = new Libro
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Titolo = reader["Titolo"].ToString(),
                            AnnoPubblicazione = Convert.ToInt32(reader["AnnoPubblicazione"])
                        };
                    }
                }
            }
        }
        return libro;
    }

    // 7. Insert a new Recensione
    public async Task InsertRecensione(Recensione recensione)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            string query = @"INSERT INTO Recensioni (LibroId, Voto, Data)
                             VALUES (@LibroId, @Voto, @Data)";
            using (var cmd = new SqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@LibroId", recensione.LibroId);
                cmd.Parameters.AddWithValue("@Voto", recensione.Voto);
                cmd.Parameters.AddWithValue("@Data", recensione.Data);
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }

    // 8. Get all Libri by year
    public List<Libro> GetLibriByYear(int anno)
    {
        var libri = new List<Libro>();
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            string query = "SELECT Id, Titolo, AnnoPubblicazione FROM Libri WHERE AnnoPubblicazione = @Anno";
            using (var cmd = new SqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@Anno", anno);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        libri.Add(new Libro
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Titolo = reader["Titolo"].ToString(),
                            AnnoPubblicazione = Convert.ToInt32(reader["AnnoPubblicazione"])
                        });
                    }
                }
            }
        }
        return libri;
    }

    // 9. Get authors with the count of their books (for the AutoriConConteggio view)
    public List<AutoreConteggioLibri> GetAutoriConConteggio()
    {
        var result = new List<AutoreConteggioLibri>();
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            string query = @"SELECT a.Id, a.Nome, a.Cognome, COUNT(la.LibroId) AS NumeroLibri
                             FROM Autori a
                             LEFT JOIN LibroAutori la ON a.Id = la.AutoreId
                             GROUP BY a.Id, a.Nome, a.Cognome";
            using (var cmd = new SqlCommand(query, connection))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var autore = new Autore
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Nome = reader["Nome"].ToString(),
                        Cognome = reader["Cognome"].ToString()
                    };

                    result.Add(new AutoreConteggioLibri
                    {
                        Autore = autore,
                        NumeroLibri = Convert.ToInt32(reader["NumeroLibri"])
                    });
                }
            }
        }
        return result;
    }

    // 10. Get books with reviews above a minimum threshold (for WithReviews view)
    public List<LibroRecensioniViewModel> GetLibriWithReviews(int minRec)
    {
        var result = new List<LibroRecensioniViewModel>();
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            string query = @"SELECT l.Id, l.Titolo, l.AnnoPubblicazione,
                                    COUNT(r.Id) AS NumeroRecensioni,
                                    AVG(CAST(r.Voto AS FLOAT)) AS VotoMedio
                             FROM Libri l
                             LEFT JOIN Recensioni r ON l.Id = r.LibroId
                             GROUP BY l.Id, l.Titolo, l.AnnoPubblicazione
                             HAVING COUNT(r.Id) > @MinRec";
            using (var cmd = new SqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@MinRec", minRec);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new LibroRecensioniViewModel
                        {
                            Libro = new Libro
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Titolo = reader["Titolo"].ToString(),
                                AnnoPubblicazione = Convert.ToInt32(reader["AnnoPubblicazione"])
                            },
                            NumeroRecensioni = Convert.ToInt32(reader["NumeroRecensioni"]),
                            VotoMedio = reader["VotoMedio"] != DBNull.Value ? Convert.ToDouble(reader["VotoMedio"]) : 0
                        });
                    }
                }
            }
        }
        return result;
    }
}



using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using WordCloudCsharp;
using System.Drawing;
using System.Drawing.Imaging;
using ScottPlot;


class Articulos
{
    public string Titulo { get; set; }
    public string Autor { get; set; }
    public string Revista { get; set; }
    public string Año { get; set; }
    public string Editor { get; set; }
    public string URL { get; set; }
    public string Citado { get; set; }

    public Articulos()
    {
        Titulo = string.Empty;
        Autor = string.Empty;
        Revista = string.Empty;
        Año = string.Empty;
        Editor = string.Empty;
        URL = string.Empty;
        Citado = string.Empty;
    }
}
class Articulo
{
    public string Citado { get; set; }
    public string Año { get; set; }
    public string Titulo { get; set; }
    public string Revista { get; set; }

    public Articulo()
    {
        Citado = string.Empty;
        Año = string.Empty;
        Titulo = string.Empty;
        Revista = string.Empty;
    }
}
class Articulo2
{
    public string Titulo { get; set; }
    public string URL { get; set; }

    public Articulo2()
    {
        Titulo = string.Empty;
        URL = string.Empty;
    }
}
class Autores
{
    public string Autor { get; set; }
    public string Titulo { get; set; }

    public Autores()
    {
        Autor = string.Empty;
        Titulo = string.Empty;
    }
}

class Program
{


    static void Main()
    {
        // Cargar el DataFrame desde el archivo CSV
        using (var reader = new StreamReader("web_scraping_citas.csv"))
        using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
        {
            var records = csv.GetRecords<Articulos>().ToList();

            /*************************1******************************/
            // Ordenar el DataFrame por número de citas en orden descendente
             var dfOrdenado = records.OrderByDescending(record => double.Parse(record.Citado, CultureInfo.InvariantCulture)).ToList();

            // Seleccionar los 10 artículos con más citas
            var top10Citados = dfOrdenado.Take(10).ToList();



            // Crear una lista de objetos de la clase Articulo
            var resultado = top10Citados.Select(record => new Articulo
            {
                Citado = record.Citado,
                Año = record.Año,
                Titulo = record.Titulo,
                Revista = record.Revista
            }).ToList();


            // Guardar el resultado en un archivo CSV
            using (var writer = new StreamWriter("top_10_articulos_citados.csv"))
            using (var csvWriter = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                csvWriter.WriteRecords(resultado);
            }

            /*************************2******************************/
            // Solicitar al usuario que ingrese la palabra clave de interés
            Console.Write("Ingrese la palabra clave de interés: ");
            string palabraClave = Console.ReadLine();

            // Filtrar los artículos que contienen la palabra clave en el título
            var resultados = records.Where(record => record.Titulo.Contains(palabraClave, StringComparison.OrdinalIgnoreCase)).ToList();

            // Crear un DataFrame con los títulos y URLs de los artículos que coinciden
            var resultadosFiltrados = resultados.Select(record => new Articulo2
            {
                Titulo = record.Titulo,
                URL = record.URL
            }).ToList();

            // Guardar el resultado en un archivo CSV
            using (var writer = new StreamWriter("resultados_palabra_clave.csv"))
            using (var csvWriter = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                csvWriter.WriteRecords(resultadosFiltrados);
            }

            Console.WriteLine("Se han guardado los resultados en 'resultados_palabra_clave.csv'.");

            /*************************3******************************/
            // Extraer la lista de autores
            var autores = records
                .Where(record => !string.IsNullOrEmpty(record.Autor))
                .Select(record => record.Autor)
                .ToList();

            // Crear una lista plana de autores
            var listaAutores = autores
                .SelectMany((string autor) => autor?.Split(new string[] { " and " }, StringSplitOptions.None) ?? Enumerable.Empty<string>())
                .ToList();

            // Contar la frecuencia de cada autor
            var frecuenciaAutores = listaAutores
                .GroupBy(autor => autor)
                .Select(group => new
                {
                    Autor = group.Key,
                    Frecuencia = group.Count()
                })
                .ToList();

            // Ordenar la lista de autores por el número de veces que aparecen
            var frecuenciaAutoresOrdenada = frecuenciaAutores
                .OrderByDescending(autor => autor.Frecuencia)
                .ToList();

            // Guardar el resultado en un archivo CSV
            using (var writer = new StreamWriter("autores_frecuencia.csv"))
            using (var csvWriter = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                csvWriter.WriteRecords(frecuenciaAutoresOrdenada);
            }

            Console.WriteLine("Se ha guardado la frecuencia de los autores en 'autores_frecuencia.csv'.");

            /*************************4******************************/
            // Obtener los títulos del DataFrame
            var titulos = records.Select(record => record.Titulo).ToList();

            // Crear una lista de palabras significativas para omitir
            List<string> palabrasOmitir = new List<string>
            {
                "a", "an", "the", "in", "of", "for", "and", "on", "with", "to", "by", "at"
            };

            // Procesar los títulos y contar las palabras
            List<string> todasPalabras = new List<string>();
            foreach (var titulo in titulos)
            {
                var palabras = titulo.ToLower().Split(' ');
                foreach (var palabra in palabras)
                {
                    if (!palabrasOmitir.Contains(palabra))
                    {
                        todasPalabras.Add(palabra);
                    }
                }
            }

            // Crear un diccionario de frecuencias de palabras
            Dictionary<string, int> contadorPalabras = new Dictionary<string, int>();
            foreach (var palabra in todasPalabras)
            {
                if (contadorPalabras.ContainsKey(palabra))
                {
                    contadorPalabras[palabra]++;
                }
                else
                {
                    contadorPalabras[palabra] = 1;
                }
            }

            // Configurar el tamaño de la imagen de la nube de palabras
            var width = 800;
            var height = 400;

            // Generar la nube de palabras a partir del diccionario de frecuencias
            var wordcloud = ISingletion<WordcloudSrv>.Instance
                .GetWordCloud(width, height)
                .Draw(contadorPalabras.Keys.ToList(), contadorPalabras.Values.ToList());

            // Guardar la imagen en el disco
            wordcloud.Save("wordcloud.png", System.Drawing.Imaging.ImageFormat.Png);

            Console.WriteLine("Nube de palabras generada y guardada en 'wordcloud.png'.");

            // Obtener la cantidad de artículos por año
            var conteoAnios = records
                .GroupBy(record => record.Año)
                .Select(group => new { Anio = group.Key, Cantidad = group.Count() })
                .OrderBy(result => result.Anio)
                .ToList();

            // Crear un arreglo de años, cantidades y posiciones personalizadas
            var cantidades = conteoAnios.Select(result => (double)result.Cantidad).ToArray();
            var posiciones = conteoAnios.Select((result, index) => (double)index).ToArray();  // Posiciones personalizadas

            // Crear un objeto Plot
            var plt = new ScottPlot.Plot(600, 400);

            // Agregar un gráfico de barras al plot con posiciones personalizadas
            plt.AddBar(cantidades, posiciones);

            // Ajustar los límites del eje para eliminar el espacio por debajo del gráfico de barras
            plt.SetAxisLimits(yMin: 0);

            // Guardar la imagen en el disco
            plt.SaveFig("bar_graph.png");
            Console.WriteLine("Grafico de barras generado y guardado en 'bar_graph.png'.");

        }

    }



}

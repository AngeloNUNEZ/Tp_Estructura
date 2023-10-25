using System.Data;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Text;
using System.Text.RegularExpressions;
using BibTeXLibrary;

class Program
{
    static void Main()
    {
        // Crear un DataTable con los datos
        DataTable dataTable = new DataTable();
        dataTable.Columns.Add("Titulo");
        dataTable.Columns.Add("Autor");
        dataTable.Columns.Add("Revista");
        dataTable.Columns.Add("Año");
        dataTable.Columns.Add("Editor");
        dataTable.Columns.Add("URL");
        dataTable.Columns.Add("Citado");

        var chromeOptions = new ChromeOptions();
        chromeOptions.AddArguments("--start-maximized");
        IWebDriver driver = new ChromeDriver(chromeOptions);
        driver.Navigate().GoToUrl("https://scholar.google.com/scholar?cites=5866269323493626547&as_sdt=2005&sciodt=0,5&hl=es");

        // Esperar hasta que los resultados de la búsqueda se carguen
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(300));
        wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.Id("gs_res_ccl")));

        // Extraer todos los artículos de una página
        IList<IWebElement> articulos = driver.FindElements(By.XPath("//*[@id='gs_res_ccl']//h3"));
        int secondsPause = 1;
        

        for (int i = 0; i < articulos.Count; i++)
        {
            /**Recargar los datos de la pagina**/
            // Esperar hasta que los resultados de la búsqueda se carguen
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(300));
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.Id("gs_res_ccl")));

            // Extraer todos los artículos de una página
            articulos = driver.FindElements(By.XPath("//*[@id='gs_res_ccl']//h3"));
            /**********************************/
            Console.WriteLine(articulos[i].Text);
            int divIndex = i + 1;
            string link = "";

            try
            {
                link = new WebDriverWait(driver, TimeSpan.FromSeconds(20)).Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.XPath($@"//*[@id='gs_res_ccl_mid']/div[{divIndex}]/div[2]/div[3]/a[3]"))).GetAttribute("href");
                Console.WriteLine(link);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error al encontrar el elemento: " + e);
            }

            string xpath = $@"//*[@id='gs_res_ccl_mid']/div[{divIndex}]/div[2]/div[3]/a[3]";
            string citadoPor = "No disponible";

            try
            {
                citadoPor = articulos[i].FindElement(By.XPath(xpath)).Text;
            }
            catch (NoSuchElementException)
            {
                citadoPor = "No disponible";
            }

            int numCitas = 0;

            // Utiliza una expresión regular para extraer el número de citas
            Match match = Regex.Match(citadoPor, @"\d+");
            if (match.Success)
            {
                numCitas = int.Parse(match.Value);
            }

            Console.WriteLine("Citado por: " + numCitas);

            try
            {
                IWebElement enlaceCitar = articulos[i].FindElement(By.XPath($@"//*[@id='gs_res_ccl_mid']/div[{divIndex}]/div[2]/div[3]/a[2]"));
                Console.WriteLine("Enlace Citar: " + enlaceCitar.GetAttribute("href"));
                secondsPause = new Random().Next(10, 15);
                Thread.Sleep(TimeSpan.FromSeconds(secondsPause));
                enlaceCitar.Click();

                try
                {
                    secondsPause = new Random().Next(10, 15);
                    Thread.Sleep(TimeSpan.FromSeconds(secondsPause));
                    IWebElement enlaceBibTeX = new WebDriverWait(driver, TimeSpan.FromSeconds(10)).Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.XPath("//*[@id='gs_citi']/a[1]")));
                    Console.WriteLine("Enlace BibTeX: " + enlaceBibTeX.GetAttribute("href"));
                    enlaceBibTeX.Click();
                    secondsPause = new Random().Next(10, 15);
                    Thread.Sleep(TimeSpan.FromSeconds(secondsPause));
                    Console.WriteLine("Click en bibtex");
                    
                    try
                    {
                        // Esperar a que ciertos elementos se carguen completamente
                        wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                        wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("/html/body/pre")));
                        IWebElement bibtexElement = driver.FindElement(By.XPath("/html/body/pre"));
                        string bibtexData = bibtexElement.Text;
                        Console.WriteLine("bibtex_data: " + bibtexData);
                        // Crear un analizador BibParser a partir de la cadena de texto
                        var parser = new BibParser(new StringReader(bibtexData));
                        Console.WriteLine("Logro crear parser: "+parser);
                        // Obtener todas las entradas BibTeX (puede haber varias)
                        var entries = parser.GetAllResult();
                        Console.WriteLine("Logro crear entries: "+entries);
                        secondsPause = new Random().Next(15, 20);
                        Thread.Sleep(TimeSpan.FromSeconds(secondsPause));
                        if (entries.Count > 0)
                        {
                            Console.WriteLine("Entro al if ");
                            // Tomar la primera entrada (parsed_entries[0])
                            var entry = entries[0];

                            // Acceder a los campos BibTeX directamente
                            string title = entry.Title;
                            string author = entry["author"];
                            string journal = entry["journal"];
                            string year = entry["year"];
                            string publisher = entry["publisher"];

                            // Crear una nueva fila en el DataTable
                            DataRow newDataRow = dataTable.NewRow();
                            newDataRow["Titulo"] = title;
                            newDataRow["Autor"] = author;
                            newDataRow["Revista"] = journal;
                            newDataRow["Año"] = year;
                            newDataRow["Editor"] = publisher;
                            newDataRow["URL"] = link;
                            newDataRow["Citado"] = numCitas;

                            // Agregar la nueva fila al DataTable
                            dataTable.Rows.Add(newDataRow);

                            // Visualizar los datos
                            Console.WriteLine("Datos BibTeX:");
                            Console.WriteLine("Título: " + title);
                            Console.WriteLine("Autor: " + author);
                            Console.WriteLine("Revista: " + journal);
                            Console.WriteLine("Año: " + year);
                            Console.WriteLine("Editor: " + publisher);
                            Console.WriteLine("Salio del if ");
                        }
                        else
                        {
                            Console.WriteLine("Entro al else ");
                            // Manejar el caso en que no se encuentren datos BibTeX válidos
                            Console.WriteLine("No se encontraron datos BibTeX válidos para el artículo: " + articulos[i].Text);
                            Console.WriteLine("Salio del else ");
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error al procesar los datos BibTeX para el artículo: " + articulos[i].Text);
                        Console.WriteLine(ex);
                    }
                    // Volver atrás para el siguiente artículo
                    secondsPause = new Random().Next(15, 20);
                    Thread.Sleep(TimeSpan.FromSeconds(secondsPause));
                    driver.Navigate().GoToUrl("https://scholar.google.com/scholar?cites=5866269323493626547&as_sdt=2005&sciodt=0,5&hl=es");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("No se encontró el enlace 'BibTeX' para el artículo: ");
                    Console.WriteLine(ex);
                }
                Console.WriteLine("Salio del try bibtex parseo ");
                // Volver atrás
                secondsPause = new Random().Next(20, 35);
                Thread.Sleep(TimeSpan.FromSeconds(secondsPause));
                driver.Navigate().GoToUrl("https://scholar.google.com/scholar?cites=5866269323493626547&as_sdt=2005&sciodt=0,5&hl=es");
            }
            catch (Exception ex)
            {
                Console.WriteLine("No se encontró el enlace 'Citar' para el artículo: ");
                Console.WriteLine(ex);
            }
            Console.WriteLine("Salio del try citar ");

        }//for
        // Almacenar el DataTable en un archivo CSV
        DataTableToCsv(dataTable, "web_scraping_citas.csv");

        Thread.Sleep(TimeSpan.FromSeconds(60));

        // Cerrar el navegador
        driver.Quit();

        // Mostrar los primeros registros del DataTable
        Console.WriteLine("Datos obtenidos:");
        for (int i = 0; i < Math.Min(dataTable.Rows.Count, 5); i++)
        {
            DataRow row = dataTable.Rows[i];
            Console.WriteLine($"Título: {row["Titulo"]}, Autor: {row["Autor"]}, Revista: {row["Revista"]}, Año: {row["Año"]}, Editor: {row["Editor"]}, URL: {row["URL"]}, Citado: {row["Citado"]}");
        }

    }//main

    // Función para guardar el DataTable en un archivo CSV
    private static void DataTableToCsv(DataTable dataTable, string filePath)
    {
        StringBuilder sb = new StringBuilder();

        IEnumerable<string> columnNames = dataTable.Columns.Cast<DataColumn>().Select(column => column.ColumnName);
        sb.AppendLine(string.Join(",", columnNames));

        foreach (DataRow row in dataTable.Rows)
        {
            IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
            sb.AppendLine(string.Join(",", fields));
        }

        File.WriteAllText(filePath, sb.ToString());
    }
}

require 'selenium-webdriver'
require 'nokogiri'
require 'csv'
require 'open-uri'
require 'bibtex'

# Configurar el WebDriver
driver = Selenium::WebDriver.for :chrome
driver.navigate.to 'https://scholar.google.com/scholar?cites=5866269323493626547&as_sdt=2005&sciodt=0,5&hl=es'

wait = Selenium::WebDriver::Wait.new(:timeout => 10)
wait.until { driver.find_element(:id, 'gs_res_ccl') }

articulos = driver.find_elements(:xpath, '//*[@id="gs_res_ccl"]//h3')
data = []

articulos.each_with_index do |articulo, i|
  title = articulo.text
  div_index = i + 1

  # Obtener el enlace del artículo
  begin
    link = wait.until { articulo.find_element(:xpath, "//*[@id='gs_res_ccl_mid']/div[#{div_index}]/div[2]/div[3]/a[3]") }.attribute("href")
    puts link
  rescue => e
    puts "Error al encontrar el enlace del artículo: #{e}"
  end

  # Obtener el número de citas del artículo
  citado_por = articulo.find_element(:xpath, "//*[@id='gs_res_ccl_mid']/div[#{div_index}]/div[2]/div[3]/a[3]").text rescue "No disponible"
  num_citas = citado_por[/\d+/].to_i

  puts "Citado por: #{num_citas}"

  # Interactuar con "Citar"
  begin
    enlace_citar = articulo.find_element(:xpath, "//*[@id='gs_res_ccl_mid']/div[#{div_index}]/div[2]/div[3]/a[2]").click
    sleep rand(10..15)

    # Intentar interactuar con "BibTeX"
    begin
      sleep rand(10..15)
      enlace_bibtex = wait.until { driver.find_element(:xpath, '//*[@id="gs_citi"]/a[1]') }.click
      sleep rand(10..15)
      bibtex_data = Nokogiri::HTML(driver.page_source).at_css('pre').text

      puts "Datos BibTeX:"
      puts bibtex_data

      # Analizar el BibTeX
      bibtex_entry = BibTeX.parse(bibtex_data).first
      data_entry = {
        'Titulo' => bibtex_entry.title.to_s,
        'Autor' => bibtex_entry.author.to_s,
        'Revista' => bibtex_entry.journal.to_s,
        'Año' => bibtex_entry.year.to_s,
        'Editor' => bibtex_entry.publisher.to_s,
        'URL' => link,
        'Citado' => num_citas
      }
      data << data_entry

      sleep rand(15..20)
      driver.navigate.back
    rescue
      puts "No se encontró el enlace 'BibTeX' para el artículo: #{title}"
      data << nil
    end

    sleep rand(20..35)
    driver.navigate.back
  rescue
    puts "No se encontró el enlace 'Citar' para el artículo: #{title}"
  end
end

# Guardar los datos en CSV
CSV.open("web_scraping_citas.csv", "wb") do |csv|
  csv << ['Titulo', 'Autor', 'Revista', 'Año', 'Editor', 'URL', 'Citado']
  data.each do |row|
    csv << row.values if row
  end
end

sleep 60
driver.quit
puts "Datos obtenidos"

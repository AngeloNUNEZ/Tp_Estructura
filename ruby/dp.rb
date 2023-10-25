require 'csv'
require 'nokogiri'
require 'open-uri'


# 1 -
# Cargar el CSV en un arreglo de hashes
data = []
CSV.foreach('web_scraping_citas.csv', headers: true) do |row|
  data << row.to_h
end

# Ordenar el arreglo por el número de citas en orden descendente
data.sort_by! { |item| -item['Citado'].to_i }

# Seleccionar los 10 artículos con más citas
top_10_citados = data.first(10)

# Crear un nuevo arreglo de hashes con las columnas deseadas
resultado = top_10_citados.map do |item|
  { 'Citado' => item['Citado'], 'Año' => item['Año'], 'Titulo' => item['Titulo'], 'Revista' => item['Revista'] }
end

# Guardar el resultado en un archivo CSV
CSV.open('top_10_articulos_citados.csv', 'w') do |csv|
  csv << ['Citado', 'Año', 'Titulo', 'Revista']
  resultado.each do |item|
    csv << [item['Citado'], item['Año'], item['Titulo'], item['Revista']]
  end
end

# 2 -
# Solicitar al usuario que ingrese la palabra clave de interés
print 'Ingrese la palabra clave de interés: '
palabra_clave = gets.chomp

# Filtrar los artículos que contienen la palabra clave en el título
resultados = data.select { |item| item['Titulo'].downcase.include?(palabra_clave.downcase) }

# Crear un arreglo de hashes con los títulos y URLs de los artículos que coinciden
resultados_filtrados = resultados.map { |item| { 'Titulo' => item['Titulo'], 'URL' => item['URL'] } }

# Guardar el resultado en un archivo CSV
CSV.open('resultados_palabra_clave.csv', 'w') do |csv|
  csv << ['Titulo', 'URL']
  resultados_filtrados.each do |item|
    csv << [item['Titulo'], item['URL']]
  end
end

puts "Se han guardado los resultados en 'resultados_palabra_clave.csv'."

# 3 -
# Extraer la lista de autores
autores = data.map { |item| item['Autor'].split(' and ') }.compact.flatten

# Contar la frecuencia de cada autor
frecuencia_autores = autores.tally

# Ordenar la lista de autores por el número de veces que aparecen
frecuencia_autores = frecuencia_autores.sort_by { |autor, frecuencia| -frecuencia }

# Guardar el resultado en un archivo CSV
CSV.open('autores_frecuencia.csv', 'w') do |csv|
  csv << ['Autor', 'Frecuencia']
  frecuencia_autores.each do |autor, frecuencia|
    csv << [autor, frecuencia]
  end
end

puts "Se ha guardado la frecuencia de los autores en 'autores_frecuencia.csv'."

# 4 -
# Crear una lista de palabras significativas para omitir
palabras_omitir = ['a', 'an', 'the', 'in', 'of', 'for', 'and', 'on', 'with', 'to', 'by', 'at']

# Función para procesar los títulos y extraer palabras significativas
def procesar_titulo(titulo, palabras_omitir)
  palabras = titulo.downcase.scan(/\w+/)
  palabras_filtradas = palabras.reject { |palabra| palabras_omitir.include?(palabra) }
  palabras_filtradas
end



# Procesar los títulos y contar las palabras
todas_palabras = data.map { |item| procesar_titulo(item['Titulo'], palabras_omitir) }.flatten

contador_palabras = Hash.new(0)
todas_palabras.each { |palabra| contador_palabras[palabra] += 1 }

# Luego, para generar una nube de palabras:


# Contar la cantidad de artículos recuperados por año
conteo_anios = data.map { |item| item['Año'].to_i }.tally

# Datos para el gráfico de barras (año y cantidad de artículos)
years = conteo_anios.keys
article_count = conteo_anios.values

# Configurar el gráfico de barras


puts "Proceso completado. Se han generado gráficos y archivos CSV."
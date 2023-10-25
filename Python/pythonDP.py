import pandas as pd
import matplotlib.pyplot as plt
from wordcloud import WordCloud
from collections import Counter
import re

# Cargar el DataFrame desde el archivo CSV
df = pd.read_csv('web_scraping_citas.csv')

# Visualizar las primeras filas del DataFrame
print("Primeras 5 filas del DataFrame:")
print(df.head())
"""
1- 
"""
#Ordenar el DataFrame por número de citas en orden descendente
df_ordenado = df.sort_values(by='Citado', ascending=False)
df_ordenado.head()

# Seleccionar los 10 artículos con más citas
top_10_citados = df_ordenado.head(10)
print(top_10_citados)

# Crear un nuevo DataFrame con las columnas deseadas
resultado = top_10_citados[['Citado', 'Año', 'Titulo', 'Revista']]
print(resultado)

# Guardar el resultado en un archivo CSV
resultado.to_csv('top_10_articulos_citados.csv', index=False)

"""
2 -
"""
# Solicitar al usuario que ingrese la palabra clave de interés
palabra_clave = input("Ingrese la palabra clave de interés: ")

# Filtrar los artículos que contienen la palabra clave en el título
resultados = df[df['Titulo'].str.contains(palabra_clave, case=False, na=False, regex=True)]

# Crear un DataFrame con los títulos y URLs de los artículos que coinciden
resultados_filtrados = resultados[['Titulo', 'URL']]

# Guardar el resultado en un archivo CSV
resultados_filtrados.to_csv('resultados_palabra_clave.csv', index=False)

print("Se han guardado los resultados en 'resultados_palabra_clave.csv'.")
print(resultados_filtrados)
"""
3 - 
"""

# Extraer la lista de autores
autores = df['Autor'].str.split(' and ')

# Crear una lista plana de autores
lista_autores = [autor for autores_articulo in autores.dropna() for autor in autores_articulo]

# Contar la frecuencia de cada autor
frecuencia_autores = pd.Series(lista_autores).value_counts().reset_index()
frecuencia_autores.columns = ['Autor', 'Frecuencia']

# Ordenar la lista de autores por el número de veces que aparecen
frecuencia_autores = frecuencia_autores.sort_values(by='Frecuencia', ascending=False)

# Guardar el resultado en un archivo CSV
frecuencia_autores.to_csv('autores_frecuencia.csv', index=False)

print("Se ha guardado la frecuencia de los autores en 'autores_frecuencia.csv'.")
print(frecuencia_autores)

"""
4 - 
"""
titulos = df['Titulo']

# Crear una lista de palabras significativas para omitir
palabras_omitir = ['a', 'an', 'the', 'in', 'of', 'for', 'and', 'on', 'with', 'to', 'by', 'at']

# Función para procesar los títulos y extraer palabras significativas
def procesar_titulo(titulo):
    palabras = re.findall(r'\w+', titulo.lower())
    palabras_filtradas = [palabra for palabra in palabras if palabra not in palabras_omitir]
    return palabras_filtradas

# Procesar los títulos y contar las palabras
todas_palabras = [palabra for titulo in titulos for palabra in procesar_titulo(titulo)]
contador_palabras = Counter(todas_palabras)

# Crear un gráfico de nube de palabras
wordcloud = WordCloud(width=800, height=400, background_color='white').generate_from_frequencies(contador_palabras)
plt.figure(figsize=(10, 5))
plt.imshow(wordcloud, interpolation='bilinear')
plt.axis('off')
plt.title('Nube de Palabras en Títulos de Artículos')
plt.show()

# Contar la cantidad de artículos recuperados por año
conteo_anios = df['Año'].value_counts().sort_index()

# Crear un gráfico de barras para la cantidad de artículos por año
plt.figure(figsize=(10, 5))
conteo_anios.plot(kind='bar')
plt.title('Cantidad de Artículos por Año de Publicación')
plt.xlabel('Año')
plt.ylabel('Cantidad de Artículos')
plt.xticks(rotation=45)
plt.show()

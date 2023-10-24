import pandas as pd
import matplotlib.pyplot as plt
from wordcloud import WordCloud
from collections import Counter
import re
df = pd.read_csv('web_scraping_citas.csv')

df_ordenado = df.sort_values(by='Citado', ascending=False)
df_ordenado.head()
top_10_citados = df_ordenado.head(10)
top_10_citados
resultado = top_10_citados[['Citado', 'Año', 'Titulo', 'Revista']]
resultado
resultado.to_csv('top_10_articulos_citados.csv', index=False)
palabra_clave = input("Ingrese la palabra clave de interés: ")
resultados = df[df['Titulo'].str.contains(palabra_clave, case=False, na=False, regex=True)]
resultados_filtrados = resultados[['Titulo', 'URL']]
resultados_filtrados.to_csv('resultados_palabra_clave.csv', index=False)
resultados_filtrados
autores = df['Autor'].str.split(' and ')
lista_autores = [autor for autores_articulo in autores.dropna() for autor in autores_articulo]
frecuencia_autores = pd.Series(lista_autores).value_counts().reset_index()
frecuencia_autores.columns = ['Autor', 'Frecuencia']

frecuencia_autores = frecuencia_autores.sort_values(by='Frecuencia', ascending=False)

frecuencia_autores.to_csv('autores_frecuencia.csv', index=False)

frecuencia_autores

titulos = df['Titulo']
palabras_omitir = ['a', 'an', 'the', 'in', 'of', 'for', 'and', 'on', 'with', 'to', 'by', 'at']

def procesar_titulo(titulo):
    palabras = re.findall(r'\w+', titulo.lower())
    palabras_filtradas = [palabra for palabra in palabras if palabra not in palabras_omitir]
    return palabras_filtradas

todas_palabras = [palabra for titulo in titulos for palabra in procesar_titulo(titulo)]
contador_palabras = Counter(todas_palabras)
wordcloud = WordCloud(width=800, height=400, background_color='white').generate_from_frequencies(contador_palabras)
plt.figure(figsize=(10, 5))
plt.imshow(wordcloud, interpolation='bilinear')
plt.axis('off')
plt.title('Nube de Palabras en Títulos de Artículos')
plt.show()
conteo_anios = df['Año'].value_counts().sort_index()
plt.figure(figsize=(10, 5))
conteo_anios.plot(kind='bar')
plt.title('Cantidad de Artículos por Año de Publicación')
plt.xlabel('Año')
plt.ylabel('Cantidad de Artículos')
plt.xticks(rotation=45)
plt.show()
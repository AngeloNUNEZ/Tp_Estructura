import random
import re
import pandas as pd

from bibtexparser import loads
from time import sleep
from selenium import webdriver
from selenium.common import NoSuchElementException
from selenium.webdriver.common.by import By
from selenium.webdriver.common.keys import Keys
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from bs4 import BeautifulSoup


# Crear un DataFrame con los datos
df = pd.DataFrame({
    'Titulo': [],  # Crea columnas vacías para los campos
    'Autor': [],
    'Revista': [],
    'Año': [],
    'Editor': [],
    'URL' : [],
    'Citado' : []
})

driver = webdriver.Chrome()
driver.get('https://scholar.google.com/scholar?cites=5866269323493626547&as_sdt=2005&sciodt=0,5&hl=es')

# Esperar hasta que los resultados de la búsqueda se carguen
wait = WebDriverWait(driver, 10)
wait.until(EC.presence_of_element_located((By.ID, 'gs_res_ccl')))

#Extrae todos los articulos de una página
articulos = driver.find_elements(By.XPATH, '//*[@id="gs_res_ccl"]//h3')
secondsPause = 1
data = []


for i in range(len(articulos)):
    title = articulos[i].text
    div_index = i + 1
    try:
        link = WebDriverWait(driver, 20).until(EC.presence_of_element_located((By.XPATH, f'//*[@id="gs_res_ccl_mid"]/div[{div_index}]/div[2]/div[3]/a[3]'))).get_attribute("href")
        print(link)
    except Exception as e:
        print("Error al encontrar el elemento:", str(e))


    xpath = f'//*[@id="gs_res_ccl_mid"]/div[{div_index}]/div[2]/div[3]/a[3]'
    try:
        citadoPor = articulos[i].find_element(By.XPATH, xpath).text
    except NoSuchElementException:
        citadoPor = "No disponible"

    # Utiliza una expresión regular para extraer el número de citas
    match = re.search(r'(\d+)', citadoPor)
    if match:
        num_citas = int(match.group(1))
    else:
        num_citas = 0  # Establece 0 si no se encuentra ningún número

    print("Citado por:", num_citas)

    # Interactuar con "Citar"
    try:
        enlaceCitar = articulos[i].find_element(By.XPATH, f'//*[@id="gs_res_ccl_mid"]/div[{div_index}]/div[2]/div[3]/a[2]')
        print("Enlace Citar:", enlaceCitar.get_attribute("href"))
        secondsPause = random.randrange(10, 15)
        sleep(secondsPause)
        enlaceCitar.click()

        # Intentar interactuar con "BibTeX"
        try:
            secondsPause = random.randrange(10, 15)
            sleep(secondsPause)
            enlaceBibTeX = WebDriverWait(driver, 10).until(EC.element_to_be_clickable((By.XPATH, '//*[@id="gs_citi"]/a[1]')))
            print("Enlace BibTeX:", enlaceBibTeX.get_attribute("href"))
            enlaceBibTeX.click()
            secondsPause = random.randrange(10, 15)
            sleep(secondsPause)

            # Guardar los datos de la página en la variable bibtex_data
            bibtex_data = driver.page_source

            # Utilizamos BeautifulSoup para extraer los datos BibTeX
            soup = BeautifulSoup(bibtex_data, 'html.parser')
            pre_element = soup.find('pre')
            if pre_element:
                bibtex_data = pre_element.get_text()
                print("Datos BibTeX:")
                print(bibtex_data)
                data.append(bibtex_data)

            try:
                # Analizar el BibTeX y acceder a los campos
                parsed_entry = loads(bibtex_data)
                if parsed_entry.entries:
                    entry = parsed_entry.entries[0]
                    # Crear un nuevo DataFrame con los datos del artículo
                    new_data = pd.DataFrame({
                        'Titulo': [entry.get('title')],
                        'Autor': [entry.get('author')],
                        'Revista': [entry.get('journal')],
                        'Año': [entry.get('year')],
                        'Editor': [entry.get('publisher')],
                        'URL': [link],
                        'Citado': [num_citas]
                    })

                    # Agregar el nuevo DataFrame al DataFrame principal
                    df = pd.concat([df, new_data], ignore_index=True)
                else:
                    # Manejar el caso en que no se encuentra ningún dato BibTeX válido
                    print("No se encontraron datos BibTeX válidos para el artículo:", title)
                    data.append(None)
            except Exception as e:
                print("Error al procesar los datos BibTeX para el artículo:", title)
                print(e)
                data.append(None)

            # Volver atras para el siguiente articulo
            driver.back()
            secondsPause = random.randrange(15, 20)
            sleep(secondsPause)

        except Exception as e:
            print("No se encontró el enlace 'BibTeX' para el artículo:", title)
            data.append(None)  # Almacenar None cuando hay una excepción

        #Volver atras
        driver.back()
        secondsPause = random.randrange(20, 35)
        sleep(secondsPause)
    except Exception as e:
        print("No se encontró el enlace 'Citar' para el artículo:", title)
        print(e)



# Almacenar el DataFrame en un archivo CSV
df.to_csv('web_scraping_citas.csv', index=False)
sleep(60)
driver.quit()
print("Datos obtenidos: ",df.head())

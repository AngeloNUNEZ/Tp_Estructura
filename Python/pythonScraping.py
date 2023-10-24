import re
import pandas as pd
from bibtexparser import loads
from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from bs4 import BeautifulSoup

def initialize_data():
    return pd.DataFrame({
        'Titulo': [],
        'Autor': [],
        'Revista': [],
        'Año': [],
        'Editor': [],
        'URL': [],
        'Citado': []
    })

def extract_link_and_citations(driver, idx):
    try:
        element_link = WebDriverWait(driver, 5).until(EC.presence_of_element_located(
            (By.XPATH, f'//*[@id="gs_res_ccl_mid"]/div[{idx}]/div[2]/div[3]/a[3]')))
        link = element_link.get_attribute("href")
        cited_by_text = element_link.text

        match = re.search(r'(\d+)', cited_by_text)
        citations = int(match.group(1)) if match else 0
    except Exception as e:
        link, citations = None, 0

    return link, citations

def fetch_and_store_bibtex(driver, idx, link, citations, data_container):
    try:
        cite_elem = driver.find_element(By.XPATH, f'//*[@id="gs_res_ccl_mid"]/div[{idx}]/div[2]/div[3]/a[2]')
        cite_elem.click()

        bibtex_elem = WebDriverWait(driver, 5).until(EC.element_to_be_clickable((By.XPATH, '//*[@id="gs_citi"]/a[1]')))
        bibtex_elem.click()

        page_data = driver.page_source
        soup = BeautifulSoup(page_data, 'html.parser')
        bibtex_text = soup.find('pre').get_text() if soup.find('pre') else None

        parsed_bibtex = loads(bibtex_text)
        if parsed_bibtex.entries:
            entry = parsed_bibtex.entries[0]
            new_row = {
                'Titulo': [entry.get('title')],
                'Autor': [entry.get('author')],
                'Revista': [entry.get('journal')],
                'Año': [entry.get('year')],
                'Editor': [entry.get('publisher')],
                'URL': [link],
                'Citado': [citations]
            }
            data_container = pd.concat([data_container, pd.DataFrame(new_row)], ignore_index=True)
        driver.back()
    except Exception as e:
        pass

    return data_container

def web_scraping():
    driver = webdriver.Chrome()
    driver.get('https://scholar.google.com/scholar?cites=5866269323493626547&as_sdt=2005&sciodt=0,5&hl=es')
    WebDriverWait(driver, 5).until(EC.presence_of_element_located((By.ID, 'gs_res_ccl')))

    articles = driver.find_elements(By.XPATH, '//*[@id="gs_res_ccl"]//h3')
    scraped_data = initialize_data()

    for i in range(1, len(articles) + 1):
        link, citations = extract_link_and_citations(driver, i)
        if link:
            scraped_data = fetch_and_store_bibtex(driver, i, link, citations, scraped_data)

    driver.quit()
    scraped_data.to_csv('web_scraping_results.csv', index=False)
    print("Datos obtenidos:\n", scraped_data.head())

if __name__ == "__main__":
    web_scraping()

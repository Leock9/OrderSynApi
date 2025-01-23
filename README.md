# OrderSyncApi

A OrderSyncApi é uma API construída em .NET 8 para sincronização e processamento de pedidos. Ela utiliza o Redis para armazenar informações, como arquivos enviados, e suporta execução local com contêineres Docker.

[![.NET 8 Build, Docker Build](https://github.com/Leock9/OrderSynApi/actions/workflows/main.yml/badge.svg)](https://github.com/Leock9/OrderSynApi/actions/workflows/main.yml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Leock9_OrderSynApi&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=Leock9_OrderSynApi)

## **Sumário**
- [Visão Geral](#visão-geral)
- [Tecnologias Utilizadas](#tecnologias-utilizadas)
- [Como Executar](#como-executar)
    - [Rodando via Docker Compose](#rodando-via-docker-compose)
- [DockerHub]()
- [Testes de Estresse](#testes-de-estresse)
---

## **Visão Geral**

### Recursos Principais da API
- Endpoint processamento de arquivos
- Endpoint com filtros opcionais (ID de pedido, intervalo de datas).
- Validação de dados com regras personalizadas usando FluentValidation.
- Armazenamento de arquivos e seus conteúdos no Redis.
- Construção modular com FastEndpoints para endpoints rápidos e organizados.

### Estratégias adotadas

- Clean Architecture: Alta manutenibilidade e separação de responsabilidades.
- Redis como Cache: Acesso rápido aos dados e alta escalabilidade.
- FluentValidation: Regras de validação reutilizáveis e centralizadas.
- FastEndpoints: Redução de boilerplate e endpoints limpos.
- Paralelismo: Processamento eficiente de grandes volumes de dados.
- Rate Limiting: Proteção contra ataques de negação de serviço.
- Containerização: Ambiente de desenvolvimento e produção consistente.

### Débitos Técnicos na OrderSyncApi
- Testes de Integração: Testes automatizados para garantir a integridade da aplicação.
- Autenticação e Autorização: Implementação de autenticação JWT para proteger os endpoints.
- Colisão Chaves Redis: Implementação de um sistema de geração de chaves único para evitar colisões.
- Processo assincrono, justificado com testes do k6.
---

## **Tecnologias Utilizadas**

Certifique-se de que as seguintes ferramentas estejam instaladas:
- [.NET 8](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/)
- [Docker Compose](https://docs.docker.com/compose/)
---

## **Como Executar**
### **Rodando via Docker Compose**

1. Clone o repositório:
   ```bash
   git clone https://github.com/Leock9/OrderSynApi
   cd OrderSynApi

2. Execute o comando abaixo para rodar a aplicação:
   ```bash
   docker-compose up -d
   ```
3. Importe a collection para seu postaman e faça as requisições necessárias.
   ```bash
   OrderSync.postman_collection.json

4. Acesse a documentação da API através do Swagger:
   ```bash
    http://localhost:8080/swagger

5. Para acessar o UI do Redis:
   ```bash
    http://localhost:8081

## **DockerHub**
A imagem da aplicação está disponível no DockerHub, para baixar a imagem execute o comando abaixo:
```bash
docker pull lkhouri/ordersyncapi
```
## **Testes de Estresse K6**

Os testes de estresse foram realizados com o K6, junto ao docker-compose

# Relatório de Análise de Desempenho e Justificativa para Implementação de Processo Assíncrono

## Objetivo
Este relatório tem como objetivo avaliar o desempenho de dois endpoints principais: **File Sync Endpoint** e **Get File Endpoint**, analisando o tempo de resposta, latência, taxa de sucesso das requisições, e considerando a necessidade de um processo assíncrono para melhorar a escalabilidade e o desempenho do sistema.

---

## Sumário de Resultados

| **Métrica**                     | **Valor**            | **Comentários**                                                             |
|----------------------------------|----------------------|-----------------------------------------------------------------------------|
| **Duração total do teste**       | 1 minuto e 33 segundos | Teste de carga realizado por um tempo suficiente para avaliar o desempenho. |
| **Usuários virtuais (VUs)**      | 100% (máximo de 100)  | Teste com a capacidade máxima de usuários virtuais, simulando alta carga.   |
| **Total de requisições HTTP**    | 3495                 | Número total de requisições realizadas durante o teste.                    |
| **Taxa de requisições**          | 37.38 requisições/s   | Taxa de requisições estável e consistente.                                  |
| **Taxa de iterações**            | 12.46 iterações/s     | Número adequado de iterações por segundo.                                   |
| **Taxa de sucesso das requisições** | 100% (sem falhas)    | Nenhuma falha nas requisições durante o teste.                               |
| **Tempo médio de resposta**      | 5.92ms               | Respostas rápidas e consistentes.                                           |
| **Latência média de espera**     | 4.78ms               | Latência eficiente e sem picos elevados.                                     |
| **Tempo médio de bloqueio**      | 31.78µs              | Tempo de bloqueio baixo, indicando baixa sobrecarga no servidor.            |
| **Duração média das iterações**  | 4.03 segundos        | Iterações realizadas de forma eficiente dentro do tempo esperado.           |

---

## Desempenho de Endpoint - File Sync

| **Métrica**                     | **Valor**            | **Comentários**                                                             |
|----------------------------------|----------------------|-----------------------------------------------------------------------------|
| **Check de Status 200**          | 100%                | O endpoint respondeu com status 200, sem falhas nas respostas HTTP.         |
| **Check de Resposta Sucessiva**  | 0% (Falhou em 1160 casos) | Embora o status 200 tenha sido retornado, a resposta não foi considerada bem-sucedida em 1160 requisições, indicando falha na confirmação de resposta. |
| **Total de Requisições**         | 3495                 | Número significativo de requisições realizadas, que pode ter causado falhas nas respostas. |
| **Taxa de Falhas de Resposta**   | 33.24%               | Uma taxa significativa de falhas nas respostas, indicando problemas de processamento no backend. |
| **Rate Limit (Limitação de Taxa)** | 100 requisições/s   | O endpoint **File Sync** possui uma limitação de taxa de 100 requisições por segundo, o que pode ter contribuído para o aumento das falhas em situações de carga elevada. |

---

## Desempenho de Endpoint - Get File

| **Métrica**                     | **Valor**            | **Comentários**                                                             |
|----------------------------------|----------------------|-----------------------------------------------------------------------------|
| **Check de Status 200**          | 100%                | O endpoint retornou status 200 em todas as requisições.                     |
| **Check de Conteúdo (getFileOutput)** | 100%              | Respostas com o conteúdo esperado em todas as requisições.                   |
| **Check de Conteúdo (users)**    | 100%                | Respostas contendo o campo "users" corretamente.                            |
| **Taxa de Sucesso**              | 87.55%              | A maior parte das requisições foi bem-sucedida, mas houve falhas em 12.45% das requisições, indicando possíveis gargalos sob carga elevada. |

---

## Razonamento para Implementação de Processo Assíncrono

A análise das métricas de desempenho indica a necessidade de um processo assíncrono para garantir maior escalabilidade e melhorar a latência durante picos de carga. Abaixo estão as razões que justificam essa necessidade:

1. **Falhas nas Respostas (File Sync Endpoint):**
   - **Falhas nas respostas**: O **File Sync Endpoint** apresentou falhas significativas em 1160 casos, apesar de retornar o status 200. Isso sugere que o processamento da resposta no backend falhou ou foi interrompido, provavelmente devido à alta carga de requisições e ao tempo de processamento elevado.

2. **Taxa de Falhas (File Sync Endpoint):**
   - A **taxa de falhas de resposta** no **File Sync Endpoint** foi de 33.24%, o que é alarmante para um sistema que deve ser robusto sob carga. A implementação de um processo assíncrono ajudaria a distribuir melhor a carga de trabalho e permitiria que o sistema lidasse com mais requisições simultâneas.

3. **Rate Limit (Limitação de Taxa):**
   - O **File Sync Endpoint** possui um **rate limit de 100 requisições por segundo**. Sob carga, esse limite pode ser um fator limitante, o que pode ter causado falhas nas respostas durante o teste. Com a implementação de um processo assíncrono, seria possível evitar o bloqueio do processamento enquanto aguarda a resposta de processos longos, como a sincronização de arquivos.

4. **Tempo de Resposta e Latência:**
   - Embora o **tempo médio de resposta** de 5.92ms e a **latência média** de 4.78ms sejam satisfatórios, a **duração média das iterações** (4.03 segundos) indica que, sob carga elevada, a performance pode ser afetada. O uso de processos assíncronos permitirá que o sistema libere recursos durante o tempo de espera, resultando em um melhor tempo de resposta geral para o sistema.

5. **Escalabilidade e Concurrency:**
   - A implementação de processos assíncronos ajudará a melhorar a escalabilidade do sistema, permitindo que mais requisições sejam processadas simultaneamente sem causar bloqueios no fluxo geral. Essa abordagem é necessária para lidar com o crescimento do número de requisições e usuários simultâneos no futuro.

---

## Conclusão

Com base na análise das métricas de desempenho, a **implementação de um processo assíncrono** é essencial para garantir a escalabilidade do sistema e melhorar o tempo de resposta, especialmente sob alta carga de requisições. O **File Sync Endpoint** apresentou falhas significativas devido à limitação de taxa (rate limit) e à carga excessiva, enquanto o **Get File Endpoint** teve um desempenho mais estável, mas ainda assim demonstrou falhas em algumas requisições.

A adoção de um processo assíncrono permitirá que o sistema gerencie melhor os recursos, melhore a taxa de sucesso e minimize o impacto das limitações de taxa, proporcionando uma experiência mais eficiente e escalável para os usuários.


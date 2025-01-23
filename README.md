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
- Processamento de arquivos com filtros opcionais (ID de pedido, intervalo de datas).
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
- Ver nescessidade de processo assincrono.
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
    http://localhost:5000/swagger

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

Endpoint de teste:

## Get File

### Estratégia de Carga do Teste

### 1. VUs (Usuários Virtuais):
- O teste foi executado com **2 usuários virtuais** inicialmente.
- O número máximo de **100 usuários virtuais** foi configurado, escalando a carga para até 100 VUs.

### 2. Taxa de Requisições:
- Durante a execução, a carga foi ajustada para realizar aproximadamente **40.79 requisições por segundo (RPS)**.

### 3. Duração:
- O teste foi executado por **2 minutos (2m00.4s)**.

### 4. Iterações:
- Foram realizadas **4911 iterações**, com cada VU executando um número de requisições durante o teste.

### Pontos a Considerar:
- **Pico de Usuários Virtuais**: O teste atingiu um pico de **100 usuários virtuais**, indicando uma carga considerável.
- **Porcentagem de Falhas**: A falha foi **0%**, com todas as requisições completando com sucesso.
- **Distribuição da Duração das Requisições**: As requisições duraram entre 2.93ms e 279.32ms, com uma média de **6.17ms**, sugerindo um comportamento de latência relativamente baixo.

### Conclusão:
A estratégia de carga adotada envolveu um teste de stress, com aumento gradual da carga até atingir 100 usuários virtuais, realizando 40 a 63 requisições por segundo. O tempo de execução foi de aproximadamente 2 minutos, com um alto nível de sucesso nas requisições e sem falhas.


| **Métrica**                                 | **Valor**                                                        |
| ------------------------------------------- | ---------------------------------------------------------------- |
| **Status HTTP**                             | 200 (OK)                                                         |
| **Contém getFileOutput**                    | Sim                                                              |
| **Contém usuários**                         | Sim                                                              |
| **Taxa de sucesso das verificações**        | 100,00% (14733 de 14733)                                         |
| **Dados recebidos**                         | 720 MB (6.0 MB/s)                                                |
| **Dados enviados**                          | 506 kB (4.2 kB/s)                                                |
| **Duração do Grupo de Testes**              | Média: 1,01s, Mínimo: 1s, Máximo: 1,45s                          |
| **Tempo de Bloqueio da Requisição HTTP**    | Média: 75,97µs, Máximo: 263,5ms                                  |
| **Tempo de Conexão HTTP**                   | Média: 67,55µs, Máximo: 263,41ms                                 |
| **Duração da Requisição HTTP**              | Média: 6,17ms, Máximo: 279,32ms                                  |
| **Falhas nas Requisições HTTP**             | 0,00% (0 de 4911 falharam)                                       |
| **Tempo de Recebimento de Requisição HTTP** | Média: 1,44ms, Máximo: 135,11ms                                  |
| **Tempo de Envio de Requisição HTTP**       | Média: 27,22µs, Máximo: 10,09ms                                  |
| **Tempo de Handshake TLS**                  | 0s                                                               |
| **Tempo de Espera na Requisição HTTP**      | Média: 4,7ms, Máximo: 279,2ms                                    |
| **Total de requisições HTTP**               | 4911                                                             |
| **Taxa de requisições HTTP**                | 40,79 requisições/s                                              |
| **Duração da Iteração**                     | Média: 1,01s, Mínimo: 1s, Máximo: 1,45s                          |
| **Total de iterações**                      | 4911                                                             |
| **Taxa de iterações**                       | 40,79 iterações/s                                                |
| **VUs (usuários virtuais)**                 | Mínimo: 1, Máximo: 100                                           |
| **Máximo de VUs**                           | 100                                                              |
| **Status Final**                            | Executando por 2m00.4s, 4911 iterações completas, 0 interrupções |


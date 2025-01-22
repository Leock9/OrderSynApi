# OrderSyncApi

A OrderSyncApi é uma API construída em .NET 8 para sincronização e processamento de pedidos. Ela utiliza o Redis para armazenar informações, como arquivos enviados, e suporta execução local com contêineres Docker.

[![.NET 8 Build, Docker Build](https://github.com/Leock9/OrderSynApi/actions/workflows/main.yml/badge.svg)](https://github.com/Leock9/OrderSynApi/actions/workflows/main.yml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Leock9_OrderSynApi&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=Leock9_OrderSynApi)

## **Sumário**
- [Visão Geral](#visão-geral)
- [Tecnologias Utilizadas](#tecnologias-utilizadas)
- [Como Executar](#como-executar)
    - [Rodando via Docker Compose](#rodando-via-docker-compose)
- [DockerHub](https://hub.docker.com/r/lkhouri/calcapi)
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
- Endpoint infringindo o princípio da responsabilidade única: Separação do endpoint de processamento de arquivos e consulta de dados com filtros.
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

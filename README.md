# CRUD Simples em .NET + SQLite

Projeto de estudo para entender, na prática, **como funciona um CRUD** e **como uma aplicação .NET se conecta a um banco de dados**.

> Segurança, validações utilizando TryPase e interface gráfica ficaram de fora de propósito. O foco aqui são os **fundamentos** utilizando CLI.

---

## Sumário

1. [Objetivo](#1-objetivo)
2. [Como executar](#2-como-executar)
3. [Estrutura do projeto](#3-estrutura-do-projeto)
4. [O que é um CRUD](#4-o-que-é-um-crud)
5. [Banco de dados e SQLite](#5-banco-de-dados-e-sqlite)
6. [Conexão com o banco (ADO.NET)](#6-conexão-com-o-banco-adonet)
7. [Executando comandos SQL](#7-executando-comandos-sql)
8. [Linguagem SQL aprendida](#8-linguagem-sql-aprendida)
9. [Fundamentos de C# aplicados](#9-fundamentos-de-c-aplicados)
10. [Organização e arquitetura](#10-organização-e-arquitetura)
11. [Ferramentas do ecossistema .NET](#11-ferramentas-do-ecossistema-net)
12. [Boas práticas já aplicadas](#12-boas-práticas-já-aplicadas)
13. [O que ficou de fora](#13-o-que-ficou-de-fora)
14. [Exercícios para praticar](#14-exercícios-para-praticar)
15. [Próximos passos](#15-próximos-passos)
16. [Resumo em uma tabela](#16-resumo-em-uma-tabela)

---

## 1. Objetivo

Aprender, sem frameworks, o caminho completo de um dado, utilizando apenas CLI:

```
Usuário digita  →  C# monta o comando  →  Driver SQLite  →  Arquivo loja.db
                                                              ↓
Usuário vê o resultado  ←  C# lê o resultado  ←  Driver SQLite
```

---

## 2. Como executar

Pré-requisito: [.NET SDK](https://dotnet.microsoft.com/download) 8 ou superior.

```bash
cd CrudSqlite
dotnet run
```

- Na primeira execução o arquivo `loja.db` é criado automaticamente.
- Se tiver outra versão do SDK instalada, ajuste `<TargetFramework>` no `CrudSqlite.csproj` (ex.: `net9.0`, `net10.0`).
- Para inspecionar o banco por fora, abra o `loja.db` no **DB Browser for SQLite**.

---

## 3. Estrutura do projeto

```
CrudSqlite/
├── CrudSqlite.csproj          → configuração do projeto e dependências
├── Program.cs                 → ponto de entrada + menu do console
├── Models/
│   └── Produto.cs             → classe que representa uma linha da tabela
└── Data/
    └── ProdutoRepository.cs   → todo o acesso ao banco de dados
```

**Aprendizado:** cada arquivo tem **uma responsabilidade**. Quem desenha o menu não precisa saber escrever SQL, e quem escreve SQL não precisa saber o que é um `Console.WriteLine`.

---

## 4. O que é um CRUD

CRUD é a sigla das **quatro operações básicas** sobre qualquer dado persistido.

| Letra | Operação | Comando SQL | Método no projeto |
|-------|----------|-------------|-------------------|
| **C** | Create (criar)    | `INSERT` | `Inserir()` |
| **R** | Read (ler)        | `SELECT` | `ListarTodos()`, `BuscarPorId()` |
| **U** | Update (atualizar)| `UPDATE` | `Atualizar()` |
| **D** | Delete (excluir)  | `DELETE` | `Excluir()` |

**Aprendizado:** praticamente todo sistema (rede social, e-commerce, ERP) é, no fundo, um conjunto de CRUDs com regras de negócio por cima. Em APIs web, esses quatro verbos correspondem aos métodos HTTP `POST`, `GET`, `PUT/PATCH` e `DELETE`.

---

## 5. Banco de dados e SQLite

### Conceitos de banco relacional
- **Banco de dados**: lugar onde os dados ficam guardados de forma **persistente** (sobrevivem ao fechar o programa).
- **Tabela**: estrutura com colunas fixas (ex.: `Produtos`).
- **Linha (registro)**: um item da tabela (ex.: um produto).
- **Coluna (campo)**: um atributo (ex.: `Nome`, `Preco`).
- **Chave primária (`PRIMARY KEY`)**: identificador único de cada linha (`Id`).
- **`AUTOINCREMENT`**: o próprio banco gera o `Id` sequencial, e por isso não informamos o `Id` no `INSERT`.
- **`NOT NULL`**: impede que a coluna fique vazia.

### Por que SQLite para estudar?
- É um banco **embutido**: não precisa instalar nem configurar servidor.
- Todo o banco é **um único arquivo** (`loja.db`).
- Usa SQL padrão, então o conhecimento é reaproveitável em SQL Server, PostgreSQL e MySQL.

### Tipos de dados no SQLite
O SQLite tem poucos tipos nativos (`INTEGER`, `TEXT`, `REAL`, `BLOB`). Não existe um tipo `DECIMAL` verdadeiro, e por isso o `Preco` foi declarado como `TEXT` e o driver converte para `decimal` em C#.

**Aprendizado:** cada banco tem suas particularidades de tipos, e é preciso conhecer o mapeamento entre o tipo do C# e o tipo do banco. `decimal` é o tipo correto para dinheiro em C#, pois `double` e `float` têm erros de arredondamento.

---

## 6. Conexão com o banco (ADO.NET)

O **ADO.NET** é a base de acesso a dados do .NET. Frameworks como o Entity Framework são construídos em cima dele.

### Connection string
```csharp
private const string ConnectionString = "Data Source=loja.db";
```
Texto que diz **onde** e **como** conectar. No SQLite basta o caminho do arquivo. Em bancos como SQL Server ou PostgreSQL ela inclui servidor, porta, usuário e senha.

### O ciclo de vida de uma conexão
```csharp
using var conexao = new SqliteConnection(ConnectionString);  // 1. cria
conexao.Open();                                              // 2. abre
// ... executa comandos ...                                  // 3. usa
// fim do método → Dispose() fecha a conexão automaticamente // 4. fecha
```

**Aprendizados:**
- Conexão é um **recurso limitado**: abre-se, usa-se e fecha-se o mais rápido possível.
- `using var` garante o fechamento **mesmo se der erro** no meio do caminho. É o equivalente a um `try/finally` automático.
- Esquecer de fechar conexões causa vazamento de recursos (*resource leak*).

---

## 7. Executando comandos SQL

### Os objetos principais

| Objeto | Função |
|--------|--------|
| `SqliteConnection` | A "ponte" até o banco |
| `SqliteCommand`    | O comando SQL a ser executado |
| `SqliteParameter`  | Valor que entra no SQL de forma segura |
| `SqliteDataReader` | Lê, linha a linha, o resultado de um `SELECT` |

### Qual método de execução usar?

| Método | Quando usar | Retorno | Usado em |
|--------|-------------|---------|----------|
| `ExecuteNonQuery()` | Comandos que **alteram** dados ou estrutura | Nº de linhas afetadas | `CREATE TABLE`, `INSERT`, `UPDATE`, `DELETE` |
| `ExecuteReader()`   | Comandos que **devolvem** linhas | `DataReader` | `SELECT` |
| `ExecuteScalar()`   | Quando se espera **um único valor** (não usado aqui) | `object` | `SELECT COUNT(*)`, por exemplo |

### Lendo o resultado de um SELECT
```csharp
using var leitor = comando.ExecuteReader();
while (leitor.Read())            // avança para a próxima linha; false quando acabam
{
    var id   = leitor.GetInt32(0);   // 0 = posição da coluna no SELECT
    var nome = leitor.GetString(1);
}
```
**Aprendizado:** a posição dos `Get...(n)` depende da **ordem das colunas no SELECT**. Por isso listamos as colunas explicitamente em vez de usar `SELECT *`.

### Parâmetros: nunca concatenar valores no SQL
```csharp
// Correto
comando.CommandText = "SELECT * FROM Produtos WHERE Id = $id;";
comando.Parameters.AddWithValue("$id", id);

// Errado
comando.CommandText = "SELECT * FROM Produtos WHERE Id = " + id;
```
**Por quê?**
- Evita **SQL Injection**, um dos ataques mais comuns e perigosos.
- Resolve automaticamente formatação de datas, decimais e aspas.
- Permite ao banco reaproveitar o plano de execução.

### Usando o retorno do `ExecuteNonQuery`
```csharp
return comando.ExecuteNonQuery() > 0;
```
Se o `UPDATE` ou `DELETE` não encontrou nenhum `Id`, retorna `0`. Assim sabemos se a operação **realmente aconteceu**, e o programa informa isso ao usuário.

---

## 8. Linguagem SQL aprendida

```sql
-- Criar tabela só se ainda não existir
CREATE TABLE IF NOT EXISTS Produtos (
    Id         INTEGER PRIMARY KEY AUTOINCREMENT,
    Nome       TEXT    NOT NULL,
    Preco      TEXT    NOT NULL,
    Quantidade INTEGER NOT NULL
);

-- Create
INSERT INTO Produtos (Nome, Preco, Quantidade) VALUES ($nome, $preco, $quantidade);

-- Read
SELECT Id, Nome, Preco, Quantidade FROM Produtos ORDER BY Id;
SELECT Id, Nome, Preco, Quantidade FROM Produtos WHERE Id = $id;

-- Update
UPDATE Produtos SET Nome = $nome, Preco = $preco, Quantidade = $quantidade WHERE Id = $id;

-- Delete
DELETE FROM Produtos WHERE Id = $id;
```

**Pontos de atenção que aprendi:**
- **`WHERE` no `UPDATE` e no `DELETE` é crítico.** Sem ele, o comando afeta **todas as linhas da tabela**.
- `IF NOT EXISTS` torna a criação da tabela **idempotente**: pode ser executada várias vezes sem erro.
- `ORDER BY` garante a ordem; sem ele, a ordem do resultado não é garantida.
- Nomear as colunas no `INSERT` e no `SELECT` deixa o código mais seguro contra mudanças na tabela.

---

## 9. Fundamentos de C# aplicados

### Orientação a objetos
- **Classe** (`Produto`, `ProdutoRepository`): molde para criar objetos.
- **Propriedades** (`{ get; set; }`): forma idiomática do C# de expor dados.
- **Encapsulamento**: a `ConnectionString` é `private const`, e só o repositório a conhece.
- **Sobrescrita de método** (`override ToString()`): define como o objeto aparece como texto.

### Sintaxe moderna do C#
| Recurso | Exemplo no projeto | Para que serve |
|---------|--------------------|----------------|
| *Top-level statements* | `Program.cs` sem `class Program` / `Main` | Menos código repetitivo no ponto de entrada |
| *File-scoped namespace* | `namespace CrudSqlite.Models;` | Menos indentação |
| `using var` | `using var conexao = ...` | Libera recursos ao sair do escopo |
| *Implicit usings* | `<ImplicitUsings>enable</ImplicitUsings>` | `System`, `System.Linq` etc. já importados |
| *Object initializer* | `new Produto { Nome = ..., Preco = ... }` | Cria e preenche o objeto em uma expressão |
| *Expression-bodied member* | `public override string ToString() => ...` | Método de uma linha |
| *String interpolation* | `$"[{Id}] {Nome}"` | Montar texto com valores |
| *Funções locais* | `void Cadastrar()` dentro do `Program.cs` | Funções auxiliares sem precisar de classe |

### Nulidade (*Nullable Reference Types*)
```csharp
public Produto? BuscarPorId(int id)   // o "?" diz: pode retornar null
```
```csharp
if (produto is null) { ... }                          // checagem de null
Console.WriteLine(produto?.ToString() ?? "Não achou"); // ?. e ??
```
**Aprendizado:** o `?` no tipo é uma **informação contratual**: quem chama sabe que precisa tratar o caso "não encontrado". Isso previne a famosa `NullReferenceException`.

### Coleções e fluxo de controle
- `List<Produto>`: lista tipada e dinâmica, usada para devolver vários registros.
- `while (true)` com `return`: laço do menu que só termina quando o usuário escolhe sair.
- `switch`: escolha entre opções do menu.
- `foreach`: percorrer a lista para exibir.

### Entrada de dados segura
```csharp
if (int.TryParse(Console.ReadLine(), out var valor)) return valor;
```
`TryParse` **não lança exceção** se o texto for inválido, e apenas devolve `false`. É a forma correta de converter entrada do usuário.

---

## 10. Organização e arquitetura

### Separação de responsabilidades
| Camada | Arquivo | Responsabilidade |
|--------|---------|------------------|
| Apresentação | `Program.cs` | Interagir com o usuário |
| Acesso a dados | `ProdutoRepository.cs` | Falar com o banco |
| Modelo | `Produto.cs` | Representar o dado |

### Padrão Repository
A classe `ProdutoRepository` **isola o acesso ao banco**. Se um dia trocarmos SQLite por SQL Server, só ela precisa mudar, e o menu continua igual.

### Modelo ↔ Tabela
Cada propriedade de `Produto` corresponde a uma coluna da tabela `Produtos`. Esse mapeamento, feito à mão aqui (`leitor.GetString(1)` → `Nome`), é o que ORMs como o Entity Framework fazem automaticamente. Ter feito manualmente ajuda a entender o que a ferramenta faz "por baixo dos panos".

---

## 11. Ferramentas do ecossistema .NET

- **`.csproj`**: arquivo XML que descreve o projeto (tipo de saída, versão do .NET, pacotes).
- **NuGet**: gerenciador de pacotes. Adicionamos o `Microsoft.Data.Sqlite` via `<PackageReference>`.
- **`dotnet` CLI**:
  - `dotnet new console` → cria projeto
  - `dotnet add package <nome>` → adiciona pacote
  - `dotnet restore` → baixa as dependências
  - `dotnet build` → compila
  - `dotnet run` → compila e executa
- **Driver (provider)**: biblioteca que "ensina" o .NET a conversar com um banco específico.

---

## 12. Boas práticas já aplicadas

- Uso de **parâmetros** em vez de concatenação de strings no SQL.
- Conexões **abertas e fechadas** por operação com `using`.
- **Separação de responsabilidades** em pastas e classes.
- Tratamento de retorno nulo com **tipos anuláveis**.
- Conversão de entrada com **`TryParse`**.
- Criação de tabela **idempotente** (`IF NOT EXISTS`).
- Nomes de colunas explícitos nas consultas.
- Retorno `bool` nas operações de `UPDATE`/`DELETE` para informar sucesso.

---

## 13. O que ficou de fora

Intencionalmente, por ser um projeto de estudo:

| Tema | Por que importa em projetos reais |
|------|-----------------------------------|
| Tratamento de exceções (`try/catch`) | Erros de banco não devem derrubar o programa |
| Validações (nome vazio, preço negativo) | Dados inválidos não deveriam chegar ao banco |
| Transações (`BeginTransaction`) | Garantem que várias operações ocorram juntas ou nenhuma ocorra |
| Segurança e autenticação | Controle de quem pode ler/alterar dados |
| Testes automatizados | Garantem que o CRUD continua funcionando após mudanças |
| Migrações | Versionar mudanças na estrutura do banco |
| Async/await | Não travar a aplicação durante operações de I/O |
| Interface (Web/Desktop) | O console é só para fins didáticos |

---

## 14. Exercícios para praticar

**Nível básico**
1. Adicione o campo `Categoria` (texto) ao produto, passando pela tabela, pelo modelo, pelo repositório e pelo menu.
2. Impeça cadastrar produto com nome vazio ou preço negativo.
3. Peça confirmação ("Tem certeza? S/N") antes de excluir.

**Nível intermediário**
4. Crie o método `BuscarPorNome(string termo)` usando `WHERE Nome LIKE $termo`.
5. Crie uma opção que mostre o **valor total em estoque** (`SUM(Preco * Quantidade)`), usando `ExecuteScalar()`.
6. Envolva as chamadas ao banco em `try/catch` e mostre mensagens amigáveis.

**Nível avançado**
7. Crie uma segunda tabela (`Categorias`) e relacione com `Produtos` usando **chave estrangeira** e `JOIN`.
8. Extraia uma interface `IProdutoRepository` e entenda por que isso facilita testes.
9. Converta os métodos para **assíncronos** (`ExecuteNonQueryAsync`, `OpenAsync`).

---

## 15. Próximos passos

1. **Entity Framework Core**: substituir o SQL manual por um ORM e ver o que muda.
2. **Dapper**: meio-termo entre ADO.NET puro e ORM completo.
3. **ASP.NET Core Web API**: expor o CRUD via HTTP (`GET`, `POST`, `PUT`, `DELETE`).
4. **Bancos servidor**: repetir o projeto com PostgreSQL ou SQL Server.
5. **Testes unitários** com xUnit.
6. **Injeção de dependência** e configuração via `appsettings.json`.

---

## 16. Resumo em uma tabela

| Conceito | Resumo |
|----------|--------|
| CRUD | Create, Read, Update, Delete: as 4 operações básicas sobre dados |
| SQL | `INSERT`, `SELECT`, `UPDATE`, `DELETE` correspondem ao CRUD |
| SQLite | Banco relacional em um único arquivo, ótimo para estudo |
| ADO.NET | Base de acesso a dados do .NET |
| Connection string | Texto que informa onde está o banco |
| `SqliteConnection` | Abre a ponte com o banco |
| `SqliteCommand` | Representa o comando SQL |
| `ExecuteNonQuery` | Para comandos que alteram dados |
| `ExecuteReader` | Para consultas que retornam linhas |
| Parâmetros | Protegem contra SQL Injection e formatam valores |
| `using` | Fecha conexões automaticamente |
| Repository | Isola o código de acesso ao banco |
| Model | Classe que representa uma linha da tabela |
| `?` (nullable) | Indica que um valor pode ser nulo |
| `TryParse` | Converte entrada do usuário sem lançar exceção |
| NuGet | Gerenciador de pacotes do .NET |
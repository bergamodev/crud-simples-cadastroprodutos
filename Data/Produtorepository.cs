using CrudSqlite.Models;
using Microsoft.Data.Sqlite;

namespace CrudSqlite.Data;

public class ProdutoRepository
{
    // "Data Source" é o caminho do arquivo .db. Se não existir, o SQLite cria sozinho.
    private const string ConnectionString = "Data Source=loja.db";

    // Cria a tabela caso ainda não exista
    public void CriarTabela()
    {
        using var conexao = new SqliteConnection(ConnectionString);
        conexao.Open();

        var comando = conexao.CreateCommand();
        comando.CommandText = @"
            CREATE TABLE IF NOT EXISTS Produtos (
                Id         INTEGER PRIMARY KEY AUTOINCREMENT,
                Nome       TEXT    NOT NULL,
                Preco      TEXT    NOT NULL,
                Quantidade INTEGER NOT NULL
            );";
        comando.ExecuteNonQuery();
    }

    // CREATE
    public void Inserir(Produto produto)
    {
        using var conexao = new SqliteConnection(ConnectionString);
        conexao.Open();

        var comando = conexao.CreateCommand();
        comando.CommandText = @"
            INSERT INTO Produtos (Nome, Preco, Quantidade)
            VALUES ($nome, $preco, $quantidade);";

        // Parâmetros evitam concatenar strings no SQL (boa prática desde o início)
        comando.Parameters.AddWithValue("$nome", produto.Nome);
        comando.Parameters.AddWithValue("$preco", produto.Preco);
        comando.Parameters.AddWithValue("$quantidade", produto.Quantidade);

        comando.ExecuteNonQuery();
    }

    // READ (todos)
    public List<Produto> ListarTodos()
    {
        var lista = new List<Produto>();

        using var conexao = new SqliteConnection(ConnectionString);
        conexao.Open();

        var comando = conexao.CreateCommand();
        comando.CommandText = "SELECT Id, Nome, Preco, Quantidade FROM Produtos ORDER BY Id;";

        using var leitor = comando.ExecuteReader();
        while (leitor.Read())
        {
            lista.Add(new Produto
            {
                Id = leitor.GetInt32(0),
                Nome = leitor.GetString(1),
                Preco = leitor.GetDecimal(2),
                Quantidade = leitor.GetInt32(3)
            });
        }

        return lista;
    }

    // READ (um só)
    public Produto? BuscarPorId(int id)
    {
        using var conexao = new SqliteConnection(ConnectionString);
        conexao.Open();

        var comando = conexao.CreateCommand();
        comando.CommandText = "SELECT Id, Nome, Preco, Quantidade FROM Produtos WHERE Id = $id;";
        comando.Parameters.AddWithValue("$id", id);

        using var leitor = comando.ExecuteReader();
        if (!leitor.Read()) return null;

        return new Produto
        {
            Id = leitor.GetInt32(0),
            Nome = leitor.GetString(1),
            Preco = leitor.GetDecimal(2),
            Quantidade = leitor.GetInt32(3)
        };
    }

    // UPDATE — retorna true se alguma linha foi alterada
    public bool Atualizar(Produto produto)
    {
        using var conexao = new SqliteConnection(ConnectionString);
        conexao.Open();

        var comando = conexao.CreateCommand();
        comando.CommandText = @"
            UPDATE Produtos
            SET Nome = $nome, Preco = $preco, Quantidade = $quantidade
            WHERE Id = $id;";
        comando.Parameters.AddWithValue("$nome", produto.Nome);
        comando.Parameters.AddWithValue("$preco", produto.Preco);
        comando.Parameters.AddWithValue("$quantidade", produto.Quantidade);
        comando.Parameters.AddWithValue("$id", produto.Id);

        return comando.ExecuteNonQuery() > 0;
    }

    // DELETE — retorna true se alguma linha foi removida
    public bool Excluir(int id)
    {
        using var conexao = new SqliteConnection(ConnectionString);
        conexao.Open();

        var comando = conexao.CreateCommand();
        comando.CommandText = "DELETE FROM Produtos WHERE Id = $id;";
        comando.Parameters.AddWithValue("$id", id);

        return comando.ExecuteNonQuery() > 0;
    }
}
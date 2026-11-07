namespace CrudSqlite.Models;

//Representa uma linha da tabela "Produtos"
public class Produto
{
  public int Id {get; set;}
  public string Nome {get; set;}
  public decimal Preco {get; set;}
  public int Quantidade {get; set;}
  public override string ToString() => $"\n[{Id}] {Nome} | R$ {Preco:N2} | Qtd: {Quantidade}";
}
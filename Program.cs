using CrudSqlite.Data;
using CrudSqlite.Models;

var repositorio = new ProdutoRepository();
repositorio.CriarTabela();

while (true)
{
  Console.WriteLine("\n=== CRUD SIMPLES ===");
  Console.WriteLine("1 - Cadastrar");
  Console.WriteLine("2 - Listar");
  Console.WriteLine("3 - Buscar por Id");
  Console.WriteLine("4 - Atualizar");
  Console.WriteLine("5 - Excluir");
  Console.WriteLine("0 - SAIR");
  Console.Write("Opção: ");

  switch (Console.ReadLine())
  {
    case "1": Cadastrar(); break;
    case "2": Listar(); break;
    case "3": Buscar(); break;
    case "4": Atualizar(); break;
    case "5": Excluir(); break;
    case "0": return;
    default: Console.WriteLine("Opção inválida!"); break;
  }
}

void Cadastrar()
{
  var produto = new Produto
  {
    Nome = LerTexto("Nome: "),
    Preco = LerDecimal("Preco: "),
    Quantidade = LerInt("Quantidade: ")
  };
  repositorio.Inserir(produto);
  Console.WriteLine("Produto cadastrado!");
}

void Listar()
{
  var produtos = repositorio.ListarTodos();
  if(produtos.Count == 0){Console.WriteLine("Nenhum produto cadastrado.");
  return; }
  foreach(var p in produtos) Console.WriteLine(p);
}

void Buscar()
{
  var produto = repositorio.BuscarPorId(LerInt("Id: "));
  Console.WriteLine(produto?.ToString() ?? "Produto não encontrado.");
}

void Atualizar()
{
  var produto = repositorio.BuscarPorId(LerInt("Id do produto a atualizar: "));
  if(produto is null){Console.WriteLine("Produto não encontrado."); return; }
  Console.WriteLine($"Atual: {produto}");
  produto.Nome = LerTexto("Novo nome: ");
  produto.Preco = LerDecimal("Novo preço: ");
  produto.Quantidade = LerInt("Nova quantidade: ");
  Console.WriteLine(repositorio.Atualizar(produto) ? "Atualizado." : "Nada foi alterado.");
}

void Excluir()
{
  var id = LerInt("Id do produto que deseja excluir: ");
  Console.WriteLine(repositorio.Excluir(id) ? "Excluido" : "Produto não encontrado.");
}

//Funções para ler dados do console
string LerTexto(string mensagem)
{
  Console.Write(mensagem);
  return Console.ReadLine() ?? "";
}

int LerInt(string mensagem)
{
  while (true)
  {
    Console.Write(mensagem);
    if(int.TryParse(Console.ReadLine(), out var valor)) return valor;
    Console.WriteLine("Número inválido, tente denovo.");
  }
}

decimal LerDecimal(string mensagem)
{
  while (true)
  {
    Console.Write(mensagem);
    if(decimal.TryParse(Console.ReadLine(), out var valor)) return valor;
    Console.WriteLine("Valor inválido, tente denovo.");
  }
}
public static class EventosController{
    private static List<Evento> Eventos = new();
    private static int idAtual = 1;
    public static void ListarEventos(this WebApplication app){
        app.MapGet("/api/eventos/listar", () => 
        {
            return Results.Ok(Eventos);
        });
    }
    public static void ListarEventoPorId(this WebApplication app){
        app.MapGet("/api/eventos/listar/{id}", (int id) => 
        {
            var evento = Eventos.FirstOrDefault(e => e.Id == id);
            if(evento == null)
                return Results.NotFound("Evento não encontrado.");
            return Results.Ok(evento);
        });
    }
    public static void CadastrarEventos(this WebApplication app){
        app.MapPost("/api/eventos/cadastrar", (Evento novoEvento) => 
        {

            if(Eventos.Any(e => e.Nome == novoEvento.Nome)){
                return Results.BadRequest("O nome de evento informado já está cadastrado");
            }

            if(novoEvento.Data < DateTime.Now){
                return Results.BadRequest("A data do evento não pode ser antiga, coloque uma data futura!");
            }

            novoEvento.Id = idAtual;
            idAtual++;

            Eventos.Add(novoEvento);
            return Results.Ok(novoEvento);
        });
    }
     
}

public class Evento{
    public int Id {get;set;}
    public string Nome {get;set;} = "";
    public string Descricao {get;set;} = "";
    public string Local {get;set;} = "";
    public DateTime Data {get;set;}
    public int QuantidadeIngressos {get;set;}
    public float ValorIngresso {get;set;}
    public string? FotoUrl { get; set; }
}
public static class CuponsController{
    public static List<Cupons> Cupons = new();
    private static int idAtual = 1;
    public static void ListarCupons(this WebApplication app){
        app.MapGet("/api/cupons/listar", () => 
        {
            return Results.Ok(Cupons);
        });
    }
    public static void CadastrarCupons(this WebApplication app){
        app.MapPost("/api/cupons/cadastrar", (Cupons novoCupon) => 
        {
            if(Cupons.Any(c => c.Codigo == novoCupon.Codigo)){
                return Results.BadRequest("O cupon informado já está cadastrado");
            }

            novoCupon.Id = idAtual;
            idAtual++;

            Cupons.Add(novoCupon);
            return Results.Ok(novoCupon);
        });
    }
    
}

public class Cupons{
    public int Id {get;set;}
    public string Codigo {get;set;} = "";
    public int PercentualDesconto {get;set;}
}
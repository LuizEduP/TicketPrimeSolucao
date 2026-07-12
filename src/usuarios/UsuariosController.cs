using Microsoft.AspNetCore.Http.HttpResults;

public static class UsuariosController{
    private static List<Usuario> Usuarios = new();
    private static int idAtual = 1;
    public static void ListarUsuarios(this WebApplication app){
        app.MapGet("/api/usuarios/listar", () => 
        {
            return Results.Ok(Usuarios);
        });
    }
    public static void CadastrarUsuarios(this WebApplication app){
        app.MapPost("/api/usuarios/cadastrar", (Usuario novoUsuario) => 
        {
            if(novoUsuario.Cpf.Length != 11){
                return Results.BadRequest("O cpf deve ter 11 caracteres");
            }

            if(novoUsuario.Senha.Length < 6){
                return Results.BadRequest("A senha deve ter pelo menos 6 caracteres");
            }
            
            if(Usuarios.Any(u => u.Cpf == novoUsuario.Cpf)){
                return Results.BadRequest("O cpf informado já está cadastrado");
            }

            novoUsuario.Id = idAtual;
            idAtual++;

            Usuarios.Add(novoUsuario);
            return Results.Ok(novoUsuario);
        });
    }
    
}

public class Usuario{
    public int Id {get;set;}
    public string Nome {get;set;} = "";
    public string Email {get;set;} = "";
    public string Cpf {get;set;} = "";
    public bool Adm {get;set;} = false;
    public string Senha {get;set;} = ""; 
}
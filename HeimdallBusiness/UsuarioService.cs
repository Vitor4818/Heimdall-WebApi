namespace HeimdallBusiness;
using HeimdallModel; // Adicione isso no início do arquivo MotoService.cs


public class UsuarioService {
    private static readonly List<UsuarioModel> _usuarios = new();
    private static int _nextId = 1;

    public List<UsuarioModel> ListarUsuario () => _usuarios;

    public UsuarioModel? ObterPorId(int id) => _usuarios.FirstOrDefault(m=> m.id == id);
    public UsuarioModel? ObterPorNome(string nome) => _usuarios.FirstOrDefault(m=> m.Nome == nome);

    public UsuarioModel CadastrarUsuario(UsuarioModel user){
        user.id = _nextId++;
        _usuarios.Add(user);
        return user;
    }

    public bool AtualizarUsuario(UsuarioModel user){
        var existente = ObterPorId(user.id);
        if(existente == null) return false; 
        existente.Nome = user.Nome;
        existente.Sobrenome = user.Sobrenome;
        existente.DataNascimento = user.DataNascimento;
        existente.Cpf = user.Cpf;
        existente.Email = user.Email;
        existente.Senha = user.Senha;
        existente.CategoriaUsuario = user.CategoriaUsuario;
        return true;
    }
    public bool RemoverUsuario(int id){
        {
            var usuario = ObterPorId(id);
            if (usuario == null) return false;
        

        _usuarios.Remove(usuario);
        return true;
    }
}
}
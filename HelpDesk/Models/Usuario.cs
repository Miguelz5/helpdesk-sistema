using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email em formato inválido")]
        public string Email { get; set; }

        // Senha sem [Required] - validação será feita no controller
        public string Senha { get; set; }

        public DateTime DataCadastro { get; set; }
        public bool IsAdministrador { get; set; }
    }
}
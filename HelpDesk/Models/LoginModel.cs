using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email em formato inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Senha é obrigatória")]
        [DataType(DataType.Password)]
        public string Senha { get; set; }

        public bool LembrarMe { get; set; }
    }
}
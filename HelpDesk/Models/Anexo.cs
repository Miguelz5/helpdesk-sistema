// Models/Anexo.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Models
{
    public class Anexo
    {
        public int Id { get; set; }

        [Required]
        public int ChamadoId { get; set; }

        [Required]
        public string NomeArquivo { get; set; }

        [Required]
        public string TipoArquivo { get; set; }

        public long TamanhoArquivo { get; set; }

        [Required]
        public byte[] DadosArquivo { get; set; }

        public DateTime DataUpload { get; set; }

        [Required]
        public string UploadPor { get; set; }

        // Navigation property
        public Chamado Chamado { get; set; }
    }
}
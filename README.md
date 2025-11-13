 🖥️ Sistema Help Desk - ASP.NET Web

Sistema de gerenciamento de chamados técnicos desenvolvido em ASP.NET Core MVC com interface web responsiva e recursos avançados de suporte técnico.

 ✨ Funcionalidades

 🎯 Gestão de Chamados
- Criar, editar e excluir chamados com interface intuitiva
- Categorização automática (Hardware, Software, Rede, Acesso, Outros)
- Controle de prioridades (Urgente, Alta, Média, Baixa)
- Acompanhamento em tempo real do status dos chamados
- Atribuição para técnicos específicos

 📊 Dashboard e Estatísticas
- Visualizar estatísticas em tempo real (total, abertos, resolvidos)
- Gráficos interativos com Chart.js
- Filtro de chamados urgentes
- Métricas de desempenho por técnico e categoria

 💬 Sistema de Chat Integrado
- Chat em tempo real entre usuários e técnicos
- Histórico de conversas persistente
- Interface similar ao WhatsApp para melhor experiência
- Notificações de novas mensagens

 🤖 Inteligência Artificial
- Sugestões automáticas de respostas com Gemini AI
- FAQ dinâmico baseado no histórico de chamados
- Categorização inteligente de novos chamados
- Análise de similaridade entre problemas

 📁 Gestão de Arquivos
- Upload de anexos nos chamados
- Preview de imagens e documentos
- Controle de tamanho e tipos de arquivo
- Download seguro de anexos

 🛠️ Tecnologias Utilizadas

- Backend: ASP.NET Core MVC
- Frontend: Bootstrap 5, HTML5, CSS3, JavaScript
- Banco de Dados: SQL Server
- IA: Google Gemini AI API
- Comunicação em Tempo Real: SignalR
- Gráficos: Chart.js
- Icons: Font Awesome

 🚀 Como Executar

 Pré-requisitos
- Visual Studio 2022 ou superior
- .NET 6.0 SDK ou superior
- SQL Server 2012+
- Conexão com internet (para recursos de IA)

 Instalação e Execução

1. Clone o repositório:
```bash
git clone https://github.com/seu-usuario/helpdesk-web.git
cd helpdesk-web
```

2. Configure o banco de dados:
- Execute o script SQL de criação do banco
- Atualize a connection string no `appsettings.json`

3. Configure a API Key do Gemini AI no `appsettings.json`:
```json
{
  "GeminiApi": {
    "ApiKey": "sua-api-key-aqui",
    "Endpoint": "https://generativelanguage.googleapis.com/v1beta/models/gemini-pro"
  }
}
```

4. Execute o projeto:
- Abra o projeto no Visual Studio 2022
- Restaure os pacotes NuGet (se necessário)
- Pressione F5 para executar

 🐳 Docker (Opcional)

```bash
docker build -t helpdesk-web .
docker run -p 8080:80 helpdesk-web
```

 📱 Acesso ao Sistema

 👥 Tipos de Usuário
- Usuário Comum: Abertura e acompanhamento de chamados
- Técnico: Atendimento de chamados e uso do chat
- Administrador: Gestão completa do sistema

 🔐 Credenciais de Teste
```
Admin: admin@helpdesk.com / 123456
Técnico: tecnico@helpdesk.com / 123456
Usuário: usuario@teste.com / 123456
```

 🔧 Configuração

 Variáveis de Ambiente
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=HelpDeskDB;Integrated Security=true;"
  },
  "GeminiApi": {
    "ApiKey": "sua-chave-aqui"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

 🌐 Integrações

 🔗 Plataforma Desktop
- Sincronização em tempo real com versão desktop
- Banco de dados compartilhado
- Notificações cruzadas entre plataformas

 ☁️ APIs Externas
- Google Gemini AI para automação de respostas
- Serviço de E-mail para notificações
- Armazenamento em nuvem para backup de anexos

 🐛 Solução de Problemas

 Problemas Comuns e Soluções

1. Erro de conexão com banco de dados
   - Verifique a connection string no appsettings.json
   - Confirme se o SQL Server está em execução

2. Chat não funciona
   - Verifique se o SignalR está configurado corretamente
   - Confirme as permissões do usuário

3. IA não responde
   - Valide a API Key do Gemini AI
   - Verifique a conexão com a internet

4. Upload de arquivos falha
   - Confirme as permissões da pasta wwwroot/uploads
   - Verifique o tamanho máximo permitido

 🤝 Contribuição

Contribuições são sempre bem-vindas! Para contribuir:

1. Faça um fork do projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para detalhes.

 👥 Desenvolvedores

- Miguel da Silva Faria 
- Gustavo Alves de Araújo 

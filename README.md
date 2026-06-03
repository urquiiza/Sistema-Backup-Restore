# Backup Restore

![Plataforma](https://img.shields.io/badge/Plataforma-WPF-blue)
![.NET](https://img.shields.io/badge/.NET-8.0-blueviolet)
![C#](https://img.shields.io/badge/Linguagem-C%23-lightgrey)

## 🎯 Descrição
O **Backup Restore** é uma aplicação desktop desenvolvida em C# (WPF) com o objetivo de automatizar e gerir operações de backup, restauração e manutenção preventiva em bancos de dados Firebird e PostgreSQL. O sistema garante a saúde dos dados de forma silenciosa e eficiente, ideal para ambientes que exigem alta disponibilidade.

## 🧠 Desafios Técnicos e Arquitetura (Destaques)
Para tornar esta aplicação robusta, foram implementadas as seguintes soluções:
- **Prevenção de Memory Leaks (Deadlocks):** O redirecionamento nativo do `StandardOutput` em bancos grandes causava o congelamento da aplicação. A solução foi implementar leitura assíncrona (`BeginOutputReadLine`) utilizando buffers de memória isolados.
- **Integração com SO (Task Scheduler):** O sistema não depende de estar aberto para funcionar. Ele comunica via código com o Agendador de Tarefas do Windows, criando rotinas "invisíveis" para a madrugada.
- **UX:** A tela adapta-se dinamicamente ao contexto do utilizador, ocultando ou exibindo opções de agendamento e bloqueando concorrência de ações para evitar falhas humanas.

## 🚀 Funcionalidades
- **Backup e Restauração**: Suporte nativo para Firebird (2.5 e 4.0) e PostgreSQL.
- **Manutenção Automática**: 
  - Firebird: Ciclo de backup e restauração automatizados.
  - PostgreSQL: Vacuum e reindexação.
- **Agendador Integrado**: Configuração visual de tarefas do Windows para execuções de madrugada.
- **Sistema de Logs**: Registo detalhado e rotativo (sucesso e erro) em arquivos de texto locais, garantindo rastreabilidade sem travar os processos invisíveis.

## 🛠️ Tecnologias Utilizadas
- **Plataforma**: WPF
- **Linguagem**: C# 12.0
- **Framework**: .NET 8.0
- **Bancos de Dados**: Firebird, PostgreSQL

## ⚙️ Pré-requisitos
- .NET 8.0 instalado.
- Caminhos para os executáveis nativos dos bancos (`gbak.exe`, `pg_dump.exe`, `pg_restore.exe`, etc.) configurados no ambiente alvo.

## 💻 Como Usar
1. **Clone o Repositório**.
2. **Abra o Projeto no Visual Studio 2022** (certifique-se de ter as cargas de trabalho do WPF instaladas).
3. **Configure o Ambiente**: Insira os caminhos dos executáveis e as credenciais dos bancos de dados diretamente na interface.
4. **Agende ou Execute**: Escolha entre disparar a manutenção de forma manual ou utilizar o painel de agendamento para criar a tarefa no Windows.

## 📂 Estrutura do Projeto
- `MainWindow.xaml` / `.cs`: Interface principal com validação de dados e máquina de estados da UI.
- `ComandosFirebird.cs` / `ComandosPostgres.cs`: Lógica de I/O e argumentos de terminal específicos de cada motor.
- `AgendaManutencao.cs`: Motor de execução em segundo plano com tratamento assíncrono de terminal.

## 🤝 Contribuição
Contribuições são bem-vindas! Sinta-se à vontade para abrir issues ou enviar pull requests.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESCOLA_API.Models;
using ESCOLA_API.Security;
using Microsoft.EntityFrameworkCore;

namespace ESCOLA_API.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<Aluno> Alunos { get; set; }
        public DbSet<Professor> Professores { get; set; }
        public DbSet<Diretoria> Diretorias { get; set; }
        public DbSet<Perfil> Perfis { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<TipoEnsino> TiposEnsino { get; set; }
        public DbSet<TurmaEnsino> TurmasEnsino { get; set; }
        public DbSet<AlunoTurmaEnsino> AlunosTurmasEnsino { get; set; }
        public DbSet<AreaConhecimento> AreasConhecimento { get; set; }
        public DbSet<Disciplina> Disciplinas { get; set; }
        public DbSet<DisciplinaEvento> DisciplinaEventos { get; set; }
        public DbSet<CalendarioEscolarEvento> CalendarioEscolarEventos { get; set; }
        public DbSet<CadernetaDigital> CadernetasDigitais { get; set; }
        public DbSet<UsuarioArquivo> UsuarioArquivos { get; set; }
        public DbSet<Holerite> Holerites { get; set; }
        public DbSet<Notificacao> Notificacoes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Perfil>(entity =>
            {
                entity.ToTable("Perfil");
                entity.HasKey(perfil => perfil.IdPerfil);
                entity.Property(perfil => perfil.DescricaoPerfil)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            builder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuario");
                entity.HasKey(usuario => usuario.IdUsuario);
                entity.Property(usuario => usuario.Nome)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(usuario => usuario.Email)
                    .IsRequired()
                    .HasMaxLength(150);
                entity.Property(usuario => usuario.Telefone)
                    .IsRequired()
                    .HasMaxLength(20);
                entity.Property(usuario => usuario.DataNascimento)
                    .HasColumnType("date");
                entity.Property(usuario => usuario.NomeMae)
                    .HasMaxLength(100);
                entity.Property(usuario => usuario.NomePai)
                    .HasMaxLength(100);
                entity.Property(usuario => usuario.Endereco)
                    .HasMaxLength(200);
                entity.Property(usuario => usuario.Senha)
                    .IsRequired()
                    .HasMaxLength(255);
                entity.Property(usuario => usuario.FotoPerfilUrl)
                    .HasMaxLength(500);
                entity.Property(usuario => usuario.NomeUsuarioCriador)
                    .HasMaxLength(100);
                entity.Property(usuario => usuario.ExclusaoContaMotivo)
                    .HasMaxLength(500);
                entity.Property(usuario => usuario.ResetSenhaTokenHash)
                    .HasMaxLength(128);
                entity.HasIndex(usuario => usuario.Email)
                    .IsUnique();
                entity.HasOne(usuario => usuario.Perfil)
                    .WithMany(perfil => perfil.Usuarios)
                    .HasForeignKey(usuario => usuario.IdPerfil)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Professor>(entity =>
            {
                entity.HasOne(professor => professor.Usuario)
                    .WithMany(usuario => usuario.Professores)
                    .HasForeignKey(professor => professor.IdUsuario)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<Aluno>(entity =>
            {
                entity.HasOne(aluno => aluno.Professor)
                    .WithMany(professor => professor.Alunos)
                    .HasForeignKey(aluno => aluno.ProfessorId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(aluno => aluno.Usuario)
                    .WithMany(usuario => usuario.Alunos)
                    .HasForeignKey(aluno => aluno.IdUsuario)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<Diretoria>(entity =>
            {
                entity.ToTable("Diretoria");
                entity.HasOne(diretoria => diretoria.Usuario)
                    .WithMany(usuario => usuario.Diretorias)
                    .HasForeignKey(diretoria => diretoria.IdUsuario)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<TipoEnsino>(entity =>
            {
                entity.ToTable("TipoEnsino");
                entity.HasKey(tipo => tipo.IdTipoEnsino);
                entity.Property(tipo => tipo.Nome)
                    .IsRequired()
                    .HasMaxLength(80);
                entity.HasIndex(tipo => tipo.Nome)
                    .IsUnique();
            });

            builder.Entity<TurmaEnsino>(entity =>
            {
                entity.ToTable("TurmaEnsino");
                entity.HasKey(turma => turma.IdTurmaEnsino);
                entity.Property(turma => turma.Nome)
                    .IsRequired()
                    .HasMaxLength(80);
                entity.Property(turma => turma.Codigo)
                    .IsRequired()
                    .HasMaxLength(20);
                entity.HasIndex(turma => new { turma.IdTipoEnsino, turma.Codigo })
                    .IsUnique();
                entity.HasOne(turma => turma.TipoEnsino)
                    .WithMany(tipo => tipo.Turmas)
                    .HasForeignKey(turma => turma.IdTipoEnsino)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<AlunoTurmaEnsino>(entity =>
            {
                entity.ToTable("AlunoTurmaEnsino");
                entity.HasKey(matricula => matricula.IdAlunoTurmaEnsino);
                entity.Property(matricula => matricula.MatriculadoEmUtc)
                    .IsRequired();
                entity.HasIndex(matricula => matricula.IdAlunoUsuario)
                    .IsUnique();
                entity.HasIndex(matricula => matricula.IdTurmaEnsino);
                entity.HasOne(matricula => matricula.AlunoUsuario)
                    .WithMany(usuario => usuario.MatriculasTurma)
                    .HasForeignKey(matricula => matricula.IdAlunoUsuario)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(matricula => matricula.TurmaEnsino)
                    .WithMany(turma => turma.AlunosMatriculados)
                    .HasForeignKey(matricula => matricula.IdTurmaEnsino)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(matricula => matricula.UsuarioResponsavel)
                    .WithMany()
                    .HasForeignKey(matricula => matricula.IdUsuarioResponsavel)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<AreaConhecimento>(entity =>
            {
                entity.ToTable("AreaConhecimento");
                entity.HasKey(area => area.IdAreaConhecimento);
                entity.Property(area => area.Nome)
                    .IsRequired()
                    .HasMaxLength(120);
                entity.HasIndex(area => new { area.IdTipoEnsino, area.Nome })
                    .IsUnique();
                entity.HasOne(area => area.TipoEnsino)
                    .WithMany(tipo => tipo.AreasConhecimento)
                    .HasForeignKey(area => area.IdTipoEnsino)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Disciplina>(entity =>
            {
                entity.ToTable("Disciplina");
                entity.HasKey(disciplina => disciplina.IdDisciplina);
                entity.Property(disciplina => disciplina.Nome)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(disciplina => disciplina.Observacao)
                    .HasMaxLength(500);
                entity.Property(disciplina => disciplina.OfertaObrigatoria)
                    .HasDefaultValue(true);
                entity.HasIndex(disciplina => new { disciplina.IdProfessorUsuario, disciplina.IdTurmaEnsino, disciplina.Nome })
                    .IsUnique();
                entity.HasOne(disciplina => disciplina.ProfessorUsuario)
                    .WithMany(usuario => usuario.DisciplinasMinistradas)
                    .HasForeignKey(disciplina => disciplina.IdProfessorUsuario)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(disciplina => disciplina.TurmaEnsino)
                    .WithMany(turma => turma.Disciplinas)
                    .HasForeignKey(disciplina => disciplina.IdTurmaEnsino)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(disciplina => disciplina.AreaConhecimento)
                    .WithMany(area => area.Disciplinas)
                    .HasForeignKey(disciplina => disciplina.IdAreaConhecimento)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<DisciplinaEvento>(entity =>
            {
                entity.ToTable("DisciplinaEvento");
                entity.HasKey(evento => evento.IdEventoDisciplina);
                entity.Property(evento => evento.Tipo)
                    .IsRequired()
                    .HasMaxLength(20);
                entity.Property(evento => evento.Titulo)
                    .IsRequired()
                    .HasMaxLength(120);
                entity.Property(evento => evento.Descricao)
                    .HasMaxLength(500);
                entity.Property(evento => evento.Data)
                    .HasColumnType("date");
                entity.HasIndex(evento => new { evento.IdDisciplina, evento.Data });
                entity.HasOne(evento => evento.Disciplina)
                    .WithMany(disciplina => disciplina.Eventos)
                    .HasForeignKey(evento => evento.IdDisciplina)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<CalendarioEscolarEvento>(entity =>
            {
                entity.ToTable("CalendarioEscolarEvento");
                entity.HasKey(evento => evento.IdEventoCalendarioEscolar);
                entity.Property(evento => evento.Tipo)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(evento => evento.Titulo)
                    .IsRequired()
                    .HasMaxLength(120);
                entity.Property(evento => evento.Descricao)
                    .HasMaxLength(500);
                entity.Property(evento => evento.PublicoAlvo)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(evento => evento.NomeUsuarioCriador)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(evento => evento.Data)
                    .HasColumnType("date");
                entity.HasIndex(evento => evento.Data);
                entity.HasOne(evento => evento.UsuarioCriador)
                    .WithMany()
                    .HasForeignKey(evento => evento.IdUsuarioCriador)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<CadernetaDigital>(entity =>
            {
                entity.ToTable("CadernetaDigital");
                entity.HasKey(caderneta => caderneta.IdCadernetaDigital);
                entity.Property(caderneta => caderneta.Notas)
                    .IsRequired()
                    .HasMaxLength(120);
                entity.HasIndex(caderneta => caderneta.IdProfessorUsuario);
                entity.HasIndex(caderneta => new { caderneta.IdAlunoUsuario, caderneta.IdDisciplina })
                    .IsUnique();
                entity.HasOne(caderneta => caderneta.AlunoUsuario)
                    .WithMany(usuario => usuario.CadernetasComoAluno)
                    .HasForeignKey(caderneta => caderneta.IdAlunoUsuario)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(caderneta => caderneta.ProfessorUsuario)
                    .WithMany()
                    .HasForeignKey(caderneta => caderneta.IdProfessorUsuario)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(caderneta => caderneta.TipoEnsino)
                    .WithMany()
                    .HasForeignKey(caderneta => caderneta.IdTipoEnsino)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(caderneta => caderneta.TurmaEnsino)
                    .WithMany()
                    .HasForeignKey(caderneta => caderneta.IdTurmaEnsino)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(caderneta => caderneta.Disciplina)
                    .WithMany(disciplina => disciplina.Cadernetas)
                    .HasForeignKey(caderneta => caderneta.IdDisciplina)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<UsuarioArquivo>(entity =>
            {
                entity.ToTable("Arquivo");
                entity.HasKey(arquivo => arquivo.IdArquivo);
                entity.Property(arquivo => arquivo.NomeBlob)
                    .HasMaxLength(500);
                entity.Property(arquivo => arquivo.TipoArquivo)
                    .HasMaxLength(30);
                entity.Property(arquivo => arquivo.NomeOriginal)
                    .HasMaxLength(255);
                entity.Property(arquivo => arquivo.Url)
                    .HasMaxLength(500);
                entity.Property(arquivo => arquivo.ContentType)
                    .HasMaxLength(120);
                entity.HasOne(arquivo => arquivo.Usuario)
                    .WithMany(usuario => usuario.Arquivos)
                    .HasForeignKey(arquivo => arquivo.IdUsuario)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<Holerite>(entity =>
            {
                entity.ToTable("Holerite");
                entity.HasKey(holerite => holerite.IdHolerite);
                entity.Property(holerite => holerite.NomeBlob)
                    .IsRequired()
                    .HasMaxLength(500);
                entity.Property(holerite => holerite.NomeOriginal)
                    .IsRequired()
                    .HasMaxLength(255);
                entity.Property(holerite => holerite.Url)
                    .IsRequired()
                    .HasMaxLength(500);
                entity.Property(holerite => holerite.ContentType)
                    .IsRequired()
                    .HasMaxLength(120);
                entity.HasIndex(holerite => new { holerite.IdUsuario, holerite.CompetenciaAno, holerite.CompetenciaMes });
                entity.HasOne(holerite => holerite.Usuario)
                    .WithMany(usuario => usuario.Holerites)
                    .HasForeignKey(holerite => holerite.IdUsuario)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Notificacao>(entity =>
            {
                entity.ToTable("Notificacao");
                entity.HasKey(notificacao => notificacao.IdNotificacao);
                entity.Property(notificacao => notificacao.Tipo)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(notificacao => notificacao.Titulo)
                    .IsRequired()
                    .HasMaxLength(120);
                entity.Property(notificacao => notificacao.Mensagem)
                    .IsRequired()
                    .HasMaxLength(2000);
                entity.Property(notificacao => notificacao.Link)
                    .HasMaxLength(500);
                entity.Property(notificacao => notificacao.Notas)
                    .HasMaxLength(120);
                entity.Property(notificacao => notificacao.NomeTipoEnsino)
                    .HasMaxLength(80);
                entity.Property(notificacao => notificacao.NomeTurmaEnsino)
                    .HasMaxLength(80);
                entity.Property(notificacao => notificacao.NomeDisciplina)
                    .HasMaxLength(100);
                entity.Property(notificacao => notificacao.MediaAritmetica)
                    .HasPrecision(5, 2);
                entity.Property(notificacao => notificacao.Situacao)
                    .HasMaxLength(80);
                entity.Property(notificacao => notificacao.CorSituacao)
                    .HasMaxLength(30);
                entity.Property(notificacao => notificacao.OrigemMensagemId)
                    .HasMaxLength(160);
                entity.HasIndex(notificacao => notificacao.IdUsuario);
                entity.HasIndex(notificacao => notificacao.OrigemMensagemId)
                    .IsUnique()
                    .HasFilter("[OrigemMensagemId] IS NOT NULL");
                entity.HasOne(notificacao => notificacao.Usuario)
                    .WithMany(usuario => usuario.Notificacoes)
                    .HasForeignKey(notificacao => notificacao.IdUsuario)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Perfil>().HasData(CreatePerfis());
            builder.Entity<Usuario>().HasData(CreateUsuarios());
            builder.Entity<Professor>().HasData(CreateProfessores());
            builder.Entity<Aluno>().HasData(CreateAlunos());
            builder.Entity<Diretoria>().HasData(CreateDiretoria());
            builder.Entity<TipoEnsino>().HasData(CreateTiposEnsino());
            builder.Entity<TurmaEnsino>().HasData(CreateTurmasEnsino());
            builder.Entity<AreaConhecimento>().HasData(CreateAreasConhecimento());
            builder.Entity<Disciplina>().HasData(CreateDisciplinasCurriculares());
        }

        private const int TipoEnsinoFundamentalId = 1;
        private const int TipoEnsinoMedioId = 2;
        private const int AreaFundamentalLinguagensId = 101;
        private const int AreaFundamentalMatematicaId = 102;
        private const int AreaFundamentalCienciasNaturezaId = 103;
        private const int AreaFundamentalCienciasHumanasId = 104;
        private const int AreaFundamentalEnsinoReligiosoId = 105;
        private const int AreaMedioLinguagensId = 201;
        private const int AreaMedioMatematicaId = 202;
        private const int AreaMedioCienciasNaturezaId = 203;
        private const int AreaMedioCienciasHumanasId = 204;

        private static IEnumerable<TipoEnsino> CreateTiposEnsino()
        {
            return new[]
            {
                new TipoEnsino { IdTipoEnsino = TipoEnsinoFundamentalId, Nome = "Ensino Fundamental", Ordem = 1 },
                new TipoEnsino { IdTipoEnsino = TipoEnsinoMedioId, Nome = "Ensino Médio", Ordem = 2 }
            };
        }

        private static IEnumerable<TurmaEnsino> CreateTurmasEnsino()
        {
            return new[]
            {
                new TurmaEnsino { IdTurmaEnsino = 101, IdTipoEnsino = TipoEnsinoFundamentalId, Nome = "1º ano", Codigo = "EF1", Ordem = 1 },
                new TurmaEnsino { IdTurmaEnsino = 102, IdTipoEnsino = TipoEnsinoFundamentalId, Nome = "2º ano", Codigo = "EF2", Ordem = 2 },
                new TurmaEnsino { IdTurmaEnsino = 103, IdTipoEnsino = TipoEnsinoFundamentalId, Nome = "3º ano", Codigo = "EF3", Ordem = 3 },
                new TurmaEnsino { IdTurmaEnsino = 104, IdTipoEnsino = TipoEnsinoFundamentalId, Nome = "4º ano", Codigo = "EF4", Ordem = 4 },
                new TurmaEnsino { IdTurmaEnsino = 105, IdTipoEnsino = TipoEnsinoFundamentalId, Nome = "5º ano", Codigo = "EF5", Ordem = 5 },
                new TurmaEnsino { IdTurmaEnsino = 106, IdTipoEnsino = TipoEnsinoFundamentalId, Nome = "6º ano", Codigo = "EF6", Ordem = 6 },
                new TurmaEnsino { IdTurmaEnsino = 107, IdTipoEnsino = TipoEnsinoFundamentalId, Nome = "7º ano", Codigo = "EF7", Ordem = 7 },
                new TurmaEnsino { IdTurmaEnsino = 108, IdTipoEnsino = TipoEnsinoFundamentalId, Nome = "8º ano", Codigo = "EF8", Ordem = 8 },
                new TurmaEnsino { IdTurmaEnsino = 109, IdTipoEnsino = TipoEnsinoFundamentalId, Nome = "9º ano", Codigo = "EF9", Ordem = 9 },
                new TurmaEnsino { IdTurmaEnsino = 201, IdTipoEnsino = TipoEnsinoMedioId, Nome = "1ª série", Codigo = "EM1", Ordem = 1 },
                new TurmaEnsino { IdTurmaEnsino = 202, IdTipoEnsino = TipoEnsinoMedioId, Nome = "2ª série", Codigo = "EM2", Ordem = 2 },
                new TurmaEnsino { IdTurmaEnsino = 203, IdTipoEnsino = TipoEnsinoMedioId, Nome = "3ª série", Codigo = "EM3", Ordem = 3 }
            };
        }

        private static IEnumerable<AreaConhecimento> CreateAreasConhecimento()
        {
            return new[]
            {
                new AreaConhecimento { IdAreaConhecimento = AreaFundamentalLinguagensId, IdTipoEnsino = TipoEnsinoFundamentalId, Nome = "Linguagens", Ordem = 1 },
                new AreaConhecimento { IdAreaConhecimento = AreaFundamentalMatematicaId, IdTipoEnsino = TipoEnsinoFundamentalId, Nome = "Matemática", Ordem = 2 },
                new AreaConhecimento { IdAreaConhecimento = AreaFundamentalCienciasNaturezaId, IdTipoEnsino = TipoEnsinoFundamentalId, Nome = "Ciências da Natureza", Ordem = 3 },
                new AreaConhecimento { IdAreaConhecimento = AreaFundamentalCienciasHumanasId, IdTipoEnsino = TipoEnsinoFundamentalId, Nome = "Ciências Humanas", Ordem = 4 },
                new AreaConhecimento { IdAreaConhecimento = AreaFundamentalEnsinoReligiosoId, IdTipoEnsino = TipoEnsinoFundamentalId, Nome = "Ensino Religioso", Ordem = 5 },
                new AreaConhecimento { IdAreaConhecimento = AreaMedioLinguagensId, IdTipoEnsino = TipoEnsinoMedioId, Nome = "Linguagens e suas Tecnologias", Ordem = 1 },
                new AreaConhecimento { IdAreaConhecimento = AreaMedioMatematicaId, IdTipoEnsino = TipoEnsinoMedioId, Nome = "Matemática e suas Tecnologias", Ordem = 2 },
                new AreaConhecimento { IdAreaConhecimento = AreaMedioCienciasNaturezaId, IdTipoEnsino = TipoEnsinoMedioId, Nome = "Ciências da Natureza e suas Tecnologias", Ordem = 3 },
                new AreaConhecimento { IdAreaConhecimento = AreaMedioCienciasHumanasId, IdTipoEnsino = TipoEnsinoMedioId, Nome = "Ciências Humanas e Sociais Aplicadas", Ordem = 4 }
            };
        }

        private static IEnumerable<Disciplina> CreateDisciplinasCurriculares()
        {
            var disciplinas = new List<Disciplina>();
            var idDisciplina = 1001;

            foreach (var idTurma in new[] { 101, 102, 103, 104, 105 })
            {
                AddFundamentalDisciplinas(disciplinas, ref idDisciplina, idTurma, incluirLinguaInglesa: false);
            }

            foreach (var idTurma in new[] { 106, 107, 108, 109 })
            {
                AddFundamentalDisciplinas(disciplinas, ref idDisciplina, idTurma, incluirLinguaInglesa: true);
            }

            foreach (var idTurma in new[] { 201, 202, 203 })
            {
                AddMedioDisciplinas(disciplinas, ref idDisciplina, idTurma);
            }

            return disciplinas;
        }

        private static void AddFundamentalDisciplinas(
            List<Disciplina> disciplinas,
            ref int idDisciplina,
            int idTurmaEnsino,
            bool incluirLinguaInglesa)
        {
            AddDisciplina(disciplinas, ref idDisciplina, idTurmaEnsino, AreaFundamentalLinguagensId, "Língua Portuguesa", 1);
            AddDisciplina(disciplinas, ref idDisciplina, idTurmaEnsino, AreaFundamentalLinguagensId, "Arte", 2);
            AddDisciplina(disciplinas, ref idDisciplina, idTurmaEnsino, AreaFundamentalLinguagensId, "Educação Física", 3);

            if (incluirLinguaInglesa)
            {
                AddDisciplina(
                    disciplinas,
                    ref idDisciplina,
                    idTurmaEnsino,
                    AreaFundamentalLinguagensId,
                    "Língua Inglesa",
                    4,
                    "Obrigatória a partir do 6º ano.");
            }

            AddDisciplina(disciplinas, ref idDisciplina, idTurmaEnsino, AreaFundamentalMatematicaId, "Matemática", 1);
            AddDisciplina(disciplinas, ref idDisciplina, idTurmaEnsino, AreaFundamentalCienciasNaturezaId, "Ciências", 1);
            AddDisciplina(disciplinas, ref idDisciplina, idTurmaEnsino, AreaFundamentalCienciasHumanasId, "História", 1);
            AddDisciplina(disciplinas, ref idDisciplina, idTurmaEnsino, AreaFundamentalCienciasHumanasId, "Geografia", 2);
            AddDisciplina(
                disciplinas,
                ref idDisciplina,
                idTurmaEnsino,
                AreaFundamentalEnsinoReligiosoId,
                "Ensino Religioso",
                1,
                "Oferta obrigatória pela escola, matrícula facultativa para o aluno.",
                ofertaObrigatoria: true,
                matriculaFacultativa: true);
        }

        private static void AddMedioDisciplinas(List<Disciplina> disciplinas, ref int idDisciplina, int idTurmaEnsino)
        {
            AddDisciplina(disciplinas, ref idDisciplina, idTurmaEnsino, AreaMedioLinguagensId, "Língua Portuguesa", 1);
            AddDisciplina(disciplinas, ref idDisciplina, idTurmaEnsino, AreaMedioLinguagensId, "Literatura", 2);
            AddDisciplina(disciplinas, ref idDisciplina, idTurmaEnsino, AreaMedioLinguagensId, "Arte", 3);
            AddDisciplina(disciplinas, ref idDisciplina, idTurmaEnsino, AreaMedioLinguagensId, "Educação Física", 4);
            AddDisciplina(disciplinas, ref idDisciplina, idTurmaEnsino, AreaMedioLinguagensId, "Língua Inglesa", 5);
            AddDisciplina(disciplinas, ref idDisciplina, idTurmaEnsino, AreaMedioMatematicaId, "Matemática", 1);
            AddDisciplina(disciplinas, ref idDisciplina, idTurmaEnsino, AreaMedioCienciasNaturezaId, "Física", 1);
            AddDisciplina(disciplinas, ref idDisciplina, idTurmaEnsino, AreaMedioCienciasNaturezaId, "Química", 2);
            AddDisciplina(disciplinas, ref idDisciplina, idTurmaEnsino, AreaMedioCienciasNaturezaId, "Biologia", 3);
            AddDisciplina(disciplinas, ref idDisciplina, idTurmaEnsino, AreaMedioCienciasHumanasId, "História", 1);
            AddDisciplina(disciplinas, ref idDisciplina, idTurmaEnsino, AreaMedioCienciasHumanasId, "Geografia", 2);
            AddDisciplina(disciplinas, ref idDisciplina, idTurmaEnsino, AreaMedioCienciasHumanasId, "Filosofia", 3);
            AddDisciplina(disciplinas, ref idDisciplina, idTurmaEnsino, AreaMedioCienciasHumanasId, "Sociologia", 4);
        }

        private static void AddDisciplina(
            List<Disciplina> disciplinas,
            ref int idDisciplina,
            int idTurmaEnsino,
            int idAreaConhecimento,
            string nome,
            int ordem,
            string? observacao = null,
            bool ofertaObrigatoria = true,
            bool matriculaFacultativa = false)
        {
            disciplinas.Add(new Disciplina
            {
                IdDisciplina = idDisciplina++,
                Nome = nome,
                IdTurmaEnsino = idTurmaEnsino,
                IdAreaConhecimento = idAreaConhecimento,
                Observacao = observacao,
                OfertaObrigatoria = ofertaObrigatoria,
                MatriculaFacultativa = matriculaFacultativa,
                Ordem = ordem
            });
        }

        private static IEnumerable<Perfil> CreatePerfis()
        {
            return new[]
            {
                new Perfil { IdPerfil = PerfilSistema.AdministradorId, DescricaoPerfil = PerfilSistema.Administrador },
                new Perfil { IdPerfil = PerfilSistema.ProfessorId, DescricaoPerfil = PerfilSistema.Professor },
                new Perfil { IdPerfil = PerfilSistema.AlunoId, DescricaoPerfil = PerfilSistema.Aluno }
            };
        }

        private static IEnumerable<Usuario> CreateUsuarios()
        {
            var usuarios = new List<Usuario>
            {
                new Usuario
                {
                    IdUsuario = 1,
                    Nome = "Administrador Sistema",
                    Email = "admin@escola.com",
                    Telefone = "11999990001",
                    Senha = CreateSeedPassword(1),
                    IdPerfil = PerfilSistema.AdministradorId
                }
            };

            var professores = new[]
            {
                "Vinicius", "Paula", "Suzana", "Carlos", "Mariana",
                "Roberto", "Fernanda", "Ricardo", "Patricia", "Marcelo"
            };

            usuarios.AddRange(professores.Select((nome, index) => new Usuario
            {
                IdUsuario = index + 2,
                Nome = $"Professor {nome}",
                Email = $"professor{index + 1:00}@escola.com",
                Telefone = $"1198888{index + 1:0000}",
                Senha = CreateSeedPassword(index + 2),
                IdPerfil = PerfilSistema.ProfessorId
            }));

            var alunos = new[]
            {
                "Maria", "Joao", "Alex", "Ana", "Bruno",
                "Carla", "Daniel", "Elisa", "Fabio"
            };

            usuarios.AddRange(alunos.Select((nome, index) => new Usuario
            {
                IdUsuario = index + 12,
                Nome = $"Aluno {nome}",
                Email = $"aluno{index + 1:00}@escola.com",
                Telefone = $"1197777{index + 1:0000}",
                Senha = CreateSeedPassword(index + 12),
                IdPerfil = PerfilSistema.AlunoId
            }));

            return usuarios;
        }

        private static IEnumerable<Diretoria> CreateDiretoria()
        {
            return new[]
            {
                new Diretoria
                {
                    Id = 1,
                    Nome = "Administrador Sistema",
                    IdUsuario = 1
                }
            };
        }

        private static string CreateSeedPassword(int usuarioId)
        {
            var salt = Convert.ToBase64String(Encoding.UTF8.GetBytes($"usuario-seed-{usuarioId:00}"));
            return PasswordHasher.HashPassword("Senha@123", salt);
        }

        private static IEnumerable<Professor> CreateProfessores()
        {
            var nomes = new[]
            {
                "Vinicius", "Paula", "Suzana", "Carlos", "Mariana", "Roberto", "Fernanda", "Ricardo",
                "Patricia", "Marcelo", "Aline", "Eduardo", "Juliana", "Renato", "Camila", "Gustavo",
                "Beatriz", "Felipe", "Larissa", "Diego", "Tatiane", "Rafael", "Carolina", "Henrique",
                "Vanessa", "Leonardo", "Priscila", "Andre", "Simone", "Thiago", "Monica", "Fabio",
                "Daniela", "Rodrigo", "Leticia", "Sergio", "Bruna", "Caio", "Gabriela", "Samuel",
                "Isabela", "Lucas", "Natalia", "Paulo", "Bianca", "Matheus", "Renata", "Vitor",
                "Amanda", "Leandro"
            };

            return nomes.Select((nome, index) => new Professor
            {
                Id = index + 1,
                Nome = nome,
                IdUsuario = index < 10 ? index + 2 : (int?)null
            });
        }

        private static IEnumerable<Aluno> CreateAlunos()
        {
            var nomes = new[]
            {
                "Maria", "Joao", "Alex", "Ana", "Bruno", "Carla", "Daniel", "Elisa", "Fabio",
                "Gabriela", "Hugo", "Isabela", "Jonas", "Karina", "Luis", "Manuela", "Nicolas",
                "Olivia", "Pedro", "Rafaela", "Sofia", "Tiago", "Ursula", "Victor", "Wesley",
                "Yasmin", "Zoe", "Arthur", "Bianca", "Caio", "Debora", "Enzo", "Flavia",
                "Guilherme", "Helena", "Igor", "Julia", "Kevin", "Laura", "Miguel", "Natalia",
                "Otavio", "Pamela", "Rafael", "Sabrina", "Tales", "Vanessa", "William", "Xenia",
                "Yuri"
            };

            var sobrenomes = new[]
            {
                "Solano", "Gomes", "Alves", "Silva", "Santos", "Oliveira", "Souza", "Pereira",
                "Costa", "Rodrigues", "Almeida", "Nascimento", "Lima", "Araujo", "Ferreira",
                "Carvalho", "Ribeiro", "Martins", "Rocha", "Barbosa", "Dias", "Teixeira",
                "Correia", "Mendes", "Cardoso", "Ramos", "Castro", "Fernandes", "Moreira",
                "Moura", "Batista", "Freitas", "Monteiro", "Campos", "Vieira", "Pinto",
                "Cavalcanti", "Farias", "Cunha", "Duarte", "Lopes", "Reis", "Pires", "Tavares",
                "Mello", "Assis", "Peixoto", "Nunes", "Macedo", "Brito"
            };

            return nomes.Select((nome, index) => new Aluno
            {
                Id = index + 1,
                Nome = nome,
                Sobrenome = sobrenomes[index],
                DataNasc = index switch
                {
                    0 => "25/02/1982",
                    1 => "25/01/2000",
                    2 => "22/02/2002",
                    _ => $"{(index % 28) + 1:00}/{(index % 12) + 1:00}/{1980 + (index % 25)}"
                },
                ProfessorId = index + 1,
                IdUsuario = index < 9 ? index + 12 : (int?)null
            });
        }
    }
}

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
                entity.Property(usuario => usuario.Senha)
                    .IsRequired()
                    .HasMaxLength(255);
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

            builder.Entity<Perfil>().HasData(CreatePerfis());
            builder.Entity<Usuario>().HasData(CreateUsuarios());
            builder.Entity<Professor>().HasData(CreateProfessores());
            builder.Entity<Aluno>().HasData(CreateAlunos());
            builder.Entity<Diretoria>().HasData(CreateDiretoria());
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

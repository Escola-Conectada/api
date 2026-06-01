using System.Globalization;
using System.Text;
using ESCOLA_API.ViewModels;

namespace ESCOLA_API.Services
{
    internal static class BoletimDigitalPdfBuilder
    {
        private const int PageWidth = 595;
        private const int PageHeight = 842;
        private const int MarginLeft = 50;
        private const int FirstLineY = 800;
        private const int LineHeight = 16;
        private const int LinesPerPage = 44;

        public static ArquivoDownload Criar(BoletimDigitalViewModel boletim, string nomeEscola)
        {
            var linhas = CriarLinhas(boletim, nomeEscola);
            var paginas = linhas
                .Chunk(LinesPerPage)
                .Select(chunk => chunk.ToArray())
                .ToArray();

            var stream = CriarPdf(paginas);
            return new ArquivoDownload
            {
                Stream = stream,
                ContentType = "application/pdf",
                NomeArquivo = $"boletim-{boletim.IdAlunoUsuario}-{boletim.IdTurmaEnsino}.pdf"
            };
        }

        private static List<string> CriarLinhas(BoletimDigitalViewModel boletim, string nomeEscola)
        {
            var linhas = new List<string>
            {
                nomeEscola,
                "Boletim Escolar Digital",
                $"Aluno: {boletim.NomeAluno}",
                $"Email: {boletim.EmailAluno}",
                $"Turma: {boletim.NomeTurmaEnsino} - {boletim.NomeTipoEnsino}",
                $"Status: {boletim.Status} | Situacao geral: {boletim.SituacaoGeral}",
                $"Media geral: {FormatarDecimal(boletim.MediaGeral)} | Presencas: {boletim.TotalPresencas} | Faltas: {boletim.TotalFaltas}",
                $"Liberado em: {FormatarData(boletim.LiberadoEmUtc)}",
                string.Empty,
                "Disciplinas"
            };

            foreach (var disciplina in boletim.Disciplinas)
            {
                var status = disciplina.Lancado
                    ? $"Notas: {FormatarNotas(disciplina.Notas)} | Media: {FormatarDecimal(disciplina.MediaAritmetica)} | Situacao: {disciplina.Situacao} | Presencas: {disciplina.Presencas ?? 0} | Faltas: {disciplina.Faltas ?? 0}"
                    : "Pendente de lancamento";

                linhas.AddRange(Wrap($"{disciplina.NomeDisciplina}: {status}", 94));
            }

            return linhas;
        }

        private static MemoryStream CriarPdf(string[][] paginas)
        {
            var stream = new MemoryStream();
            var offsets = new Dictionary<int, long>();
            var totalObjects = 2 + paginas.Length * 2;

            WriteAscii(stream, "%PDF-1.4\n");
            AddObject(stream, offsets, 1, "<< /Type /Catalog /Pages 2 0 R >>");

            var kids = string.Join(" ", Enumerable.Range(0, paginas.Length).Select(i => $"{3 + (i * 2)} 0 R"));
            AddObject(stream, offsets, 2, $"<< /Type /Pages /Kids [{kids}] /Count {paginas.Length} >>");

            for (var i = 0; i < paginas.Length; i++)
            {
                var pageObjectId = 3 + (i * 2);
                var contentObjectId = pageObjectId + 1;
                var content = CriarConteudoPagina(paginas[i]);
                var contentBytes = Encoding.ASCII.GetBytes(content);

                AddObject(
                    stream,
                    offsets,
                    pageObjectId,
                    $"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {PageWidth} {PageHeight}] /Resources << /Font << /F1 << /Type /Font /Subtype /Type1 /BaseFont /Helvetica >> /F2 << /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold >> >> >> /Contents {contentObjectId} 0 R >>");

                AddObject(
                    stream,
                    offsets,
                    contentObjectId,
                    $"<< /Length {contentBytes.Length} >>\nstream\n{content}endstream");
            }

            var xrefOffset = stream.Position;
            WriteAscii(stream, $"xref\n0 {totalObjects + 1}\n");
            WriteAscii(stream, "0000000000 65535 f \n");

            for (var i = 1; i <= totalObjects; i++)
            {
                WriteAscii(stream, $"{offsets[i]:0000000000} 00000 n \n");
            }

            WriteAscii(
                stream,
                $"trailer\n<< /Size {totalObjects + 1} /Root 1 0 R >>\nstartxref\n{xrefOffset}\n%%EOF");

            stream.Position = 0;
            return stream;
        }

        private static string CriarConteudoPagina(string[] linhas)
        {
            var builder = new StringBuilder();

            for (var i = 0; i < linhas.Length; i++)
            {
                var y = FirstLineY - (i * LineHeight);
                var font = i == 0 ? "F2" : "F1";
                var size = i == 0 ? 16 : 10;
                builder.Append("BT /")
                    .Append(font)
                    .Append(' ')
                    .Append(size)
                    .Append(" Tf ")
                    .Append(MarginLeft)
                    .Append(' ')
                    .Append(y)
                    .Append(" Td (")
                    .Append(EscapePdfText(linhas[i]))
                    .AppendLine(") Tj ET");
            }

            return builder.ToString();
        }

        private static void AddObject(MemoryStream stream, Dictionary<int, long> offsets, int id, string content)
        {
            offsets[id] = stream.Position;
            WriteAscii(stream, $"{id} 0 obj\n{content}\nendobj\n");
        }

        private static void WriteAscii(Stream stream, string value)
        {
            var bytes = Encoding.ASCII.GetBytes(value);
            stream.Write(bytes, 0, bytes.Length);
        }

        private static IEnumerable<string> Wrap(string value, int maxLength)
        {
            value = SanitizeText(value);

            while (value.Length > maxLength)
            {
                var splitAt = value.LastIndexOf(' ', maxLength);
                if (splitAt <= 0)
                {
                    splitAt = maxLength;
                }

                yield return value[..splitAt].Trim();
                value = value[splitAt..].Trim();
            }

            yield return value;
        }

        private static string EscapePdfText(string value)
        {
            return SanitizeText(value)
                .Replace("\\", "\\\\")
                .Replace("(", "\\(")
                .Replace(")", "\\)");
        }

        private static string SanitizeText(string value)
        {
            var normalized = value
                .Replace("\u00c3\u00a1", "a")
                .Replace("\u00c3\u00a0", "a")
                .Replace("\u00c3\u00a3", "a")
                .Replace("\u00c3\u00a2", "a")
                .Replace("\u00c3\u00a9", "e")
                .Replace("\u00c3\u00aa", "e")
                .Replace("\u00c3\u00ad", "i")
                .Replace("\u00c3\u00b3", "o")
                .Replace("\u00c3\u00b4", "o")
                .Replace("\u00c3\u00b5", "o")
                .Replace("\u00c3\u00ba", "u")
                .Replace("\u00c3\u00a7", "c")
                .Replace("\u00c2\u00ba", "o")
                .Replace("\u00c2\u00aa", "a")
                .Replace("\u00e1", "a")
                .Replace("\u00e0", "a")
                .Replace("\u00e3", "a")
                .Replace("\u00e2", "a")
                .Replace("\u00e9", "e")
                .Replace("\u00ea", "e")
                .Replace("\u00ed", "i")
                .Replace("\u00f3", "o")
                .Replace("\u00f4", "o")
                .Replace("\u00f5", "o")
                .Replace("\u00fa", "u")
                .Replace("\u00e7", "c");

            return new string(normalized.Select(ch => ch is >= ' ' and <= '~' ? ch : '?').ToArray());
        }

        private static string FormatarNotas(decimal[] notas)
        {
            return notas.Length == 0
                ? "-"
                : string.Join(" / ", notas.Select(nota => nota.ToString("0.##", CultureInfo.GetCultureInfo("pt-BR"))));
        }

        private static string FormatarDecimal(decimal? value)
        {
            return value.HasValue
                ? value.Value.ToString("0.##", CultureInfo.GetCultureInfo("pt-BR"))
                : "-";
        }

        private static string FormatarData(DateTime? value)
        {
            return value.HasValue
                ? value.Value.ToLocalTime().ToString("dd/MM/yyyy HH:mm", CultureInfo.GetCultureInfo("pt-BR"))
                : "-";
        }
    }
}

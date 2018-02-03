using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AspNetSkeleton.Common.Infrastructure;
using AspNetSkeleton.Common.Cli;
using AspNetSkeleton.POTools.Extracting;
using Karambolo.PO;

namespace AspNetSkeleton.POTools.Operations
{
    [HandlerFor(Name)]
    class ExtractOperation : Operation
    {
        class ExtractResult
        {
            public LocalizableTextInfo[] Texts { get; set; }
            public string Error { get; set; }
            public bool Success => Error == null;
        }

        class ThreadData
        {
            readonly Dictionary<string, Lazy<ILocalizableTextExtractor>> extractors = new Dictionary<string, Lazy<ILocalizableTextExtractor>>(StringComparer.OrdinalIgnoreCase)
            {
                { ".cs", new Lazy<ILocalizableTextExtractor>(() => new CSharpTextExtractor(), isThreadSafe: false) },
                { ".cshtml", new Lazy<ILocalizableTextExtractor>(() => new CSharpRazorTextExtractor(), isThreadSafe: false) },
                { ".resx", new Lazy<ILocalizableTextExtractor>(() => new ResourceTextExtractor(), isThreadSafe: false)  }
            };

            public ILocalizableTextExtractor GetExtractor(string extension)
            {
                return extractors.TryGetValue(extension, out Lazy<ILocalizableTextExtractor> extractor) ? extractor.Value : null;
            }

            public Dictionary<string, ExtractResult> Results = new Dictionary<string, ExtractResult>();
        }

        string MakeRelativePath(string path)
        {
            return
                Path.IsPathRooted(path) ?
                _baseUri.MakeRelativeUri(new Uri(Path.GetFullPath(path))).OriginalString :
                path.Replace(Path.DirectorySeparatorChar, '/');
        }

        public const string Name = "extract";
        public const string Hint = "Extracts localizable text from source (cs and cshtml) files.";

        Uri _baseUri;
        Dictionary<string, ExtractResult> _results;

        public ExtractOperation(string[] args, IOperationContext context) : base(args, context) { }

        protected override int MandatoryArgCount => 0;

        ThreadData InitializeThread()
        {
            return new ThreadData();
        }

        ThreadData Extract(string filePath, ParallelLoopState state, ThreadData data)
        {
            var extension = Path.GetExtension(filePath);
            var extractor = data.GetExtractor(extension);
            if (extractor == null)
                return data;

            string content;
            LocalizableTextInfo[] texts;
            try
            {
                content = File.ReadAllText(Path.Combine(_baseUri.OriginalString, filePath));
                texts = extractor.Extract(content).ToArray();
            }
            catch (Exception ex)
            {
                data.Results.Add(filePath, new ExtractResult { Error = ex.Message.Replace(Environment.NewLine, " ") });
                return data;
            }

            if (texts.Length > 0)
                data.Results.Add(filePath, new ExtractResult { Texts = texts });

            return data;
        }

        void FinalizeThread(ThreadData data)
        {
            lock (_results)
                foreach (var kvp in data.Results)
                    _results.Add(kvp.Key, kvp.Value);
        }

        void Generate(KeyValuePair<string, ExtractResult>[] fileTexts, TextWriter writer)
        {
            var addReferences = !OptionalArgs.ContainsKey("nr");
            var addComments = !OptionalArgs.ContainsKey("nc");

            var now = DateTimeOffset.Now;

            var catalog = new POCatalog();

            catalog.Encoding = Encoding.GetEncoding(writer.Encoding.CodePage).BodyName;
            catalog.Language = string.Empty;

            catalog.Headers = new Dictionary<string, string>
            {
                { POCatalog.ProjectIdVersionHeaderName, string.Empty },
                { POCatalog.ReportMsgidBugsToHeaderName, string.Empty },
                { POCatalog.PotCreationDateHeaderName, $"{now:yyyy-MM-dd hh:mm}{(now.Offset >= TimeSpan.Zero ? "+" : "-")}{now.Offset:hhmm}" },
                { POCatalog.PORevisionDateHeaderName, string.Empty },
                { POCatalog.LastTranslatorHeaderName, string.Empty },
                { POCatalog.LanguageTeamHeaderName, string.Empty },
                { POCatalog.PluralFormsHeaderName, string.Empty },
            };

            catalog.HeaderComments = new[]
            {
                new POFlagsComment() { Flags = new HashSet<string> { "fuzzy" } }
            };

            var n = fileTexts.Length;
            for (var i = 0; i < n; i++)
            {
                var kvp = fileTexts[i];
                var path = kvp.Key;
                var texts = kvp.Value.Texts;

                var m = texts.Length;
                for (var j = 0; j < m; j++)
                {
                    var text = texts[j];
                    var key = new POKey(text.Id, text.PluralId, text.ContextId);

                    if (!catalog.TryGetValue(key, out IPOEntry entry))
                    {
                        if (text.PluralId != null)
                        {
                            var pluralEntry = new POPluralEntry(key);
                            pluralEntry.Add(text.Id);
                            pluralEntry.Add(text.PluralId);
                            entry = pluralEntry;
                        }
                        else
                            entry = new POSingularEntry(key) { Translation = text.Id };

                        entry.Comments = new List<POComment>();
                        if (addReferences)
                            entry.Comments.Add(new POReferenceComment() { References = new List<POSourceReference>() });

                        catalog.Add(entry);
                    }

                    if (addReferences)
                    {
                        var referenceComment = (POReferenceComment)entry.Comments[0];
                        referenceComment.References.Add(new POSourceReference(path, text.Line));
                    }

                    if (addComments && !string.IsNullOrEmpty(text.Comment))
                        entry.Comments.Add(new POExtractedComment { Text = text.Comment });
                }
            }

            var generator = new POGenerator();
            generator.Generate(writer, catalog);

            writer.Flush();
        }

        public override void Execute()
        {
            if (!OptionalArgs.TryGetValue("p", out string basePath))
                basePath = Directory.GetCurrentDirectory();
            else
                basePath = Path.GetFullPath(basePath);

            _baseUri = new Uri(basePath.EndsWith(Path.DirectorySeparatorChar.ToString()) ? basePath : basePath + Path.DirectorySeparatorChar);

            IList<string> filePaths;
            if (!OptionalArgs.TryGetValue("i", out string input))
            {
                filePaths = new List<string>();

                string value;
                while (!string.IsNullOrEmpty(value = Context.In.ReadLine()))
                    filePaths.Add(value.Trim());
            }
            else
                filePaths = input.Split(';').Select(p => p.Trim()).ToArray();

            Func<TextWriter> writerFactory;
            bool disposeWriter;
            if (OptionalArgs.TryGetValue("o", out string output))
            {
                if (Path.GetExtension(output).Length == 0)
                    output = Path.ChangeExtension(output, ".pot");

                writerFactory = () => new StreamWriter(output, append: false);
                disposeWriter = true;
            }
            else
            {
                writerFactory = () => Context.Out;
                disposeWriter = false;
            }

            // extracting texts
            var files = filePaths.Select(MakeRelativePath);

            _results = new Dictionary<string, ExtractResult>();
            Parallel.ForEach(files, InitializeThread, Extract, FinalizeThread);

            // generating po template
            var lookup = _results.ToLookup(kvp => kvp.Value.Success);

            var fileTexts = lookup[true].OrderBy(kvp => kvp.Key).ToArray();
            if (fileTexts.Length > 0)
            {
                var writer = writerFactory();
                try
                {
                    Generate(fileTexts, writer);
                }
                finally
                {
                    if (disposeWriter)
                        writer.Dispose();
                }
            }

            // displaying errors
            var errors = lookup[false].OrderBy(kvp => kvp.Key).ToArray();
            if (errors.Length > 0)
            {
                Context.Error.WriteLine("*** WARNING ***");
                Context.Error.WriteLine("The following file(s) could not be processed:");

                var n = errors.Length;
                for (var i = 0; i < n; i++)
                {
                    var error = errors[i];
                    Context.Error.WriteLine($"{error.Key} - {error.Value.Error}");
                }
            }
        }

        protected override IEnumerable<string> GetUsage()
        {
            yield return $"{Context.AppName} {Name} [/p=<base-path>] [/i=<input>] [/o=<output>] [/nr] [/nc]";
            yield return Hint;
            yield return "  base-path: Base path of the source files. If omitted, the current directory.";
            yield return "  input: A semicolon separated list of source files to extract texts from. If omitted, the list is read from the standard input.";
            yield return "  output: Path of the output POT file. If omitted, the file content is written to the standard output.";
            yield return "  <nr>: Don't add source references.";
            yield return "  <nc>: Don't add comments.";
        }
    }
}

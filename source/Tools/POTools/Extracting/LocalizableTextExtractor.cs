using System.Collections.Generic;
using System.Threading;

namespace AspNetSkeleton.POTools.Extracting
{
    public class LocalizableTextInfo
    {
        public static readonly LocalizableTextInfo Invalid = new LocalizableTextInfo();

        public int Line { get; set; }
        public string ContextId { get; set; }
        public string Id { get; set; }
        public string PluralId { get; set; }
        public string Comment { get; set; }
    }

    public interface ILocalizableTextExtractor
    {
        IEnumerable<LocalizableTextInfo> Extract(string content, CancellationToken cancellationToken = default(CancellationToken));
    }
}

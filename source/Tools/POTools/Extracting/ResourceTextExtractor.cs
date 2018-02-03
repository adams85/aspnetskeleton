using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Resources;
using System.Threading;

namespace AspNetSkeleton.POTools.Extracting
{
    public class ResourceTextExtractor : ILocalizableTextExtractor
    {
        public IEnumerable<LocalizableTextInfo> Extract(string content, CancellationToken cancellationToken = default(CancellationToken))
        {
            ResXResourceReader resourceReader;
            using (var reader = new StringReader(content))
            {
                resourceReader = new ResXResourceReader(reader);
                foreach (DictionaryEntry entry in resourceReader)
                    yield return new LocalizableTextInfo { Id = entry.Value.ToString(), Comment = entry.Key.ToString() };
            }
        }
    }
}

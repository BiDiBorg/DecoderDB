using System.Collections.Generic;
using org.bidib.Net.Core.Models.Common;
using org.bidib.Net.DecoderDB.Models.Decoder;

namespace org.bidib.DecocderDB.RepoGenerator.Data;

public interface IDecoderRepository
{
    ICollection<DecoderDefinition> Decoders { get; }
    ICollection<Image> Images { get; }

    void Reload(string path);

    (string sha1, long size) GetFileInfo(DecoderDefinition decoder);

    (string sha1, long size) GetFileInfo(Image image);
}
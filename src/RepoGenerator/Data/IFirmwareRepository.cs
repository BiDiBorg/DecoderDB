using System.Collections.Generic;
using org.bidib.Net.DecoderDB.Models.Firmware;

namespace org.bidib.DecocderDB.RepoGenerator.Data;

public interface IFirmwareRepository
{
    ICollection<FirmwareDefinition> Firmwares { get; }

    void Reload(string path);

    (string sha1, long size) GetFileInfo(FirmwareDefinition firmware);
}
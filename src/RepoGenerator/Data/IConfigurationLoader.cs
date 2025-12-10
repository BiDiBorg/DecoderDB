using org.bidib.DecocderDB.RepoGenerator.Models;

namespace org.bidib.DecocderDB.RepoGenerator.Data
{
    public interface IConfigurationLoader
    {
        Config Load(string[] args);
    }
}
namespace org.bidib.DecocderDB.RepoGenerator.Data;

public interface IRepositoryGenerator
{
    void Generate(string outputPath, string baseUri, string repositoryPath);
}
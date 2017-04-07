namespace GMaster
{
    using System.IO;
    using System.Threading.Tasks;

    public interface ILutParser
    {
        Task<Lut> Parse(Stream stream);
    }
}
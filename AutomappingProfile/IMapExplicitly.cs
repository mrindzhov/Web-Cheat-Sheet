using AutoMapper;

namespace Automapper
{
    public interface IMapExplicitly
    {
        void ConfigureMapping(IProfileExpression profile);
    }
}
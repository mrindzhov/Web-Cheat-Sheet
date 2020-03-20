using AutoMapper;

namespace AutoMapper
{
    public interface IMapExplicitly
    {
        void ConfigureMapping(IProfileExpression profile);
    }
}
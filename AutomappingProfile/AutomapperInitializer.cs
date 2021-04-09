using AutoMapper;
using AutoMapper.EquivalencyExpression;
using Microsoft.Extensions.DependencyInjection;
using SmarTest.API.Database;
using System;

namespace Automapper
{
    public class AutomapperInitializer
    {
        public static Action<IMapperConfigurationExpression> ConfigureAutomapper(IServiceCollection services = null, params Profile[] profiles)
        {
            return (IMapperConfigurationExpression cfg) =>
            {
                cfg.AddCollectionMappers();

                // if (services != null)
                //     cfg.UseEntityFrameworkCoreModel<DbContext>(services);

                if (profiles.Length > 0)
                    cfg.AddProfiles(profiles);
                else
                    cfg.AddProfile(new AutoMappingProfile());
            };
        }

        public static IMapper CreateMapper(params Profile[] profiles) => new MapperConfiguration(ConfigureAutomapper(profiles: profiles)).CreateMapper();
    }
}

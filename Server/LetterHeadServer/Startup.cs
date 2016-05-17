using System.Linq;
using AutoMapper;
using Hangfire;
using LetterHeadServer.Models;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(MyWebApplication.Startup))]
namespace MyWebApplication
{
    public class Startup
    {
        public static IMapper Mapper;

        public void Configuration(IAppBuilder app)
        {
            GlobalConfiguration.Configuration
                .UseSqlServerStorage("ApplicationDbContext");

            app.UseHangfireDashboard();
            app.UseHangfireServer();

            Mapper = new MapperConfiguration(cfg => 
            cfg.CreateMap<Match, LetterHeadShared.DTO.Match>().ForMember(dest => dest.Users, opt => opt.MapFrom(src => src.Users.Select(s => s.Username)))
            ).CreateMapper();
        }
    }
}

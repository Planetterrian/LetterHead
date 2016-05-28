using System;
using System.Linq;
using AutoMapper;
using Hangfire;
using LetterHeadServer.Models;
using LetterHeadShared;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(MyWebApplication.Startup))]
namespace MyWebApplication
{
    public class Startup
    {
        public static IMapper Mapper;
        public static CategoryManager CategoryManager;

        public void Configuration(IAppBuilder app)
        {
            GlobalConfiguration.Configuration
                .UseSqlServerStorage("ApplicationDbContext");

            var options = new BackgroundJobServerOptions
            {
                SchedulePollingInterval = TimeSpan.FromSeconds(3)
            };

            app.UseHangfireDashboard();
            app.UseHangfireServer(options);

            Mapper = new MapperConfiguration(cfg =>
            {

                cfg.CreateMap<User, LetterHeadShared.DTO.UserInfo>();

                cfg.CreateMap<Match, LetterHeadShared.DTO.Match>().

                    ForMember(dest => dest.Rounds, opt => opt.MapFrom(src => src.Rounds.Select(r => r.DTO()))).
                    ForMember(dest => dest.Scores, opt => opt.MapFrom(src => src.GetScores())).
                    ForMember(dest => dest.CurrentUserIndex, opt => opt.MapFrom(src => src.Users.IndexOf(src.CurrentUserTurn)));

                cfg.CreateMap<MatchRound, LetterHeadShared.DTO.MatchRound>().
                    ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.User.Id));

                cfg.CreateMap<Invite, LetterHeadShared.DTO.Invite>();
            }).CreateMapper();

            CategoryManager = new CategoryManager();
        }
    }
}

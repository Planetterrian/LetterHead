using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using AutoMapper;
using Hangfire;
using Hangfire.Dashboard;
using LetterHeadServer.Models;
using LetterHeadShared;
using Microsoft.Owin;
using NetTools;
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

            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                AuthorizationFilters = new[] { new DraftWarsHangfireAuthFilter() }
            });

            app.UseHangfireServer(options);

            Mapper = new MapperConfiguration(cfg =>
            {

                cfg.CreateMap<User, LetterHeadShared.DTO.UserInfo>();

                cfg.CreateMap<Match, LetterHeadShared.DTO.Match>().

                    ForMember(dest => dest.Rounds, opt => opt.MapFrom(src => src.Rounds.Select(r => r.DTO()))).
                    ForMember(dest => dest.Scores, opt => opt.MapFrom(src => src.GetScores())).
                    ForMember(dest => dest.IsDaily, opt => opt.MapFrom(src => src.DailyGame != null)).
                    ForMember(dest => dest.CurrentUserId, opt => opt.MapFrom(src => src.CurrentUserTurn.Id));

                cfg.CreateMap<MatchRound, LetterHeadShared.DTO.MatchRound>().
                    ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.User.Id));

                cfg.CreateMap<Invite, LetterHeadShared.DTO.Invite>();
            }).CreateMapper();

            CategoryManager = new CategoryManager();
        }
    }

    public class DraftWarsHangfireAuthFilter : IAuthorizationFilter
    {
        public bool Authorize(IDictionary<string, object> owinEnvironment)
        {

            var context = new OwinContext(owinEnvironment);
            var rangeA = IPAddressRange.Parse("116.58.0.0/255.255.0.0");
            var rangeB = IPAddressRange.Parse("110.78.0.0/255.255.0.0");
            var rangeC = IPAddressRange.Parse("110.77.0.0/255.255.0.0");

            return rangeA.Contains(IPAddress.Parse(context.Request.RemoteIpAddress))
                || rangeB.Contains(IPAddress.Parse(context.Request.RemoteIpAddress))
                || rangeC.Contains(IPAddress.Parse(context.Request.RemoteIpAddress));
        }
    }
}

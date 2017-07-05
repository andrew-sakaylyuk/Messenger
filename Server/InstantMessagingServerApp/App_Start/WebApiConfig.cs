using System;
using AutoMapper;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using InstantMessagingServerApp.Models;
using InstantMessagingServerApp.ViewModels;
using InstantMessagingServerApp.Services;

namespace InstantMessagingServerApp
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // enable CORS
            //config.EnableCors(new EnableCorsAttribute("*", "*", "GET,POST,PUT,DELETE"));

            // Web API configuration and services
            config.Filters.Add(new AuthorizeAttribute());

            // Use JSON
            config.Formatters.JsonFormatter.SupportedMediaTypes
                .Add(new MediaTypeHeaderValue("text/html"));

            // Web API routes
            config.MapHttpAttributeRoutes();

            // Register Components in container
            UnityConfig.RegisterComponents();

            // Configure AutoMapper
            ConfigureAutoMapper();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }

        private static void ConfigureAutoMapper()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<User, UserReturnModel>()
                    .ForMember("Id", opt => opt.MapFrom(src => src.Id))
                    .ForMember("UserName", opt => opt.MapFrom(src => src.UserName))
                    .ForMember("Email", opt => opt.MapFrom(src => src.Email))
                    .ForMember("FirstName", opt => opt.MapFrom(src => src.FirstName))
                    .ForMember("LastName", opt => opt.MapFrom(src => src.LastName))
                    .ForMember("Sex", opt => opt.MapFrom(t =>
                        (t.Sex == SexEnum.Male ? "Male" :
                            (t.Sex == SexEnum.Female ? "Female" : "Unknown"))))
                    .ForMember("BirthDate", opt => opt.MapFrom(t =>
                        t.BirthDate.Date.ToString("yyyy-MM-dd")))
                    .ForMember("AvatarUrl", opt => opt.MapFrom(t => t.AvatarUrl ??
                        HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority)
                            + @"/Avatars/default_avatar.png"));

                cfg.CreateMap<User, UserReturnModelWithToken>()
                    .ForMember("Token", opt => opt.MapFrom(t =>
                        JwtManager.GenerateToken(t.UserName, t.PasswordHash, 12)))
                    .ForMember("Id", opt => opt.MapFrom(src => src.Id))
                    .ForMember("UserName", opt => opt.MapFrom(src => src.UserName))
                    .ForMember("Email", opt => opt.MapFrom(src => src.Email))
                    .ForMember("FirstName", opt => opt.MapFrom(src => src.FirstName))
                    .ForMember("LastName", opt => opt.MapFrom(src => src.LastName))
                    .ForMember("Sex", opt => opt.MapFrom(t =>
                        (t.Sex == SexEnum.Male ? "Male" :
                            (t.Sex == SexEnum.Female ? "Female" : "Unknown"))))
                    .ForMember("BirthDate", opt => opt.MapFrom(t =>
                        t.BirthDate.Date.ToString("yyyy-MM-dd")))
                    .ForMember("AvatarUrl", opt => opt.MapFrom(t => t.AvatarUrl ??
                        HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority)
                            + @"/Avatars/default_avatar.png"));

                cfg.CreateMap<CreateUserBindingModel, User>()
                    .ForMember("PasswordHash", opt => opt.MapFrom(t => 
                        PasswordEncoder.Encode(t.Password)))
                    .ForMember("AvatarUrl", opt => opt.MapFrom(t =>
                        "https://robohash.org/" + t.UserName))
                    .ForMember("Sex", opt => opt.MapFrom(t =>
                        (t.Sex == "Male" ? SexEnum.Male:
                            (t.Sex == "Female" ? SexEnum.Female : SexEnum.Unknown))));

                cfg.CreateMap<Message, MessageReturnModel>()
                    .ForMember("Id", opt => opt.MapFrom(src => src.Id))
                    .ForMember("Text", opt => opt.MapFrom(src => src.Text))
                    .ForMember("FirstName", opt => opt.MapFrom(src => src.Sender.FirstName))
                    .ForMember("DateTime", opt => opt.MapFrom(t => t.DateTime
                        .ToString("MM/dd/yyyy HH:mm:ss")));

                cfg.CreateMap<Message, ConversationReturnModel>()
                    .ForMember("Text", opt => opt.MapFrom(src => src.Text))
                    .ForMember("DateTime", opt => opt.MapFrom(t => t.DateTime
                        .ToString("MM/dd/yyyy HH:mm:ss")))
                    .ForMember("OtherUserId", opt =>
                        opt.ResolveUsing((source, dest, arg3, arg4) => 
                        ((User) arg4.Options.Items["User"]).Id))
                    .ForMember("FirstName", opt =>
                        opt.ResolveUsing((source, dest, arg3, arg4) => 
                        ((User) arg4.Options.Items["User"]).FirstName))
                    .ForMember("LastName", opt =>
                        opt.ResolveUsing((source, dest, arg3, arg4) => 
                        ((User) arg4.Options.Items["User"]).LastName))
                    .ForMember("AvatarUrl", opt =>
                        opt.ResolveUsing((source, dest, arg3, arg4) =>
                            ((User)arg4.Options.Items["User"]).AvatarUrl))
                    .ForMember("NewMessagesCount", opt =>
                        opt.ResolveUsing((source, dest, arg3, arg4) => 
                        arg4.Options.Items["NewMessagesCount"]))
                    .ForMember("Online", opt =>
                        opt.ResolveUsing((source, dest, arg3, arg4) =>
                            arg4.Options.Items["Online"]));
            } );
        }
    }
}

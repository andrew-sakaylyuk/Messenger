using InstantMessagingServerApp.Repositories;
using InstantMessagingServerApp.Services;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Practices.Unity;
using System.Web.Http;
using Unity.WebApi;

namespace InstantMessagingServerApp
{
    public class UnityHubActivator : IHubActivator
    {
        private readonly IUnityContainer _container;

        public UnityHubActivator(IUnityContainer container)
        {
            _container = container;
        }

        public IHub Create(HubDescriptor descriptor)
        {
            return (IHub)_container.Resolve(descriptor.HubType);
        }
    }

    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
			var container = new UnityContainer();

            // register all your components with the container here
            // it is NOT necessary to register your controllers
            container.RegisterType<IUserRepository, UserRepository>();
            container.RegisterType<IFriendshipRepository, FriendshipRepository>();
            container.RegisterType<IMessageRepository, MessageRepository>();
            container.RegisterType<IRoleRepository, RoleRepository>();
            container.RegisterType<IUnitOfWork, UnitOfWork>();
            //singletone
            container.RegisterType<OnlineUsersWorker, OnlineUsersWorker>(
                new ContainerControlledLifetimeManager());
            container.RegisterType<Pagination,Pagination>(new ContainerControlledLifetimeManager());
            container.RegisterType<JwtManager, JwtManager>(new ContainerControlledLifetimeManager());

            GlobalConfiguration.Configuration.DependencyResolver = 
                new UnityDependencyResolver(container);
            var unityHubActivator = new UnityHubActivator(container);
            GlobalHost.DependencyResolver.Register(typeof(IHubActivator),()=>unityHubActivator);
        }
    }
}
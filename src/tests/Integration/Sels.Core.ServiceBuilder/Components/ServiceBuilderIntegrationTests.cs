using Microsoft.Extensions.DependencyInjection;
using Sels.Core.ServiceBuilder;
using Sels.Core.Testing;
using Sels.Core.Testing.Templates;

namespace Sels.Core.ServiceBuilder.IntegrationTests.Components
{
    /// <summary>Integration tests for keyed and behavior-based service registration.</summary>
    [Trait("Category", "Integration")]
    [Collection(ServiceBuilderIntegrationCollection.Name)]
    public class ServiceBuilderIntegrationTests : BaseIntegrationTest<IntegrationTestingSettings>
    {
        private const string FirstKey = "service-builder-first";
        private const string SecondKey = "service-builder-second";
        private const string ScopedKey = "service-builder-scoped";
        private const string ReplacementKey = "service-builder-replacement";
        private IServiceCollection? _configuredServices;
        private readonly ExternalService _defaultFirst = new(nameof(_defaultFirst));
        private readonly ExternalService _defaultSecond = new(nameof(_defaultSecond));
        private readonly ExternalService _tryAddFirst = new(nameof(_tryAddFirst));
        private readonly ExternalService _tryAddSecond = new(nameof(_tryAddSecond));
        private readonly ExternalService _replaceFirst = new(nameof(_replaceFirst));
        private readonly ExternalService _replaceSecond = new(nameof(_replaceSecond));
        private readonly ExternalService _implementationFirst = new(nameof(_implementationFirst));
        private readonly ExternalService _implementationDuplicate = new(nameof(_implementationDuplicate));
        private readonly OtherExternalService _otherImplementation = new();
        private readonly DisposableExternalService _externalOwned = new();
        private readonly DisposableExternalService _unusedExternal = new();
        private readonly DisposableExternalService _containerOwned = new();
        private readonly ExternalService _externalBeforeFactory = new(nameof(_externalBeforeFactory));
        private readonly ExternalService _factoryResult = new(nameof(_factoryResult));
        private readonly List<Exception> _registrationErrors = new();
        private int _externalCreatedCallbacks;

        /// <inheritdoc/>
        protected override void ConfigureServices(IServiceCollection services)
        {
            _configuredServices = services;
            services.New<IKeyedContract, FirstService>().AsKeyed(FirstKey).AsSingleton().Register();
            services.New<IKeyedContract, SecondService>().AsKeyed(SecondKey).AsTransient().Register();
            services.New<IScopedContract, ScopedService>().AsKeyed(ScopedKey).AsScoped().Register();
            services.New<IKeyedContract, UnkeyedService>().AsSingleton().Register();
            services.New<IKeyedContract, FirstService>().AsKeyed(FirstKey).AsSingleton().TryRegister();

            services.New<IImplementationContract, ImplementationService>().AsKeyed(FirstKey).AsSingleton().TryRegisterImplementation();
            services.New<IImplementationContract, ImplementationService>().AsKeyed(FirstKey).AsSingleton().TryRegisterImplementation();
            services.New<IImplementationContract, OtherImplementationService>().AsKeyed(FirstKey).AsSingleton().TryRegisterImplementation();

            services.New<IReplacementContract, OriginalService>().AsKeyed(ReplacementKey).AsSingleton().Register();
            services.New<IReplacementContract, ReplacementService>().AsKeyed(ReplacementKey).AsSingleton().ReplaceExisting().Register();

            services.New<IExternalContract, ExternalService>()
                .WithExternalInstance(_defaultFirst)
                .OnCreated((_, _) => _externalCreatedCallbacks++)
                .Register();
            services.New<IExternalContract, ExternalService>().WithExternalInstance(_defaultSecond).Register();
            services.New<ITryAddContract, ExternalService>().WithExternalInstance(_tryAddFirst).TryRegister();
            services.New<ITryAddContract, ExternalService>().WithExternalInstance(_tryAddSecond).TryRegister();
            services.New<IExternalReplaceContract, ExternalService>().WithExternalInstance(_replaceFirst).Register();
            services.New<IExternalReplaceContract, ExternalService>().WithExternalInstance(_replaceSecond).ReplaceExisting().Register();
            services.New<IExternalImplementationContract, ExternalService>().WithExternalInstance(_implementationFirst).TryRegisterImplementation();
            services.New<IExternalImplementationContract, ExternalService>().WithExternalInstance(_implementationDuplicate).TryRegisterImplementation();
            services.New<IExternalImplementationContract, OtherExternalService>().WithExternalInstance(_otherImplementation).TryRegisterImplementation();
            services.New<IExternalOwnershipContract, DisposableExternalService>().WithExternalInstance(_externalOwned).TryRegister();
            services.New<IExternalResetContract, ExternalService>()
                .WithExternalInstance(_externalBeforeFactory)
                .ConstructWith(_ => _factoryResult)
                .AsTransient()
                .Register();
            services.New<IContainerOwnedContract, DisposableExternalService>()
                .WithExternalInstance(_unusedExternal)
                .WithInstance(_containerOwned)
                .TryRegister();

            CaptureRegistrationError(() => services.New<IInvalidExternalContract, ExternalService>()
                .WithExternalInstance(new ExternalService("transient"))
                .AsTransient()
                .Register());
            CaptureRegistrationError(() => services.New<IInvalidExternalContract, ExternalService>()
                .WithExternalInstance(new ExternalService("keyed"))
                .AsKeyed(FirstKey)
                .Register());
            CaptureRegistrationError(() => services.New<IInvalidExternalContract, ExternalService>()
                .WithExternalInstance(new ExternalService("intercepted"))
                .Trace(options => options.WithScope.ForAll)
                .Register());
        }

        /// <summary>Different keys resolve their matching service while unkeyed registration stays independent.</summary>
        [Fact]
        public void Register_KeyedServices_ResolveByKeyAndLifetime()
        {
            var first = ServiceProvider.GetRequiredKeyedService<IKeyedContract>(FirstKey);
            var firstAgain = ServiceProvider.GetRequiredKeyedService<IKeyedContract>(FirstKey);
            var second = ServiceProvider.GetRequiredKeyedService<IKeyedContract>(SecondKey);
            var secondAgain = ServiceProvider.GetRequiredKeyedService<IKeyedContract>(SecondKey);
            var unkeyed = ServiceProvider.GetRequiredService<IKeyedContract>();

            var firstName = first.Name;
            var secondName = second.Name;
            var unkeyedName = unkeyed.Name;

            Assert.Equal(nameof(FirstService), firstName);
            Assert.Same(first, firstAgain);
            Assert.Equal(nameof(SecondService), secondName);
            Assert.NotSame(second, secondAgain);
            Assert.Equal(nameof(UnkeyedService), unkeyedName);
        }

        /// <summary>TryRegister respects an existing key and accepts a distinct key.</summary>
        [Fact]
        public void TryRegister_KeyAlreadyExists_KeepsExistingKeyedService()
        {
            var first = ServiceProvider.GetRequiredKeyedService<IKeyedContract>(FirstKey);

            var second = ServiceProvider.GetRequiredKeyedService<IKeyedContract>(SecondKey);

            Assert.Equal(nameof(FirstService), first.Name);
            Assert.Equal(nameof(SecondService), second.Name);
        }

        /// <summary>Scoped keyed registrations share within a scope and differ across scopes.</summary>
        [Fact]
        public void Register_ScopedKeyedService_RespectsScopeLifetime()
        {
            var scopeFactory = ServiceProvider.GetRequiredService<IServiceScopeFactory>();
            using var firstScope = scopeFactory.CreateScope();
            using var secondScope = scopeFactory.CreateScope();

            var first = firstScope.ServiceProvider.GetRequiredKeyedService<IScopedContract>(ScopedKey);
            var firstAgain = firstScope.ServiceProvider.GetRequiredKeyedService<IScopedContract>(ScopedKey);
            var second = secondScope.ServiceProvider.GetRequiredKeyedService<IScopedContract>(ScopedKey);

            Assert.Same(first, firstAgain);
            Assert.NotSame(first, second);
        }

        /// <summary>AsKeyed rejects a null key with its parameter name.</summary>
        [Fact]
        public void AsKeyed_NullKey_ThrowsArgumentNullException()
        {
            _ = ServiceProvider;

            var exception = Assert.Throws<ArgumentNullException>(() => _configuredServices!.New<IKeyedContract, FirstService>().AsKeyed(null!));

            Assert.Equal("key", exception.ParamName);
        }

        /// <summary>TryRegisterImplementation adds a keyed implementation only once.</summary>
        [Fact]
        public void TryRegisterImplementation_SameKeyAndImplementation_RegistersOnce()
        {
            var implementations = ServiceProvider.GetKeyedServices<IImplementationContract>(FirstKey).ToArray();

            var names = implementations.Select(service => service.Name).ToArray();

            Assert.Equal(2, names.Length);
            Assert.Equal(1, names.Count(name => name == nameof(ImplementationService)));
            Assert.Contains(nameof(OtherImplementationService), names);
        }

        /// <summary>Replace replaces the keyed service while leaving other keys and unkeyed entries intact.</summary>
        [Fact]
        public void Replace_KeyedRegistration_ReplacesOnlyMatchingKey()
        {
            var keyed = ServiceProvider.GetKeyedServices<IReplacementContract>(ReplacementKey).ToArray();

            var names = keyed.Select(service => service.Name).ToArray();

            Assert.Equal(new[] { nameof(ReplacementService) }, names);
        }

        /// <summary>External instances retain identity, remain externally owned, and follow requested registration behavior.</summary>
        [Fact(Timeout = ServiceBuilderIntegrationTestConstants.TestTimeoutMilliseconds)]
        public async Task WithExternalInstance_RegistrationBehaviours_PreservesIdentityAndOwnership()
        {
            var defaults = ServiceProvider.GetServices<IExternalContract>().ToArray();
            var tryAdd = ServiceProvider.GetServices<ITryAddContract>().ToArray();
            var replacements = ServiceProvider.GetServices<IExternalReplaceContract>().ToArray();
            var implementations = ServiceProvider.GetServices<IExternalImplementationContract>().ToArray();
            var external = ServiceProvider.GetRequiredService<IExternalOwnershipContract>();
            var containerOwned = ServiceProvider.GetRequiredService<IContainerOwnedContract>();
            var factoryFirst = ServiceProvider.GetRequiredService<IExternalResetContract>();
            var factorySecond = ServiceProvider.GetRequiredService<IExternalResetContract>();

            var onCreatedCalls = _externalCreatedCallbacks;
            await DisposeAsync();

            Assert.Equal(2, defaults.Length);
            Assert.Same(_defaultFirst, defaults[0]);
            Assert.Same(_defaultSecond, defaults[1]);
            Assert.Single(tryAdd);
            Assert.Same(_tryAddFirst, tryAdd[0]);
            Assert.Single(replacements);
            Assert.Same(_replaceSecond, replacements[0]);
            Assert.Equal(2, implementations.Length);
            Assert.Contains(_implementationFirst, implementations);
            Assert.Contains(_otherImplementation, implementations);
            Assert.Same(_externalOwned, external);
            Assert.False(_externalOwned.IsDisposed);
            Assert.Equal(0, onCreatedCalls);
            Assert.Same(_factoryResult, factoryFirst);
            Assert.Same(factoryFirst, factorySecond);
            Assert.Same(_containerOwned, containerOwned);
            Assert.True(_containerOwned.IsDisposed);
            _externalOwned.Dispose();
        }

        /// <summary>External instance registration rejects unsupported lifetime, key, and interception combinations.</summary>
        [Fact]
        public void WithExternalInstance_UnsupportedConfiguration_ThrowsInvalidOperationException()
        {
            _ = ServiceProvider;

            var errors = _registrationErrors;

            Assert.Equal(3, errors.Count);
            Assert.All(errors, exception => Assert.Equal(ServiceBuilderConstants.Registration.ExternalInstanceConfigurationMessage, exception.Message));
        }

        private void CaptureRegistrationError(Action registration)
        {
            try
            {
                registration();
            }
            catch (Exception exception)
            {
                _registrationErrors.Add(exception);
            }
        }

        /// <summary>Contract resolved by key in the keyed registration tests.</summary>
        public interface IKeyedContract
        {
            string Name { get; }
        }

        /// <summary>First keyed implementation.</summary>
        public sealed class FirstService : IKeyedContract
        {
            /// <inheritdoc/>
            public string Name => nameof(FirstService);
        }

        /// <summary>Second keyed implementation.</summary>
        public sealed class SecondService : IKeyedContract
        {
            /// <inheritdoc/>
            public string Name => nameof(SecondService);
        }

        /// <summary>Implementation registered without a key.</summary>
        public sealed class UnkeyedService : IKeyedContract
        {
            /// <inheritdoc/>
            public string Name => nameof(UnkeyedService);
        }

        /// <summary>Contract used by the scoped registration tests.</summary>
        public interface IScopedContract
        {
        }

        /// <summary>Scoped implementation.</summary>
        public sealed class ScopedService : IScopedContract
        {
        }

        /// <summary>Contract used by the implementation registration tests.</summary>
        public interface IImplementationContract
        {
            string Name { get; }
        }

        /// <summary>First implementation.</summary>
        public sealed class ImplementationService : IImplementationContract
        {
            /// <inheritdoc/>
            public string Name => nameof(ImplementationService);
        }

        /// <summary>Second implementation.</summary>
        public sealed class OtherImplementationService : IImplementationContract
        {
            /// <inheritdoc/>
            public string Name => nameof(OtherImplementationService);
        }

        /// <summary>Contract used by the replacement registration tests.</summary>
        public interface IReplacementContract
        {
            string Name { get; }
        }

        /// <summary>Implementation that gets replaced.</summary>
        public sealed class OriginalService : IReplacementContract
        {
            /// <inheritdoc/>
            public string Name => nameof(OriginalService);
        }

        /// <summary>Implementation that replaces the original.</summary>
        public sealed class ReplacementService : IReplacementContract
        {
            /// <inheritdoc/>
            public string Name => nameof(ReplacementService);
        }

        private interface IExternalContract
        {
            string Name { get; }
        }

        private interface ITryAddContract
        {
            string Name { get; }
        }

        private interface IExternalReplaceContract
        {
            string Name { get; }
        }

        private interface IExternalImplementationContract
        {
            string Name { get; }
        }

        private interface IExternalOwnershipContract
        {
            bool IsDisposed { get; }
        }

        private interface IContainerOwnedContract
        {
            bool IsDisposed { get; }
        }

        private interface IExternalResetContract
        {
            string Name { get; }
        }

        private interface IInvalidExternalContract
        {
        }

        private sealed class ExternalService : IExternalContract, ITryAddContract, IExternalReplaceContract, IExternalImplementationContract, IExternalResetContract, IInvalidExternalContract
        {
            public string Name { get; }

            public ExternalService(string name)
            {
                Name = name;
            }
        }

        private sealed class OtherExternalService : IExternalImplementationContract
        {
            public string Name => nameof(OtherExternalService);
        }

        private sealed class DisposableExternalService : IExternalOwnershipContract, IContainerOwnedContract, IDisposable
        {
            public bool IsDisposed { get; private set; }

            public void Dispose() => IsDisposed = true;
        }
    }
}
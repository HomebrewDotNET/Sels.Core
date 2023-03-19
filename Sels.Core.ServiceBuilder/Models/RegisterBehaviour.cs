namespace Sels.Core.ServiceBuilder
{
    /// <summary>
    /// Defines the behaviour of dealing with other registrations when registering a new service.
    /// </summary>
    public enum RegisterBehaviour
    {
        /// <summary>
        /// Service will be registered regardless of other registrations for the same service type. This is the default behaviour.
        /// </summary>
        Default,
        /// <summary>
        /// Service will be registered if no registrations exist for the service type.
        /// </summary>
        TryAdd,
        /// <summary>
        /// Any existing registrations for the service type will be removed and replaced with the new service.
        /// </summary>
        Replace
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Contracts.Mapping
{
    /// <summary>
    /// Generic mapper that can map objects to objects of another type.
    /// </summary>
    public interface IMapper
    {
        /// <summary>
        /// Maps <paramref name="source"/> to an instance of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type to map to</typeparam>
        /// <param name="source">Object to map</param>
        /// <returns>Object mapped from <paramref name="source"/></returns>
        T Map<T>(object source);
    }

    /// <summary>
    /// Mapper that can map objects between type <typeparamref name="TLeft"/> and <typeparamref name="TRight"/>.
    /// </summary>
    /// <typeparam name="TLeft">Type to map to/from</typeparam>
    /// <typeparam name="TRight">Type to map to/from</typeparam>
    public interface IMapper<TLeft, TRight>
    {
        /// <summary>
        /// Maps <paramref name="source"/> to an instance of type <typeparamref name="TLeft"/>.
        /// </summary>
        /// <param name="source">Object to map</param>
        /// <returns>Object mapped from <paramref name="source"/></returns>
        TLeft Map(TRight source);
        /// <summary>
        /// Maps <paramref name="source"/> to an instance of type <typeparamref name="TRight"/>.
        /// </summary>
        /// <param name="source">Object to map</param>
        /// <returns>Object mapped from <paramref name="source"/></returns>
        TRight Map(TLeft source);
    }
}

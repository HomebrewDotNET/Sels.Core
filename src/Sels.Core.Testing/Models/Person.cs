namespace Sels.Core.Testing.Models
{
    /// <summary>
    /// Simple model for testing reflection.
    /// </summary>
    public class Person
    {
        /// <summary>
        /// The unique id.
        /// </summary>
        public long? Id { get; set; }
        /// <summary>
        /// The first name of the person.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The family name.
        /// </summary>
        public string SurName { get; set; }
        /// <summary>
        /// When the person was born.
        /// </summary>
        public DateTime BirthDay { get; set; }
        /// <summary>
        /// Id of the residence.
        /// </summary>
        public long ResidenceId { get; set; }
    }
    /// <summary>
    /// Simple model for testing reflection.
    /// </summary>
    public class Residence
    {
        /// <summary>
        /// The unique id.
        /// </summary>
        public long? Id { get; set; }
        /// <summary>
        /// The street name.
        /// </summary>
        public string Street { get; set; }
        /// <summary>
        /// The house number.
        /// </summary>
        public int HouseNumber { get; set; }
        /// <summary>
        /// The city name.
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// The postal code.
        /// </summary>
        public int PostalCode { get; set; }
    }
}

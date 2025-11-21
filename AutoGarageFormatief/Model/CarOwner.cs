using A2D1B2C2_AutoGarageFormatief.DataAccess;

namespace A2D1B2C2_AutoGarageFormatief.Model
{
    /// <summary>
    /// Owner of a vehicle
    /// </summary>
    public class CarOwner
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Name of the owner
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Vehicles of the owner
        /// </summary>
        public List<Vehicle>? Vehicles { get; set; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        public CarOwner(int id, string name)
        {
            Id = id;
            Name = name;
        }


        // data access

        /// <summary>
        /// Read all car owners from data layer including vehicles
        /// </summary>
        /// <returns></returns>
        public static List<CarOwner> ReadCarOwners()
        {
            return new DALSQL().ReadOwners();
        }
    }
}

using A2D1B2C2_AutoGarageFormatief.DataAccess;
using A2D1B2C2_AutoGarageFormatief.Exceptions;

namespace A2D1B2C2_AutoGarageFormatief.Model
{
    /// <summary>
    /// Vehicle (parent) class
    /// </summary>
    public class Vehicle
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Description of the vehicle
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// License plate of the vehicle
        /// </summary>
        public string LicensePlate { get; set; }

        /// <summary>
        /// Owner of the car
        /// </summary>
        public CarOwner CarOwner { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="licensePlate"></param>
        public Vehicle(int id, string licensePlate, CarOwner carOwner)
        {
            Id = id;
            LicensePlate = licensePlate ?? string.Empty;
            CarOwner = carOwner;
        }

        /// <summary>
        /// Check licenseplate: should have 8 characters and 2 -
        /// </summary>
        /// <returns>True if correct</returns>
        public virtual bool CheckLicensePlate()
        {
            // check length equals 8 and there are two - characters
            return LicensePlate.Length == 8 && LicensePlate.Count(lp => lp == '-') == 2;
        }

        // data access

        public static Vehicle? ReadVehicleData(int id)
        {
            return new DALSQL().ReadVehicle(id);
        }

        /// <summary>
        /// Create vehicle in data layer
        /// </summary>
        public void CreateVehicleData()
        {
            if (CheckLicensePlate())
            {
                new DALSQL().CreateVehicle(this);
            }
            else
            {
                throw new InvalidLicensePlateException();
            }
        }

        /// <summary>
        /// Update the vehicle
        /// </summary>
        /// <exception cref="Exception">Invalidlicenseplate</exception>
        public void UpdateVehicleData()
        {
            if (CheckLicensePlate())
            {
                new DALSQL().UpdateVehicle(this);
            }
            else
            {
                throw new InvalidLicensePlateException();
            }
        }

        /// <summary>
        /// Delete the vehicle from the data layer
        /// </summary>
        public void DeleteVehicleData()
        {
            new DALSQL().DeleteVehicle(this);
        }
    }
}

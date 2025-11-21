namespace A2D1B2C2_AutoGarageFormatief.Model
{
    /// <summary>
    /// Commercial vehicle class
    /// </summary>
    public class CommercialVehicle : Vehicle
    {
        /// <summary>
        /// Towing weight of the vehicle
        /// </summary>
        public int TowingWeight { get; set; }

        /// <summary>
        /// Commercial vehicle should also start with a V
        /// </summary>
        /// <returns></returns>
        public override bool CheckLicensePlate()
        {
            return base.CheckLicensePlate() && LicensePlate.ToLower().StartsWith('v');
        }

        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="id"Id of the vehicle></param>
        /// <param name="licensePlate">Licenseplate</param>
        /// <param name="towingWeight">The towing weight</param>
        public CommercialVehicle(int id, string licensePlate, int towingWeight, CarOwner carOwner) : base(id, licensePlate, carOwner)
        {
            TowingWeight = towingWeight;
        }
    }
}

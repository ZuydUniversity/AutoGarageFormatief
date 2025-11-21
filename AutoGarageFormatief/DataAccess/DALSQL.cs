using A2D1B2C2_AutoGarageFormatief.Exceptions;
using A2D1B2C2_AutoGarageFormatief.Model;
using Microsoft.Data.SqlClient;

namespace A2D1B2C2_AutoGarageFormatief.DataAccess
{
    /// <summary>
    /// Data access layer for SQL
    /// </summary>
    public class DALSQL
    {
        /// <summary>
        /// Servername
        /// </summary>
        private readonly string serverName;
        /// <summary>
        /// Databasename
        /// </summary>
        private readonly string databaseName;
        /// <summary>
        /// The generated connection string
        /// </summary>
        /// <returns></returns>        
        //SQL server install:
        private string ConnectionString() => $"Data Source={serverName};Initial Catalog={databaseName};Integrated Security=True";
        // todo: pas de connection string aan indien nodig!
        //SQL in docker using https://github.com/ZuydUniversity/KSE/tree/main/sql-server-docker
        //private string ConnectionString() => $"Data Source={serverName};Initial Catalog={databaseName};User ID=sa;Password=DevPassword123!;Encrypt=True;Trust Server Certificate=True";


        /// <summary>
        /// default constructor
        /// </summary>
        public DALSQL()
        {
            serverName = ".";
            databaseName = "AutoGarageFormatief";
        }

        /// <summary>
        /// Create a vehicle
        /// </summary>
        /// <param name="vehicle">The vehicle to add</param>
        /// <exception cref="ArgumentNullException">There is no vehicle</exception>
        /// <exception cref="NoOwnerException">Vehicle has no owner</exception>
        /// <exception cref="InvalidLicensePlateException">Vehicle has invalid license plate</exception>
        public void CreateVehicle(Vehicle vehicle)
        {
            // checks
            if (vehicle == null) throw new ArgumentNullException(nameof(vehicle));
            if (vehicle.CarOwner == null)
                throw new NoOwnerException();
            if (!vehicle.CheckLicensePlate())
                throw new InvalidLicensePlateException();

            // add 
            using (SqlConnection connection = new SqlConnection(ConnectionString()))
            {
                string sql = "insert into vehicle (Description, LicensePlate, TowingWeight, CarOwnerId) VALUES (@description, @licenseplate, @towingweight, @ownerid)";
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@description", vehicle.Description);
                    command.Parameters.AddWithValue("@licenseplate", vehicle.LicensePlate);
                    command.Parameters.AddWithValue("@ownerid", vehicle.CarOwner.Id.ToString());

                    // add towing weight if commercial vehicle                   
                    command.Parameters.AddWithValue("@towingweight", vehicle is CommercialVehicle comm ? comm.TowingWeight : 0);

                    command.ExecuteNonQuery();

                    command.CommandText = "SELECT CAST(@@Identity as INT);";
                    int addId = (int)command.ExecuteScalar();
                    vehicle.Id = addId;
                }
                connection.Close();
            }
        }

        /// <summary>
        /// Read vehicles for a owner
        /// </summary>
        /// <param name="carOwner">The owner to get the vehicles for</param>
        /// <returns>A list with vehicles</returns>
        public List<Vehicle> ReadVehicles(CarOwner carOwner)
        {
            List<Vehicle> vehicles = new List<Vehicle>();

            using (SqlConnection connection = new SqlConnection())
            {
                using (SqlCommand command = new SqlCommand())
                {
                    connection.ConnectionString = ConnectionString();
                    connection.Open();
                    command.Connection = connection;
                    command.CommandText = $"select id, description, LicensePlate, TowingWeight from vehicle where carownerid = @ownerid";
                    command.Parameters.AddWithValue("@ownerid", carOwner.Id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // prevent null reference warnings
                            var idString = reader[0].ToString() ?? "0";
                            var descriptionString = reader[1].ToString() ?? string.Empty;
                            var licensePlateString = reader[2].ToString() ?? string.Empty;
                            var towingWeigtString = reader[3].ToString() ?? string.Empty;

                            Vehicle newVehicle;
                            if (String.IsNullOrEmpty(towingWeigtString))
                            {
                                newVehicle = new Vehicle(Int32.Parse(idString), licensePlateString, carOwner);
                            }
                            else
                            {
                                newVehicle = new CommercialVehicle(Int32.Parse(idString), licensePlateString, Int32.Parse(towingWeigtString), carOwner);
                            }
                            newVehicle.Description = descriptionString;
                            vehicles.Add(newVehicle);
                        }
                    }
                    return vehicles;
                }
            }
        }


        public Vehicle? ReadVehicle(int id)
        {
            using (SqlConnection connection = new SqlConnection())
            {
                using (SqlCommand command = new SqlCommand())
                {
                    connection.ConnectionString = ConnectionString();
                    connection.Open();
                    command.Connection = connection;
                    command.CommandText = $"select id, description, LicensePlate, TowingWeight, carownerid from vehicle where id = @id";
                    command.Parameters.AddWithValue("@id", id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        // will run only once
                        while (reader.Read())
                        {
                            // prevent null reference warnings
                            var idString = reader[0].ToString() ?? "0";
                            var descriptionString = reader[1].ToString() ?? string.Empty;
                            var licensePlateString = reader[2].ToString() ?? string.Empty;
                            var towingWeigtString = reader[3].ToString() ?? string.Empty;
                            var ownerIdString = reader[4].ToString() ?? string.Empty;

                            // todo get owner (slechte code hieronder....)
                            var theOwner = this.ReadOwners().FirstOrDefault(o => o.Id == Int32.Parse(ownerIdString));
                            if (theOwner != null)
                            {
                                Vehicle newVehicle;
                                if (String.IsNullOrEmpty(towingWeigtString))
                                {
                                    newVehicle = new Vehicle(Int32.Parse(idString), licensePlateString, theOwner);
                                }
                                else
                                {
                                    newVehicle = new CommercialVehicle(Int32.Parse(idString), licensePlateString, Int32.Parse(towingWeigtString), theOwner);
                                }
                                newVehicle.Description = descriptionString;
                                return newVehicle;
                            }
                        }
                    }
                }
            }
            return default;
        }


        /// <summary>
        /// Update vehicle
        /// </summary>
        /// <param name="vehicle">The vehicle to update</param>
        /// <exception cref="ArgumentNullException">No vehicle given</exception>
        /// <exception cref="Exception">Vehicle has no id so doesn't exist in database</exception>
        public void UpdateVehicle(Vehicle vehicle)
        {
            // checks
            if (vehicle == null)
            {
                throw new ArgumentNullException(nameof(vehicle));
            }

            if (vehicle.Id == 0)
            {
                throw new Exception("You have to insert first!");
            }

            using (SqlConnection connection = new SqlConnection(ConnectionString()))
            {
                connection.Open();
                string sql = $"update vehicle set description = @description, licenseplate = @licenseplate, towingweight = @weigt where id = @vehicleid;";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@description", vehicle.Description);
                    command.Parameters.AddWithValue("@licenseplate", vehicle.LicensePlate);
                    // only set towing weight when commercial vehicle
                    command.Parameters.AddWithValue("@weigt", vehicle is CommercialVehicle commercialVehicle ? commercialVehicle.TowingWeight : 0);
                    command.Parameters.AddWithValue("@vehicleid", vehicle.Id);
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }

        }

        /// <summary>
        /// Delete the vehicle
        /// </summary>
        /// <param name="vehicle">The vehicle to delete</param>
        public void DeleteVehicle(Vehicle vehicle)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString()))
            {
                connection.Open();
                string sql = "delete from vehicle where id = @id";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", vehicle.Id);
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public List<CarOwner> ReadOwners()
        {
            List<CarOwner> Owners = new List<CarOwner>();

            using (SqlConnection connection = new SqlConnection())
            {
                using (SqlCommand command = new SqlCommand())
                {
                    connection.ConnectionString = ConnectionString();
                    connection.Open();
                    command.Connection = connection;
                    command.CommandText = $"select id, name from carowner";

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        // will only iterate once but will prevent error when no results
                        while (reader.Read())
                        {
                            // prevent null reference warnings
                            var idString = reader[0].ToString() ?? "0";
                            var nameString = reader[1].ToString() ?? string.Empty;

                            CarOwner newOwner = new CarOwner(int.Parse(idString), nameString);

                            // get vehicles
                            newOwner.Vehicles = ReadVehicles(newOwner);

                            // add to list
                            Owners.Add(newOwner);
                        }
                    }
                    connection.Close();
                }
            }
            return Owners;
        }



    }
}

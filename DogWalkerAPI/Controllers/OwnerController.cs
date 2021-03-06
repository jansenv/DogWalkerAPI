﻿using DogWalkerAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DogWalkerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController] 
    public class OwnerController : ControllerBase
    {
        private readonly IConfiguration _config;

        public OwnerController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string include)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, [Name], Address, NeighborhoodId, Phone
                                        FROM Owner
                                        ";

                    cmd.CommandText = @"
                        SELECT o.Id, o.[Name], o.Address, o.NeighborhoodId, o.Phone ";

                    if (include == "neighborhood")
                    {
                        cmd.CommandText += ", n.Id, n.Name AS Neighborhood ";
                    }

                    cmd.CommandText += "FROM Owner o ";
                    if (include == "neighborhood")
                    {
                        cmd.CommandText += "LEFT JOIN Neighborhood n ON o.NeighborhoodId = n.Id";
                    }

                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Owner> owners = new List<Owner>();

                    Owner owner = null;

                    while (reader.Read())
                    {
                        if (include == "neighborhood")
                        {
                            owner = new Owner
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Address = reader.GetString(reader.GetOrdinal("Address")),
                                Phone = reader.GetString(reader.GetOrdinal("Phone")),
                                NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                                Neighborhood = new Neighborhood()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                                    Name = reader.GetString(reader.GetOrdinal("Neighborhood"))
                                }
                            };
                        }
                        else
                        {
                            owner = new Owner
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Address = reader.GetString(reader.GetOrdinal("Address")),
                                Phone = reader.GetString(reader.GetOrdinal("Phone")),
                                NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                            };
                        }

                        owners.Add(owner);
                    }
                    reader.Close();

                    return Ok(owners);
                }
            }
        }

        [HttpGet("{id}", Name = "GetOwner")]
        public async Task<IActionResult> Get(
            [FromRoute] int id,
            [FromQuery] string include
            )
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT o.Id, o.Name, o.Address, o.NeighborhoodId, o.Phone, d.id AS DogId, d.Name AS DogName, d.Breed, d.OwnerId ";
                    if (include == "neighborhood")
                    {
                        cmd.CommandText += ", n.Id, n.Name AS Neighborhood ";
                    }

                    cmd.CommandText += "FROM Owner O LEFT JOIN Dog d ON o.Id = d.OwnerId ";
                    if (include == "neighborhood")
                    {
                        cmd.CommandText += "LEFT JOIN Neighborhood n ON o.NeighborhoodId = n.Id ";
                    }

                    cmd.CommandText += "WHERE o.Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Owner owner = null;

                    if (include == "neighborhood")
                    {
                        while (reader.Read())
                        {
                            if (owner == null)
                            {
                                owner = new Owner
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    Name = reader.GetString(reader.GetOrdinal("Name")),
                                    Address = reader.GetString(reader.GetOrdinal("Address")),
                                    Phone = reader.GetString(reader.GetOrdinal("Phone")),
                                    NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                                    Neighborhood = new Neighborhood()
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                                        Name = reader.GetString(reader.GetOrdinal("Neighborhood")),
                                    },
                                    Dogs = new List<Dog>()
                                };
                            }

                            owner.Dogs.Add(new Dog()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("DogId")),
                                Name = reader.GetString(reader.GetOrdinal("DogName")),
                                Breed = reader.GetString(reader.GetOrdinal("Breed")),
                                OwnerId = reader.GetInt32(reader.GetOrdinal("Id"))
                            });
                        }
                    }
                    else
                    {
                        while (reader.Read())
                        {
                            if (owner == null)
                            {
                                owner = new Owner
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    Name = reader.GetString(reader.GetOrdinal("Name")),
                                    Address = reader.GetString(reader.GetOrdinal("Address")),
                                    Phone = reader.GetString(reader.GetOrdinal("Phone")),
                                    NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                                    Dogs = new List<Dog>()
                                };
                            }

                            owner.Dogs.Add(new Dog()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("DogId")),
                                Name = reader.GetString(reader.GetOrdinal("DogName")),
                                Breed = reader.GetString(reader.GetOrdinal("Breed")),
                                OwnerId = reader.GetInt32(reader.GetOrdinal("OwnerId"))
                            });
                        }
                    }

                    reader.Close();

                    return Ok(owner);
                }
            }
        }

        //[HttpPost]
        //public async Task<IActionResult> Post([FromBody] Owner owner)
        //{
        //    using (SqlConnection conn = Connection)
        //    {
        //        conn.Open();
        //        using (SqlCommand cmd = conn.CreateCommand())
        //        {
        //            cmd.CommandText = @"INSERT INTO Owner (Name, Address, NeighborhoodId, Phone)
        //                                OUTPUT INSERTED.Id
        //                                VALUES (@name, @address, @neighborhoodId, @phone)";

        //            cmd.Parameters.Add(new SqlParameter("@name", owner.Name));
        //            cmd.Parameters.Add(new SqlParameter("@address", owner.Address));
        //            cmd.Parameters.Add(new SqlParameter("@neighborhoodId", owner.NeighborhoodId));
        //            cmd.Parameters.Add(new SqlParameter("@phone", owner.Phone));

        //            int newId = (int)cmd.ExecuteScalar();
        //            owner.Id = newId;
        //            return CreatedAtRoute("GetOwner", new { id = newId }, owner);
        //        }
        //    }
        //}

        //[HttpPut("{id}")]
        //public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Owner owner)
        //{
        //    try
        //    {
        //        using (SqlConnection conn = Connection)
        //        {
        //            conn.Open();
        //            using (SqlCommand cmd = conn.CreateCommand())
        //            {
        //                cmd.CommandText = @"UPDATE Owner
        //                                    SET Name = @name, Address = @address, NeighborhoodId = @neighborhoodId, Phone = @phone
        //                                    WHERE Id = @id";

        //                cmd.Parameters.Add(new SqlParameter("@id", id));
        //                cmd.Parameters.Add(new SqlParameter("@name", owner.Name));
        //                cmd.Parameters.Add(new SqlParameter("@address", owner.Address));
        //                cmd.Parameters.Add(new SqlParameter("@neighborhoodId", owner.NeighborhoodId));
        //                cmd.Parameters.Add(new SqlParameter("@phone", owner.Phone));



        //                int rowsAffected = cmd.ExecuteNonQuery();
        //                if (rowsAffected > 0)
        //                {
        //                    return new StatusCodeResult(StatusCodes.Status204NoContent);
        //                }
        //                throw new Exception("No rows affected");
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        if (!OwnerExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }
        //}

        //private bool OwnerExists(int id)
        //{
        //    using (SqlConnection conn = Connection)
        //    {
        //        conn.Open();
        //        using (SqlCommand cmd = conn.CreateCommand())
        //        {
        //            cmd.CommandText = @"
        //                SELECT o.Id, o.[Name], o.Address, o.NeighborhoodId, o.Phone, n.Name NeighborhoodName, d.Id DogId, d.Name DogName, d.Breed, d.Notes
        //                FROM Owner o
        //                LEFT JOIN Neighborhood n
        //                ON o.NeighborhoodId = n.Id
        //                LEFT JOIN Dog d
        //                ON o.Id = d.OwnerId
        //                WHERE o.Id = @id";

        //            cmd.Parameters.Add(new SqlParameter("@id", id));

        //            SqlDataReader reader = cmd.ExecuteReader();
        //            return reader.Read();
        //        }
        //    }
        //}
    }
}

using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GymTECNoRelational.Models
{
    public class AdminToken
    {
        [BsonId]
        public Guid id { get; set; }
        public string adminId { get; set; }
        public string token { get; set; }

        //Constructor standard de la clase
        public AdminToken()
        {

        }
    }
}
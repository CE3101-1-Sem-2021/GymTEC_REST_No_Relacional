using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GymTECNoRelational.Models
{
    //Clase que abstrae el concepto de un cliente.
    public class Client
    {
        [BsonId]
        public Guid id { get; set; }
        public string cedula { get; set; }
        public string nombre { get; set; }
        public string apellidos { get; set; }
        public int edad { get; set; }
        public DateTime fechaNacimiento { get; set; }
        public float peso { get; set; }
        public float imc { get; set; }
        public string direccion { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string salt { get; set;}
        public string token { get; set; }
        
        //Constructor standard de la clase
        public Client()
        {

        }

    }
}
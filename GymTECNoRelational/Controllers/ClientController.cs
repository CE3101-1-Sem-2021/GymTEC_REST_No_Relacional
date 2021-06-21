using GymTECNoRelational.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace GymTECNoRelational.Controllers
{
    public class ClientController : ApiController
    {
        DBConnection database = new DBConnection();

        /*Metodo para registrar un nuevo cliente.
         * 
         * Entrada:Informacion del cliente 
         * Salida:Respuesta de tipo HTTP que indica si la operacion fue exitosa
         */
        [Route("api/Client/{operation}")]
        public HttpResponseMessage Post([FromBody]Client client,string operation)
        {
            if (operation.Equals("registerClient"))
            {
                return database.registerRequest(client);
            }
            else if(operation.Equals("loginClient"))
            {
                return database.loginRequest(client);
            }
            return Request.CreateResponse(HttpStatusCode.Conflict, "Operacion invalida");
        }

        /*Metodo para actualizar un cliente en la base da datos.
         * 
         * Entrada:Cedula actual del cliente,informacion del cliente a actualizar,token de verificacion
         * Salida:Respuesta de tipo HTTP que indica si la operacion fue exitosa.
         */

        // PUT: api/Client/5
        [Route("api/Client/updateClient/{currentId}/{token}")]
        public HttpResponseMessage Put([FromBody]Client client,string currentId,string token)
        {
            return database.updateClient(currentId, client, token);
        }
    }
}

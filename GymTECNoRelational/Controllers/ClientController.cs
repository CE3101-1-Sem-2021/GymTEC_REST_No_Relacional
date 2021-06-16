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



        /*Metodo para obtener todos los clientes registrados en la base de datos.
         * 
         * Entrada:Token de verificacion
         * Salida:Lista de clientes registrados
         */
        [Route("api/Client/getAllClients/{token}")]
        public HttpResponseMessage Get(string token)
        {
            if (database.tokenVerifier(token))
            {
                return Request.CreateResponse(HttpStatusCode.OK,database.getAllClients());
            }
            return Request.CreateResponse(HttpStatusCode.Conflict,"Token Invalido");
        }

        /*Metodo para obtener la informacion de un cliente registrado.
         * 
         * Entrada:Token de verificacion,cedula del cliente
         * Salida:Informacion del cliente
         */
        [Route("api/Client/getClient/{clientId}/{token}")]
        public HttpResponseMessage Get(string clientId,string token)
        {
            if(database.tokenVerifier(token)||database.selfTokenVerifier(token,clientId))
            {
                return Request.CreateResponse(HttpStatusCode.OK, database.getClientByColumn(clientId, "cedula"));
            }
            return Request.CreateResponse(HttpStatusCode.Conflict,"Token invalido");
        }

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

        [Route("api/Client/adminTokenUpdate/{adminId}/{token}")]
        public void Post(string token,string adminId)
        {
            database.updateAdminToken(adminId, token);
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

        /*Metodo para elimnar un cliente en la base da datos.
         * 
         * Entrada:Cedula del cliente,token de verificacion
         * Salida:Respuesta de tipo HTTP que indica si la operacion fue exitosa.
         */

        [Route("api/Client/deleteClient/{clientId}/{token}")]
        // DELETE: api/Client/5
        public HttpResponseMessage Delete(string clientId,string token)
        {
            return database.deleteClient(clientId, token);
        }
    }
}

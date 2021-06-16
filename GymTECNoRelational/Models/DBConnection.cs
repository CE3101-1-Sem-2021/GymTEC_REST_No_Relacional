using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace GymTECNoRelational.Models
{
    public class DBConnection
    {
        private IMongoDatabase database;

        public DBConnection()
        {
            MongoClient mongoClient = new MongoClient();
            database = mongoClient.GetDatabase("GymTEC");

        }

        /*Metodo para obtener todos los clientes registrados en la base de datos.
         * 
         * Entrada:Token de verificacion
         * Salida:Lista de clientes registrados
         */
        public List<Client> getAllClients()
        {
            var collection = database.GetCollection<Client>("Cliente");
            return collection.Find(new BsonDocument()).ToList();
        }
        /*Metodo para obtener la informacion de un cliente registrado a partir de la informacion de una columna que almacene un valor acaracteristico.
         * 
         * Entrada:Valor de la columna, columna a revisar
         * Salida:Informacion del cliente
         */
        public Client getClientByColumn(string columnValue,string column)
        {
            var collection = database.GetCollection<Client>("Cliente");
            var filter = Builders<Client>.Filter.Eq(column,columnValue);
            List<Client> clientList= collection.Find(filter).ToList();
            if(clientList.Count()==0)
            {
                return null;
            }
            return clientList[0];
        }

        /*Metodo para actualizar un cliente en la base da datos.
         * 
         * Entrada:Cedula actual del cliente,informacion del cliente a actualizar,token de verificacion
         * Salida:Respuesta de tipo HTTP que indica si la operacion fue exitosa.
         */
        public HttpResponseMessage updateClient(string currentId, Client client,string token)
        {
            HttpResponseMessage response = null;
            if (selfTokenVerifier(token, currentId))
            {
                if (getClientByColumn(currentId, "cedula") != null)
                {
                    if (currentId == client.cedula || getClientByColumn(client.cedula, "cedula") == null)
                    {
                        Client temp = getClientByColumn(currentId, "cedula");

                        if (temp.email == client.email || getClientByColumn(client.email, "email") == null)
                        {
                            var collection = database.GetCollection<Client>("Cliente");
                            var filter = Builders<Client>.Filter.Eq("cedula", currentId);

                            var update = Builders<Client>.Update.Set("cedula", client.cedula).
                                                                 Set("nombre", client.nombre).
                                                                 Set("apellidos", client.apellidos).
                                                                 Set("edad", client.edad).
                                                                 Set("fechaNacimiento", client.fechaNacimiento).
                                                                 Set("peso", client.peso).
                                                                 Set("imc", client.imc).
                                                                 Set("direccion", client.direccion).
                                                                 Set("email", client.email);
                            collection.UpdateOne(filter, update);

                            response = new HttpResponseMessage(HttpStatusCode.Conflict);
                            response.Content = new StringContent("Valor actualizado correctamente");
                            return response;
                        }
                        response = new HttpResponseMessage(HttpStatusCode.Conflict);
                        response.Content = new StringContent("El nuevo correo electronico ya se encuentra registrado por otro cliente");
                        return response;

                    }
                    response = new HttpResponseMessage(HttpStatusCode.Conflict);
                    response.Content = new StringContent("El nuevo numero de cedula ya se encuentra registrado por otro cliente");
                    return response;

                }
                response = new HttpResponseMessage(HttpStatusCode.Conflict);
                response.Content = new StringContent("La cedula provista no se encuentra registrada");
                return response;
            }
            response = new HttpResponseMessage(HttpStatusCode.Conflict);
            response.Content = new StringContent("Token invalido");
            return response;

        }

        /*Metodo para elimnar un cliente en la base da datos.
         * 
         * Entrada:Cedula del cliente,token de verificacion
         * Salida:Respuesta de tipo HTTP que indica si la operacion fue exitosa.
         */
        public HttpResponseMessage deleteClient(string clientId,string token)
        {
            HttpResponseMessage response = null;
            if (tokenVerifier(token))
            {
                if (getClientByColumn(clientId, "cedula") != null)
                {
                    var collection = database.GetCollection<Client>("Cliente");
                    var filter = Builders<Client>.Filter.Eq("cedula", clientId);
                    collection.DeleteOne(filter);

                    response = new HttpResponseMessage(HttpStatusCode.Conflict);
                    response.Content = new StringContent("Valor eliminado correctamente");
                    return response;
                }
                response = new HttpResponseMessage(HttpStatusCode.Conflict);
                response.Content = new StringContent("La cedula provista no se encuentra registrada");
                return response;
            }
            response = new HttpResponseMessage(HttpStatusCode.Conflict);
            response.Content = new StringContent("Token invalido");
            return response;

        }

        /*Metodo para registrar un nuevo cliente en la base da datos.
         * 
         * Entrada:Informacion del cliente a registrar
         * Salida:Respuesta de tipo HTTP que indica si la operacion fue exitosa.
         */
        public HttpResponseMessage registerRequest(Client client)
        {
            HttpResponseMessage response = null;
            Client temp = getClientByColumn(client.cedula,"cedula");
            if(temp==null)
            {
                temp = getClientByColumn(client.email, "email");
                if(temp==null)
                {
                    string[] cryptoComponents = md5Encryption(client.password);
                    client.password = cryptoComponents[0];
                    client.salt = cryptoComponents[1];
                    client.token = getToken();
                    var collection = database.GetCollection<Client>("Cliente");
                    collection.InsertOne(client);
                    response = new HttpResponseMessage(HttpStatusCode.Conflict);
                    response.Content = new StringContent("Cliente registrado correctamente");
                    return response;
                }
                response = new HttpResponseMessage(HttpStatusCode.Conflict);
                response.Content = new StringContent("El correo provisto ya se encutra registrado");
                return response;

            }
            response = new HttpResponseMessage(HttpStatusCode.Conflict);
            response.Content = new StringContent("La cedula provista ya se encuentra registrada");
            return response;

        }

        /*Metodo que permite a un cliente loguearse en el sistema.
         * 
         * Entrada:Credenciales del cliente
         * Salida:Respuesta de tipo HTTP que indica si la operacion fue exitosa.
         */
        public HttpResponseMessage loginRequest(Client client)
        {
            HttpResponseMessage response = null;
            Client temp = getClientByColumn(client.email, "email");
            if (temp != null && passwordVerifier(client.password,temp.password, temp.salt))
            {
                string token = getToken();
                
                var collection = database.GetCollection<Client>("Cliente");
                var filter = Builders<Client>.Filter.Eq("cedula",temp.cedula);

                var update = Builders<Client>.Update.Set("token",token);
                collection.UpdateOne(filter, update);

                response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(token);
                return response;
            }
            response = new HttpResponseMessage(HttpStatusCode.Conflict);
            response.Content = new StringContent("Credenciales invalidas");
            return response;
        }
        public void updateAdminToken(string adminId,string token)
        {
            var collection = database.GetCollection<AdminToken>("AdminToken");
            var filter = Builders<AdminToken>.Filter.Eq("adminId", adminId);
            var update = Builders<AdminToken>.Update.Set("token", token);
            collection.UpdateOne(filter, update,new UpdateOptions { IsUpsert=true});
        }

        /*Metodo para obtener un token unico.
        * 
        * Entradas:-
        * Salidas:Token unico.
        */
        public string getToken()
        {
            string g = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            g = g.Replace("+", "");
            return g.Replace("/", "");
        }
        /*Metodo para encriptar contraseñas.
         * 
         * Entrada:Contraseña a encriptar
         * Salida: Lista que contiene la contraseña encriptada y su sal asociada.
         */
        public string[] md5Encryption(string input)
        {
            var rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
            var buff = new byte[10];
            rng.GetBytes(buff);

            string salt = Convert.ToBase64String(buff);

            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] bytes = md5.ComputeHash(Encoding.Unicode.GetBytes(input + salt));
            string result = BitConverter.ToString(bytes).Replace("-", String.Empty).ToLower();
            return new string[] { result, salt };
        }

        /*Metodo para saber si una contraseña si nencriptar corresponde a una contraseña encriptada.
         * 
         * Entrada:Contraseña encriptada,contraseña a comparar
         * Salida: Condicion que indica si las 2 contraseñas son correspondientes
         */
        public bool passwordVerifier(string password, string encrypted, string salt)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] bytes = md5.ComputeHash(Encoding.Unicode.GetBytes(password + salt));
            string result = BitConverter.ToString(bytes).Replace("-", String.Empty).ToLower();

            return result.Equals(encrypted);
        }

        /*Metodo para verificar que un token sea valido.
         * 
         * Entrada:Token a verificar
         * Salida: Booleano que indica si el token es valido.
         */
        public bool tokenVerifier(string token)
        {

            var collection = database.GetCollection<AdminToken>("AdminToken");
            var filter = Builders<AdminToken>.Filter.Eq("token",token);
            List<AdminToken> tokenList = collection.Find(filter).ToList();
            return tokenList.Count() != 0;
          
        }

        /*Metodo para verificar que un token pertenzca a un cliente.
         * 
         * Entrada:Token a verificar
         * Salida: Booleano que indica si el token es valido.
         */
        public bool selfTokenVerifier(string token,string clientID)
        {
            Client client = getClientByColumn(token, "token");
           
            return client!=null&& client.cedula==clientID;
        }

    }
}
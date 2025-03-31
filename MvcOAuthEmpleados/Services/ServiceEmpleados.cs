using System.Net.Http.Headers;
using MvcOAuthEmpleados.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;
using NuGet.Common;

namespace MvcOAuthEmpleados.Services
{
    public class ServiceEmpleados
    {
        private string UrlApi;
        private MediaTypeWithQualityHeaderValue header;
        private IHttpContextAccessor context;

        public ServiceEmpleados(IConfiguration configuration, IHttpContextAccessor context)
        {
            this.UrlApi = configuration.GetValue<string>("ApiUrls:ApiEmpleados");
            this.header = new MediaTypeWithQualityHeaderValue("application/json");
            this.context = context;
        }

        public async Task<string> GetTokenAsync(string user, string password)
        {
            string urlApi = "https://apioauthempleados2025.azurewebsites.net/";

            using (HttpClient client = new HttpClient())
            {
                string request = "api/auth/login";
                client.BaseAddress = new Uri(urlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                LoginModel model = new LoginModel
                {
                    User = user,
                    Password = password
                };
                string json = JsonConvert.SerializeObject(model);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(request, content);
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    JObject keys = JObject.Parse(data);
                    //sacamos el valor de "response" para coger el token
                    string token = keys.GetValue("response").ToString();
                    return token; //Devolvemos el token
                }
                else
                {
                    return null;
                }
            }
        }

        private async Task<T> CallApiAsync<T>(string request)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                HttpResponseMessage response = await client.GetAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    T data = await response.Content.ReadAsAsync<T>();
                    return data;
                }
                else
                {
                    return default(T);
                }
            }
        }

        //VAMOS A REALIZAR UNA SOBRECARGA DEL METODO RECIBIENDO EL TOKEN
        private async Task<T> CallApiAsync<T>(string request, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                client.DefaultRequestHeaders.Add("Authorization", "bearer " + token);
                HttpResponseMessage response = await client.GetAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    T data = await response.Content.ReadAsAsync<T>();
                    return data;
                }
                else
                {
                    return default(T);
                }
            }
        }

        public async Task<List<Empleado>> GetEmpleadosAsync()
        {
            string request = "api/empleados";
            List<Empleado> empleados = await this.CallApiAsync<List<Empleado>>(request);
            return empleados;
        }

        //RECUPERAMOS EL USER Y SU CLAIM DEL TOKEN
        public async Task<Empleado> FindEmpleadoAsync(int idEmpleado)
        {
            string token = this.context.HttpContext.User.FindFirst(x => x.Type == "TOKEN").Value;

            string request = "api/empleados/" + idEmpleado;
            Empleado emp = await this.CallApiAsync<Empleado>(request, token);
            return emp;
        }

        public async Task<Empleado> GetPerfilAsync()
        {
            string token = this.context.HttpContext.User.FindFirst(x => x.Type == "TOKEN").Value;
            string request = "api/empleados/perfil";
            Empleado emp = await this.CallApiAsync<Empleado>(request, token);
            return emp;
        }

        public async Task<List<Empleado>> GetCompislAsync()
        {
            string token = this.context.HttpContext.User.FindFirst(x => x.Type == "TOKEN").Value;
            string request = "api/empleados/compis";
            List<Empleado> empleados = await this.CallApiAsync<List<Empleado>>(request, token);
            return empleados;
        }

        public async Task<List<string>> GetOficiosAsync()
        {
            string request = "api/empleados/oficios";
            List<string> oficios = await this.CallApiAsync<List<string>>(request);
            return oficios;
        }

        //METODO PARA DEVOLVER ESTO -> oficio=ANALISTA&oficio=DIRECTOR
        private string TransformarColleccionToQuery(List<string> collection)
        {
            string result = "";
            foreach(string elem in collection)
            {
                result += "oficio=" + elem + "&";
            }
            //quitamos el ultimo &
            result = result.TrimEnd('&');
            return result;
        }

        public async Task<List<Empleado>> GetEmpleadosOficiosAsync(List<string> oficios)
        {
            string request = "api/empleados/empleadosoficios";
            string data = this.TransformarColleccionToQuery(oficios);
            List<Empleado> empleados = await this.CallApiAsync<List<Empleado>>(request + "?" + data);
            return empleados;
        }

        public async Task UpdateEmpleadosOficioAsync(int incremento, List<string> oficios)
        {
            string request = "api/empleados/incrementarsalarios/" + incremento;
            string data = this.TransformarColleccionToQuery(oficios);
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(this.UrlApi);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(this.header);
                HttpResponseMessage response = await client.PutAsync(request + "?" + data, null);
            }
        }
    }
}

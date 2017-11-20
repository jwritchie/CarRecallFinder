using CarRecallFinder.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace CarRecallFinder.Controllers
{
    /// <summary>
    /// Get Car information
    /// </summary>
    [RoutePrefix("api/cars")]
    public class CarsController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        /** New DbContext we create after database-first.
            NOTE: this doesn't work now that CarFinderDb.edmx has been excluded from the project. **/
        //private Entities db2 = new Entities();

        /// <summary>
        /// Get all years
        /// </summary>
        [Route("Year")]
        public async Task<List<string>> GetYears()
        {
            // var cars = db2.Cars.ToList();        LINQ  statement, does same as below.
            return await db.GetYears();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        [Route("Make")]
        public async Task<List<string>> GetMakes(string year)
        {
            return await db.GetMakes(year);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="year"></param>
        /// <param name="make"></param>
        /// <returns></returns>
        [Route("Model")]
        public async Task<List<string>> GetModels(string year, string make)
        {
            return await db.GetModels(year, make);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="year"></param>
        /// <param name="make"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("Trim")]
        public async Task<List<string>> GetTrims(string year, string make, string model)
        {
            return await db.GetTrims(year, make, model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="year"></param>
        /// <param name="make"></param>
        /// <param name="model"></param>
        /// <param name="trim"></param>
        /// <returns></returns>
        [Route("Car")]
        public async Task<CarRecallFinder.Models.Car> GetCar(string year, string make, string model, string trim)
        {
            return await db.GetCar(year, make, model, trim);
        }

        /// <summary>
        /// Get recall data for the year, make, & model.
        /// </summary>
        /// <param name="year"></param>
        /// <param name="make"></param>
        /// <param name="model"></param>
        /// <param name="trim"></param>
        /// <returns></returns>
        // Make API call request to NHTSA
        [Route("CarData")]
        public async Task<IHttpActionResult> GetCarData(string year, string make, string model, string trim)
        {
            HttpResponseMessage response;

            var car = new carViewModel();

            // Allow use of whichever protocol the receiving server is using.
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://one.nhtsa.gov/");
                try
                {
                    response = await client.GetAsync("webapi/api/Recalls/vehicle/modelyear/" + year + "/make/"
                        + make + "/model/" + model + "?format=json");
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<Recalls>(json);
                    car.RecallResults = result.Results;

                    /** Builds URL with data:
                        https://one.nhtsa.gov/webapi/api/Recalls/vehicle/modelyear/2008/make/pontiac/model/g6?format=json
                        which is sent, and await resulting data back in json format. 
                    **/
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    await Task.FromResult(0);
                }
            }
            return Ok(car);
        }

        /// <summary>
        /// Get car images for the year, make, model, trim.
        /// </summary>
        /// <param name="year"></param>
        /// <param name="make"></param>
        /// <param name="model"></param>
        /// <param name="trim"></param>
        /// <returns></returns>
        [Route("CarImage")]
        public async Task<IHttpActionResult> getCarImage(string year, string make, string model, string trim)
        {
            var car = new carViewModel();

            // Bing Cognitive Search
            var query = year + " " + make + " " + model + " " + trim;

            //var url = $"https://api.cognitive.microsoft.com/bing/v7.0/images/" +
            //    $"search?q={WebUtility.UrlEncode(query)}" +
            //    $"&count=20&offset=0&mkt=en-us&safeSearch=Strict"

            var url = $"https://api.cognitive.microsoft.com/bing/v7.0/images/" +
                $"search?q={WebUtility.UrlEncode(query)}" +
                $"&count=20&offset=0&mkt=en-us&safeSearch=Strict";
            var requestHeaderKey = "Ocp-Apim-Subscription-Key";
            var requestHeaderValue = ConfigurationManager.AppSettings["bingSearchKey"];

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add(requestHeaderKey, requestHeaderValue);
                    var json = await client.GetStringAsync(url);
                    var result = JsonConvert.DeserializeObject<SearchResults>(json);
                    car.ImageUrl = result.Images.Select(i => i.ContentUrl);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

            return Ok(car);
        }


    }
}
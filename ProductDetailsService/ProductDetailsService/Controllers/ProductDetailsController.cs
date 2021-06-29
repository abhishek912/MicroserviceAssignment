using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductDetailsService.Entity;
using System.IO;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace ProductDetailsService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductDetailsController : Controller
    {
        private readonly ILogger<ProductDetailsController> _logger;
        public ProductDetailsController(ILogger<ProductDetailsController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return Ok("ProductDetails Service");
        }

        [HttpPost]
        [Route("Add")]
        [Authorize]
        public IActionResult AddProductDetails([FromBody] ProductDetail obj)
        {
            List<ProductDetail> details = new List<ProductDetail>();
            try
            {
                if (System.IO.File.Exists(@"../../DataFiles/ProductDetails.json"))
                {
                    using (StreamReader r = new StreamReader(@"../../DataFiles/ProductDetails.json"))
                    {
                        string json = r.ReadToEnd();
                        details = JsonConvert.DeserializeObject<List<ProductDetail>>(json);
                    }
                }
                details.Add(obj);
                string jsonData = JsonConvert.SerializeObject(details.ToArray());
                System.IO.File.WriteAllText(@"../../DataFiles/ProductDetails.json", jsonData);
                var message = new
                {
                    MethodCalled = "ProductDetailsService/ProductDetailsController/AddProductDetails",
                    Action = "Add the product details to the existing product",
                    Status = "Success"
                };
                _logger.LogInformation(message.ToString());
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.ToString());
                return Ok(e);
            }
            return Ok("Product details added successfully!!!");
        }

        [HttpPost]
        [Route("Remove")]
        public IActionResult RemoveProductDetails([FromBody] int id)
        {
            List<ProductDetail> details = new List<ProductDetail>();
            try
            {
                if (System.IO.File.Exists(@"../../DataFiles/ProductDetails.json"))
                {
                    using (StreamReader r = new StreamReader(@"../../DataFiles/ProductDetails.json"))
                    {
                        string json = r.ReadToEnd();
                        details = JsonConvert.DeserializeObject<List<ProductDetail>>(json);
                    }
                }
                var detail = details.Where(x => x.ProductID == id).SingleOrDefault();
                if(detail != null)
                {
                    details.Remove(detail);
                    var message = new
                    {
                        MethodCalled = "ProductDetailsService/ProductDetailsController/RemoveProductDetails",
                        Action = "Remove the product details of the existing product",
                        Status = "Success"
                    };
                    _logger.LogInformation(message.ToString());

                }
                else
                {
                    var message = new
                    {
                        MethodCalled = "ProductDetailsService/ProductDetailsController/RemoveProductDetails",
                        Action = "Remove the product details of the existing product",
                        Status = "Failed"
                    };
                    _logger.LogInformation(message.ToString());
                    return Ok("Product details already removed!!!");
                }
                string jsonData = JsonConvert.SerializeObject(details.ToArray());
                System.IO.File.WriteAllText(@"../../DataFiles/ProductDetails.json", jsonData);
            }
            catch (Exception e)
            {
                return Ok(e);
            }
            return Ok("Product details removed successfully!!!");
        }

        [HttpPost]
        [Route("Details")]
        public IActionResult GetProductDetails([FromBody] int id)
        {
            List<ProductDetail> details = new List<ProductDetail>();
            try
            {
                if (System.IO.File.Exists(@"../../DataFiles/ProductDetails.json"))
                {
                    using (StreamReader r = new StreamReader(@"../../DataFiles/ProductDetails.json"))
                    {
                        string json = r.ReadToEnd();
                        details = JsonConvert.DeserializeObject<List<ProductDetail>>(json);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.ToString());
                return Ok(e);
            }
            var detail = details.Where(x => x.ProductID == id).SingleOrDefault();
            var message = new
            {
                MethodCalled = "ProductDetailsService/ProductDetailsController/GetProductDetails",
                Action = "Get the product details of the existing product using ProductId",
                Status = "Success"
            };
            _logger.LogInformation(message.ToString());
            return Ok(detail);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using ProductService.Entity;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Net.Http.Json;
using ProductService.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace ProductService.Controllers
{
    [ApiController]
    [Route("[controller]/")]
    public class InventoryController : Controller
    {
        private readonly ILogger<InventoryController> _logger;
        public InventoryController(ILogger<InventoryController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return Ok("Product Service");
        }

        [HttpPost]
        [Route("Add")]
        [AllowAnonymous]
        //[Authorize]
        public IActionResult AddProduct([FromBody] Product obj)
        {
            List<Product> items = new List<Product>();
            int newProductId = 0;
            try
            {
                if (System.IO.File.Exists(@"../../DataFiles/Inventory.json"))
                {
                    using (StreamReader r = new StreamReader(@"../../DataFiles/Inventory.json"))
                    {
                        string json = r.ReadToEnd();
                        items = JsonConvert.DeserializeObject<List<Product>>(json);
                    }
                    if(items.Count > 0)
                    {
                        newProductId = items.Last().ProductID + 1;
                    }
                    else
                    {
                        newProductId = 1;
                    }
                    var message = new
                    {
                        MethodCalled = "ProductService/InventoryController/AddProduct",
                        NewFileCreated = false,
                        FileAlreadyExisted = true,
                        InventoryCount = items.Count+1,
                        Action = "Add new product to the inventory",
                        Status = "Success"
                    };
                }
                else
                {
                    var message = new
                    {
                        MethodCalled = "ProductService/InventoryController/AddProduct",
                        NewFileCreated = true,
                        FileAlreadyExisted = false,
                        InventoryCount = 1,
                        Action = "Add new product to the inventory",
                        Status = "Success"
                    };
                    _logger.LogInformation(message.ToString());
                    
                    newProductId = 1;
                }
                obj.ProductID = newProductId;
                items.Add(obj);
                string jsonData = JsonConvert.SerializeObject(items.ToArray());
                System.IO.File.WriteAllText(@"../../DataFiles/Inventory.json", jsonData);
            }
            catch(Exception e)
            {
                _logger.LogInformation(e.ToString());
                return Ok(e);
            }
            return Ok("Product is Added to the Inventory!!! ProductId is: " + newProductId);
        }

        [HttpPost]
        [Route("Remove")]
        [AllowAnonymous]
        //[Authorize]
        public async Task<IActionResult> RemoveProduct([FromBody] int id)
        {
            List<Product> items = new List<Product>();
            try
            {
                if (System.IO.File.Exists(@"../../DataFiles/Inventory.json"))
                {
                    using (StreamReader r = new StreamReader(@"../../DataFiles/Inventory.json"))
                    {
                        string json = r.ReadToEnd();
                        items = JsonConvert.DeserializeObject<List<Product>>(json);
                    }
                    Product prod = items.Where(x => x.ProductID == id).SingleOrDefault();
                    if(prod != null)
                    {
                        HttpClient client = new HttpClient();
                        client.BaseAddress = new Uri("https://localhost:44362/");
                        var response = await client.PostAsJsonAsync("ProductDetails/Remove", prod.ProductID).ConfigureAwait(false);
                        if (response.IsSuccessStatusCode)
                        {
                            items.Remove(prod);
                            var message = new
                            {
                                MethodCalled = "ProductService/InventoryController/RemoveProduct",
                                InventoryCount = items.Count,
                                ProductRemovedId = id,
                                Action = "Remove product from the inventory",
                                Status = "Success"
                            };
                            _logger.LogInformation(message.ToString());
                        }
                    }
                    else
                    {
                        _logger.LogInformation("MethodCalled = ProductService/InventoryController/RemoveProduct Message = Product not found!!! Status = Failed");
                        return Ok("No product found in Inventory!!!");
                    }
                    string jsonData = JsonConvert.SerializeObject(items.ToArray());
                    System.IO.File.WriteAllText(@"../../DataFiles/Inventory.json", jsonData);
                }
            }
            catch (Exception e)
            {
                return Ok(e);
            }
            return Ok("Product removed successfully!!!");
        }

        [HttpGet]
        [Route("List")]
        [AllowAnonymous]
        public IActionResult GetProductList()
        {
            List<Product> items = new List<Product>();
            try
            {
                if (System.IO.File.Exists(@"../../DataFiles/Inventory.json"))
                {
                    using (StreamReader r = new StreamReader(@"../../DataFiles/Inventory.json"))
                    {
                        string json = r.ReadToEnd();
                        items = JsonConvert.DeserializeObject<List<Product>>(json);
                    }
                    var message = new
                    {
                        MethodCalled = "ProductService/InventoryController/GetProductList",
                        Action = "Fetch all product list present in the Inventory",
                        Status = "Success"
                    };
                    _logger.LogInformation(message.ToString());
                    return Ok(items);
                }
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.ToString());
                return Ok(e);
            }
            var message1 = new
            {
                MethodCalled = "InventoryController/GetProductList",
                Action = "Fetch all product list present in the Inventory",
                Status = "Failed"
            };
            _logger.LogInformation(message1.ToString());
            return Ok("Inventory is Empty!!!");
        }

        [HttpPost]
        [Route("Details")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductDetails([FromBody] int id)
        {
            List<Product> items = new List<Product>();
            try
            {
                if (System.IO.File.Exists(@"../../DataFiles/Inventory.json"))
                {
                    using (StreamReader r = new StreamReader(@"../../DataFiles/Inventory.json"))
                    {
                        string json = r.ReadToEnd();
                        items = JsonConvert.DeserializeObject<List<Product>>(json);
                    }
                }
            }
            catch (Exception e)
            {
                return Ok(e);
            }
            var item = items.Where(x => x.ProductID == id).SingleOrDefault();

            if(item != null)
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("https://localhost:44362");
                var response = await client.PostAsJsonAsync("ProductDetails/Details", id).ConfigureAwait(false);

                if (response.IsSuccessStatusCode && await response.Content.ReadAsStringAsync()!="")
                {
                    var details = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<ProductDetailsDTO>(details);
                    data.ProductName = item.ProductName;

                    var message = new
                    {
                        MethodCalled = "ProductService/InventoryController/GetProductDetails",
                        Action = "Specific product all details fetched",
                        Status = "Success"
                    };
                    _logger.LogInformation(message.ToString());

                    return Ok(data);
                }
                var d = new
                {
                    ProductId = item.ProductID,
                    ProductName = item.ProductName
                };
                var message1 = new
                {
                    MethodCalled = "ProductService/InventoryController/GetProductDetails",
                    Action = "Specific product partial details fetched",
                    Status = "Success"
                };
                _logger.LogInformation(message1.ToString());
                return Ok(d);
            }
            var message2 = new
            {
                MethodCalled = "ProductService/InventoryController/GetProductDetails",
                Action = "Specific product details fetched",
                Status = "Failed"
            };
            _logger.LogInformation(message2.ToString());
            return Ok("No result found!!!");
        }
    }
}

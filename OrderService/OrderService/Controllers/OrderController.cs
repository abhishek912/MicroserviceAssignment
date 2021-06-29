using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrderService.Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace OrderService.Controllers
{
    [ApiController]
    //[Route("[controller]")]
    public class OrderController : Controller
    {
        private readonly ILogger<OrderController> _logger;
        public OrderController(ILogger<OrderController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        [Route("Product/AddToCart")]
        [Authorize]
        public async Task<IActionResult> ProductAddToCart(int pid, int uid)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:44362");
            var response = await client.PostAsJsonAsync("ProductDetails/Details", pid).ConfigureAwait(false);

            Cart obj = new Cart();
            if (response.IsSuccessStatusCode && await response.Content.ReadAsStringAsync() != "")
            {
                var details = await response.Content.ReadAsStringAsync();
                obj = JsonConvert.DeserializeObject<Cart>(details);
            }

            List<Cart> items = new List<Cart>();
            int newCartId;
            try
            {
                if (System.IO.File.Exists(@"../../DataFiles/Cart.json"))
                {
                    using (StreamReader r = new StreamReader(@"../../DataFiles/Cart.json"))
                    {
                        string json = r.ReadToEnd();
                        items = JsonConvert.DeserializeObject<List<Cart>>(json);
                    }

                    if(items.Count > 0)
                    {
                        var prod = items.Where(x => x.ProductID == pid && x.UserID == uid).SingleOrDefault();
                        if(prod != null)
                        {
                            var message = new
                            {
                                MethodCalled = "OrderService/OrderController/ProductAddToCart",
                                Action = "Add new product to the cart",
                                Status = "Failed",
                                Result = "Product already exist in the cart"
                            };
                            _logger.LogInformation(message.ToString());
                            return Ok("Product already exist in the Cart!!!");
                        }
                    }

                    if (items.Count > 0)
                    {
                        newCartId = items.Last().CartID + 1;
                    }
                    else
                    {
                        newCartId = 1;
                    }
                }
                else
                {
                    newCartId = 1;
                }
                obj.CartID = newCartId;
                obj.ProductID = pid;
                obj.UserID = uid;
                items.Add(obj);
                string jsonData = JsonConvert.SerializeObject(items.ToArray());
                System.IO.File.WriteAllText(@"../../DataFiles/Cart.json", jsonData);
                var message1 = new
                {
                    MethodCalled = "OrderService/OrderController/ProductAddToCart",
                    Action = "Add new product to the cart",
                    Status = "Success",
                    Result = "Product added to the cart"
                };
                _logger.LogInformation(message1.ToString());
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.ToString());
                return Ok(e);
            }

            return Ok("Product Added to Cart successfully!!!");
        }

        [HttpPost]
        [Route("Product/RemoveFromCart")]
        [Authorize]
        public IActionResult ProductRemoveFromCart(int cartId)
        {
            List<Cart> items = new List<Cart>();
            try
            {
                if (System.IO.File.Exists(@"../../DataFiles/Cart.json"))
                {
                    using (StreamReader r = new StreamReader(@"../../DataFiles/Cart.json"))
                    {
                        string json = r.ReadToEnd();
                        items = JsonConvert.DeserializeObject<List<Cart>>(json);
                    }
                    Cart prod = items.Where(x => x.CartID == cartId).SingleOrDefault();
                    if (prod != null)
                    {
                        items.Remove(prod);
                        var message = new
                        {
                            MethodCalled = "OrderService/OrderController/RemoveProductFromCart",
                            Action = "Remove product from the cart",
                            Status = "Success",
                            Result = "Product removed from the cart"
                        };
                        _logger.LogInformation(message.ToString());
                    }
                    else
                    {
                        var message = new
                        {
                            MethodCalled = "OrderService/OrderController/RemoveProductFromCart",
                            Action = "Remove product from the cart",
                            Status = "Failed",
                            Result = "Product not found"
                        };
                        _logger.LogInformation(message.ToString());
                        return Ok("Product cannot be removed from the cart, No product found!!!");
                    }
                    string jsonData = JsonConvert.SerializeObject(items.ToArray());
                    System.IO.File.WriteAllText(@"../../DataFiles/Cart.json", jsonData);
                }
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.ToString());
                return Ok(e);
            }
            return Ok("Product removed from Cart successfully!!!");
        }

        [HttpPost]
        [Route("Order/PlaceOrder")]
        [Authorize]
        public IActionResult PlaceOrder(int cartId, int userId)
        {
            List<Cart> items = new List<Cart>();
            int newOrderId = 0;
            try
            {
                if (System.IO.File.Exists(@"../../DataFiles/Cart.json"))
                {
                    using (StreamReader r = new StreamReader(@"../../DataFiles/Cart.json"))
                    {
                        string json = r.ReadToEnd();
                        items = JsonConvert.DeserializeObject<List<Cart>>(json);
                    }
                    Cart prod = items.Where(x => x.CartID == cartId && x.UserID == userId).SingleOrDefault();
                    if (prod != null)
                    {
                        Orders newOrder = new Orders
                        {
                            UserID = userId,
                            ProductID = prod.ProductID,
                            ProductSize = prod.ProductSize,
                            ProductPrice = prod.ProductPrice,
                            ProductType = prod.ProductType
                        };
                        
                        List<Orders> orders = new List<Orders>();
                        try
                        {
                            if (System.IO.File.Exists(@"../../DataFiles/Orders.json"))
                            {
                                using (StreamReader r = new StreamReader(@"../../DataFiles/Orders.json"))
                                {
                                    string json = r.ReadToEnd();
                                    orders = JsonConvert.DeserializeObject<List<Orders>>(json);
                                }
                                if (orders.Count > 0)
                                {
                                    newOrderId = orders.Last().OrderID + 1;
                                }
                                else
                                {
                                    newOrderId = 1;
                                }
                            }
                            else
                            {
                                newOrderId = 1;
                            }
                            newOrder.OrderID = newOrderId;
                            orders.Add(newOrder);
                            string jsonData = JsonConvert.SerializeObject(orders.ToArray());
                            System.IO.File.WriteAllText(@"../../DataFiles/Orders.json", jsonData);
                            var message = new
                            {
                                MethodCalled = "OrderService/OrderController/PlaceOrder",
                                Action = "Place order present in the cart",
                                Status = "Success",
                                Result = "Order placed successfully, Product removed from the cart"
                            };
                            _logger.LogInformation(message.ToString());
                        }
                        catch (Exception e)
                        {
                            _logger.LogInformation(e.ToString());
                            return Ok(e);
                        }

                        items.Remove(prod);
                    }
                    else
                    {
                        var message = new
                        {
                            MethodCalled = "OrderService/OrderController/PlaceOrder",
                            Action = "Place order present in the cart",
                            Status = "Failed",
                            Result = "Product not found in the cart"
                        };
                        _logger.LogInformation(message.ToString());
                        return Ok("Order cannot be placed, some error occurred!!!");
                    }
                    string jsonData1 = JsonConvert.SerializeObject(items.ToArray());
                    System.IO.File.WriteAllText(@"../../DataFiles/Cart.json", jsonData1);
                }
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.ToString());
                return Ok(e);
            }
            return Ok("Order placed successfully, with the order id: " + newOrderId);
        }

        [HttpPost]
        [Route("Order/CancelOrder")]
        [Authorize]
        public IActionResult CancelOrder(int orderId)
        {
            List<Orders> items = new List<Orders>();
            try
            {
                if (System.IO.File.Exists(@"../../DataFiles/Orders.json"))
                {
                    using (StreamReader r = new StreamReader(@"../../DataFiles/Orders.json"))
                    {
                        string json = r.ReadToEnd();
                        items = JsonConvert.DeserializeObject<List<Orders>>(json);
                    }
                    Orders prod = items.Where(x => x.OrderID == orderId).SingleOrDefault();
                    if (prod != null)
                    {
                        if(prod.OrderStatus == false)
                        {
                            return Ok("Order is already cancelled!!!");
                        }
                        items.Remove(prod);
                        prod.OrderStatus = false;
                        items.Add(prod);

                        var message = new
                        {
                            MethodCalled = "OrderService/OrderController/CancelOrder",
                            Action = "Cancel order if already placed",
                            Status = "Success",
                            Result = "Order cancelled successfully"
                        };
                        _logger.LogInformation(message.ToString());
                    }
                    else
                    {
                        var message = new
                        {
                            MethodCalled = "OrderService/OrderController/CancelOrder",
                            Action = "Cancel order if already placed",
                            Status = "Failed",
                            Result = "Order does not exist"
                        };
                        _logger.LogInformation(message.ToString());
                        return Ok("Order cannot be cancelled!!!");
                    }
                    string jsonData = JsonConvert.SerializeObject(items.ToArray());
                    System.IO.File.WriteAllText(@"../../DataFiles/Orders.json", jsonData);
                }
            }
            catch (Exception e)
            {
                _logger.LogInformation(e.ToString());
                return Ok(e);
            }
            return Ok("Order cancelled successfully!!!");
        }

    }
}

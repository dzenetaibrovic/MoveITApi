using DataLayer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoveIT.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly MoveITModel _context;

        public OrderController(MoveITModel context)
        {
            _context = context;
        }
        [HttpGet]
        public List<Order> Get()
        {
            return _context.Orders.Where(x => x.CustomerId == User.Claims.First().Value).ToList();          
        }
        [HttpGet("getById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            Order order = await _context.Orders.FindAsync(id);

            if (order != null)
            {
                _context.Entry(order).Reference(x => x.Customer).Load();
                string json = JsonConvert.SerializeObject(
                       order,
                       Formatting.None,
                       new JsonSerializerSettings()
                       {
                           ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                           NullValueHandling = NullValueHandling.Ignore
                       });
                return Ok(json);
            }
            else
                return NotFound(id);
        }
        [HttpGet("confirmOrder")]
        public async Task<IActionResult> ConfirmOrder(int id,bool accepted)
        {
            Order order = await _context.Orders.FindAsync(id);

            if (order != null)
            {
                if (accepted)
                    order.Status = Status.Accepted;
                else
                    order.Status = Status.Rejected;

                try
                {
                   await _context.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    return StatusCode(500, new { Error = e.Message });
                }                
                return Ok(order);
            }       
            
            return NotFound(id);
        }
        [HttpPost]
        public async Task<IActionResult> Post(Order order)
        {           
            order.Price = CalculatePrice(order.Distance,order.LivingArea,order.AtticArea, order.Basement,order.Piano);
            order.CustomerId = User.Claims.First().Value;

            _context.Add<Order>(order);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                return StatusCode(500, new { Error = e.Message });
            }

            return CreatedAtAction("GetById", new { id = order.OrderId }, order);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            Order order = await _context.Orders.FindAsync(id);

            if (order != null)
            {
                _context.Remove<Order>(order);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            else
                return NotFound(id);
        }
        private static decimal CalculatePrice(decimal distance, decimal livingArea, decimal atticArea, decimal basement, bool piano)
        {
            decimal distancePrice = 0;

            if (distance < 50)
                distancePrice += 1000 + 10 * distance;
            else if (distance < 100)
                distancePrice += 5000 + 8 * distance;
            else
                distancePrice += 10000 + 7 * distance;            

            decimal volume = livingArea + 2 * atticArea + 2 * basement;

            int cars = 1;

            if (volume >= 50)
            {
                cars += (int)(volume / 50);

                if ((volume % 50) > 0)
                    cars += 1;
            }

            decimal price = distancePrice * cars;

            if (piano)
                price += 5000;

            return price;
        }
    }
}

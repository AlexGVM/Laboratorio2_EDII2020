using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using L2.Models;
using L2.Helpers;

namespace L2.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DrinkController : ControllerBase
    { 
        [HttpGet]
        public IEnumerable<Drink> Get()
        {
            Data.Instance.Items.Clear();
            Data.Instance.Items = Data.Instance.Tree.InOrder(Data.Instance.Tree.Root, Data.Instance.Items);
            return Data.Instance.Items;
        }

        [Route("[controller]/{id}")]
        [HttpGet]
        public object Get(string id)
        {
            var drink = new Drink { Name = id };
            return Data.Instance.Tree.Search(drink) ? Data.Instance.Tree.Seeker(Data.Instance.Tree.Root, drink) : null;
        }

        [HttpPost]
        public void Post([FromBody] Drink drink)
        {
            Data.Instance.Tree.Insert(drink);
        }
    }
}

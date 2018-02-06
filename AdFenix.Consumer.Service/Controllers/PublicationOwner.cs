using AdFenix.Entities.DatabaseContext;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AdFenix.Consumer.Service.Controllers
{
    [Route("api/po")]
    public class PublicationOwner : Controller
    {
        private AdFenixDbConext adFenixDbConext;

        public PublicationOwner(AdFenixDbConext adFenixDbConext)
        {
            this.adFenixDbConext = adFenixDbConext;
        }
        [HttpGet]
        public ActionResult Get()
        {
            return Ok(this.adFenixDbConext.PublicationOwners.ToArray());
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public ActionResult Get(int id)
        {
            var po = this.adFenixDbConext.PublicationOwners.FirstOrDefault(item => item.Id == id);

            if (po == null)
            {
                return NotFound();
            }

            return Ok(po);
        }

    }
}

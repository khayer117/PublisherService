using AdFenix.Entities.DatabaseContext;
using AdFenix.Entities.DomainObject;
using System;
using System.Threading.Tasks;
using AdFenix.Infrastructure.Commands;
using AdFenix.Infrastructure.Mediators;

namespace AdFenix.Consumer.Service.Handlers
{
    public class AddPublicationOwnerCommandHandler:IActionCommandHandler<AddPublicationOwnerCommand>
    {
        private AdFenixDbConext adFenixDbConext;

        public AddPublicationOwnerCommandHandler(AdFenixDbConext adFenixDbConext)
        {
            this.adFenixDbConext = adFenixDbConext;


        }
        public async Task Handle(AddPublicationOwnerCommand command)
        {
            try
            {
                var publicationOwner = new PublicationOwner()
                {
                    HostUrl = command.HostUrl,
                    Name = command.Name
                };
                this.adFenixDbConext.Add(publicationOwner);
                await this.adFenixDbConext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine($"[AddPublicationOwnerCommandHandler.Handle] : {command.Name}");
        }
    }
}

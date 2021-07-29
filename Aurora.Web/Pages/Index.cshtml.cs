using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using Igtampe.Aurora;

namespace Igtampe.Aurora.Web.Pages {
    public class IndexModel: PageModel {

        public OutageCollection Collection { get; private set; }

        public TimeSpan Uptime => DateTime.Now - Collection.LastOutage.End;

        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger) { _logger = logger;}

        public void OnGet() {

            if (!System.IO.File.Exists("Path.txt")) {
                System.IO.File.WriteAllText("Path.txt", "Put a path here!");
                throw new FileNotFoundException("Was unable to find Path.txt, A temporary file has been created. Put the path to the AURLOG to display here.");
            }

            string AurLogLocation = System.IO.File.ReadAllText("Path.txt").Replace("\"","");
            if (!System.IO.File.Exists(AurLogLocation)) {
                throw new FileNotFoundException($"Was unable to find {AurLogLocation}.");
            }

            Collection = OutageCollection.LoadOutageCollection(AurLogLocation,new TimeSpan(365,0,0,0,0));

            //We're probably going to need to create some component here to be able to display all outages

        }
    }
}

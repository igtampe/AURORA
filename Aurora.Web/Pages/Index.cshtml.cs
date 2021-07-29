using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.IO;

namespace Igtampe.Aurora.Web.Pages {
    public class IndexModel: PageModel {

        public OutageCollection Collection { get; private set; }

        public TimeSpan Uptime { get { try { return DateTime.Now - Collection.LastOutage.End; } catch (Exception) { return new TimeSpan(0); } } }
        
        public void OnGet() {

            if (!System.IO.File.Exists("Path.txt")) {
                System.IO.File.WriteAllText("Path.txt", "Put a path here!");
                throw new FileNotFoundException("Was unable to find Path.txt, A temporary file has been created. Put the path to the AURLOG to display here.");
            }

            string AurLogLocation = System.IO.File.ReadAllText("Path.txt").Replace("\"","");
            if (!System.IO.File.Exists(AurLogLocation)) { throw new FileNotFoundException($"Was unable to find {AurLogLocation}.");  }

            Collection = OutageCollection.LoadOutageCollection(AurLogLocation,new TimeSpan(30,0,0,0,0));
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.IO;

namespace Igtampe.Aurora.Web.Pages {
    public class IndexModel: PageModel {

        public OutageCollection Collection { get; private set; }

        public TimeSpan Uptime { get { try { return DateTime.Now - Collection.LastOutage.End; } catch (Exception) { return new TimeSpan(0); } } }
        
        public IActionResult OnGet() {

            if (!System.IO.File.Exists("Path.txt")) {
                System.IO.File.WriteAllText("Path.txt", "Put a path here!");
                return Redirect("./NotConfigured");
            }

            string AurLogLocation = System.IO.File.ReadAllText("Path.txt").Replace("\"","");
            if (AurLogLocation == "Put a path here!") { return Redirect("./NotConfigured"); }
            if (!System.IO.File.Exists(AurLogLocation)) { return Redirect("./NotFound"); }

            Collection = OutageCollection.LoadOutageCollection(AurLogLocation,new TimeSpan(365,0,0,0,0));

            if (Collection.Count == 0) { return Redirect("./empty"); }

            return null;
        }
    }
}

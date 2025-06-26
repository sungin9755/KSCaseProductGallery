using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSCaseProductGallery.Models
{
    public class FirestoreListResponse
    {
        public List<FirestoreDocument> documents { get; set; }
    }

    public class FirestoreDocument
    {
        public FirestoreFields fields { get; set; }
    }

    public class FirestoreFields
    {
        public FirestoreString name { get; set; }
        public FirestoreString year { get; set; }
        public FirestoreString material { get; set; }
        public FirestoreString feature { get; set; }
        public FirestoreString description { get; set; }
        public FirestoreString image { get; set; }
    }

    public class FirestoreString
    {
        public string stringValue { get; set; }
    }
}

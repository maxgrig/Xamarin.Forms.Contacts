using System;
using System.Collections.Generic;

namespace Plugin.ContactService
{
    public class Contact
    {
        public string Name { get; set; }
        public string PhotoUri { get; set; }
        public string PhotoUriThumbnail { get; set; }
        public string Number { get; set; }
        public string Email { get; set; }

        public List<ContactAddress> Addresses { get; set; } = new List<ContactAddress>();

        public List<string> Numbers { get; set; } = new List<string>();
        public List<string> Emails { get; set; } = new List<string>();

        public override string ToString()
        {
            return Name;
        }
    }

    public class ContactAddress
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }
    }
}

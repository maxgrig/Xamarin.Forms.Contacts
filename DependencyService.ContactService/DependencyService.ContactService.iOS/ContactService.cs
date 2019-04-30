using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Contacts;
using Foundation;

namespace Plugin.ContactService
{
    public class ContactService : IContactService
    {
        public Task<IList<Contact>> GetContactListAsync()
        {
            return Task.Run(() => GetContactList());
        }

        public IList<Contact> GetContactList()
        {
            try
            {
                var keysToFetch = new[]
                {
                    CNContactKey.GivenName,
                    CNContactKey.MiddleName,
                    CNContactKey.FamilyName,
                    CNContactKey.EmailAddresses,
                    CNContactKey.PhoneNumbers,
                    CNContactKey.PostalAddresses
                };
                var contactList = ReadRawContactList(keysToFetch);

                return GetContacts(contactList).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return new List<Contact>();
            }
        }

        private static IEnumerable<CNContact> ReadRawContactList(IEnumerable<NSString> keysToFetch)
        {
            //var containerId = new CNContactStore().DefaultContainerIdentifier;
            // using the container id of null to get all containers.
            // If you want to get contacts for only a single container type, you can specify that here
            using (var store = new CNContactStore())
            {
                var allContainers = store.GetContainers(null, out _);
                return ContactListFromAllContainers(keysToFetch, allContainers, store);
            }
        }

        private static IEnumerable<CNContact> ContactListFromAllContainers(
            IEnumerable<NSString> keysToFetch,
            IEnumerable<CNContainer> allContainers,
            CNContactStore store)
        {
            var nsStrings = keysToFetch.ToList();
            var contactList = new List<CNContact>();
            foreach (var container in allContainers)
            {
                var contacts = ReadFromContainer(nsStrings, container, store);
                contactList.AddRange(contacts);
            }

            return contactList;
        }

        private static IEnumerable<CNContact> ReadFromContainer(
            IEnumerable<NSString> keysToFetch,
            CNContainer container,
            CNContactStore store)
        {
            try
            {
                using (var predicate = CNContact.GetPredicateForContactsInContainer(container.Identifier))
                {
                    return store.GetUnifiedContacts(predicate, keysToFetch.ToArray(), out _);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return null;
        }

        private static IEnumerable<Contact> GetContacts(IEnumerable<CNContact> contactList)
        {
            return contactList.Select(GetContact)
                              .Where(contact => !string.IsNullOrWhiteSpace(contact.Name));
        }

        private static Contact GetContact(CNContact item)
        {
            var numbers = GetNumbers(item.PhoneNumbers).ToList();
            var emails = GetNumbers(item.EmailAddresses).ToList();
            var addresses = GetAddresses(item.PostalAddresses).ToList();

            var contact = new Contact
            {
                Name = (string.IsNullOrWhiteSpace(item.GivenName) ? "" : item.GivenName)
                     + (string.IsNullOrWhiteSpace(item.MiddleName) ? "" : $" {item.MiddleName}")
                     + (string.IsNullOrWhiteSpace(item.FamilyName) ? "" : $" {item.FamilyName}"),
                Numbers = numbers,
                Number = numbers.LastOrDefault(),
                Emails = emails,
                Email = emails.LastOrDefault(),
                Addresses = addresses
            };

            return contact;
        }

        private static IEnumerable<ContactAddress> GetAddresses(CNLabeledValue<CNPostalAddress>[] items)
        {
            if (items == null) yield break;

            foreach (var item in items)
            {
                var value = item?.Value;
                if (value != null)
                {
                    yield return new ContactAddress
                    {
                        Street = value.Street,
                        City = value.City,
                        State = value.State,
                        Zip = value.PostalCode,
                        Country = value.Country
                    };
                }
            }
        }

        private static IEnumerable<string> GetNumbers(CNLabeledValue<NSString>[] items)
        {
            if (items == null) yield break;

            foreach (var item in items)
            {
                var value = item?.Value?.ToString();
                if (value != null) yield return value ?? "";
            }
        }

        private static IEnumerable<string> GetNumbers(CNLabeledValue<CNPhoneNumber>[] items)
        {
            if (items == null) yield break;

            foreach (var number in items)
            {
                var value = number?.Value?.StringValue ?? "";
                if (value != null) yield return value;
            }
        }
    }
}
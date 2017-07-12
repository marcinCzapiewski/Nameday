using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Contacts;

namespace Nameday
{
    public class MainPageData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<NamedayModel> Namedays { get; set; }

        private List<NamedayModel> _allNamedays = new List<NamedayModel>();

        private string _greeting = "Hello World!";
        public string Greeting
        {
            get { return _greeting; }
            set
            {
                _greeting = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Greeting)));
            }
        }

        private NamedayModel _selectedNameday;
        public NamedayModel SelectedNameday
        {
            get { return _selectedNameday; }
            set
            {
                _selectedNameday = value;

                if (value == null)
                    Greeting = "Hello World!";
                else
                    Greeting = "Hello " + value.NamesAsString;

                UpdateContacts();
            }
        }

        private string _filter;

        public string Filter
        {
            get { return _filter; }
            set
            {
                if (value == _filter)
                    return;

                _filter = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Filter)));

                PerformFiltering();
            }
        }

        public ObservableCollection<ContactEx> Contacts { get; } =
            new ObservableCollection<ContactEx>();

        public MainPageData()
        {
            Namedays = new ObservableCollection<NamedayModel>();

            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                Contacts = new ObservableCollection<ContactEx>
                {
                    new ContactEx("Contact", "1"),
                    new ContactEx("Contact", "2"),
                    new ContactEx("Contact", "3")
                };

                for (int month = 1; month <= 12; month++)
                {
                    _allNamedays.Add(new NamedayModel(month, 1, new string[] { "Adam" }));
                    _allNamedays.Add(new NamedayModel(month, 24, new string[] { "Eve", "Andrew" }));
                }
                PerformFiltering();
            }
            else
                LoadData();
        }

        public async void LoadData()
        {
            _allNamedays = await NamedayRepository.GetAllNamedaysAsync();
            PerformFiltering();
        }

        private void PerformFiltering()
        {
            if (_filter == null)
                _filter = "";

            var lowerCaseFilter = Filter.ToLowerInvariant().Trim();

            var result =
                _allNamedays.Where(d => d.NamesAsString.ToLowerInvariant()
                .Contains(lowerCaseFilter)).ToList();

            var toRemove = Namedays.Except(result).ToList();

            foreach (var x in toRemove)
                Namedays.Remove(x);

            var resultCount = result.Count;
            for (int i = 0; i < resultCount; i++)
            {
                var resultItem = result[i];
                if (i + 1 > Namedays.Count || !Namedays[i].Equals(resultItem))
                    Namedays.Insert(i, resultItem);
            }
        }

        private async void UpdateContacts()
        {
            Contacts.Clear();

            if (SelectedNameday != null)
            {
                var contactStore =
                    await ContactManager.RequestStoreAsync(ContactStoreAccessType.AllContactsReadOnly);

                foreach (var name in SelectedNameday.Names)
                    foreach (var contact in await contactStore.FindContactsAsync(name))
                        Contacts.Add(new ContactEx(contact));
            }
        }
    }
}

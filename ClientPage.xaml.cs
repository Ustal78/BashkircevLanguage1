using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BashkircevLanguage
{
    /// <summary>
    /// Логика взаимодействия для ClientPage.xaml
    /// </summary>
    
    public partial class ClientPage : Page
    {
        int currentPage = 1;
        int itemsPerPage = 10;
        int totalPages = 1;
        public ClientPage()
        {
            InitializeComponent();

            FilterCombo.Items.Add("Все");
            FilterCombo.Items.Add("мужской");
            FilterCombo.Items.Add("женский");

            FilterCombo.SelectedIndex = 0;
            SortCombo.SelectedIndex = 0;

            CountCombo.SelectedIndex = 0;

            UpdateClients();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddEditPage());
        }

        private void UpdateClients()
        {
            var clients = BashkircevLanguageEntities.GetContext().Client.ToList();

            string searchText = SearchBox.Text.ToLower();

            string CleanPhone(string phone)
            {
                if (string.IsNullOrEmpty(phone)) return "";
                return System.Text.RegularExpressions.Regex.Replace(phone, @"[^\d]", "");
            }

            string cleanSearch = System.Text.RegularExpressions.Regex.Replace(SearchBox.Text, @"[^\d]", "");

            clients = clients.Where(c => (c.FirstName != null && c.FirstName.ToLower().Contains(searchText)) || (c.LastName != null && c.LastName.ToLower().Contains(searchText)) ||
                (c.Patronymic != null && c.Patronymic.ToLower().Contains(searchText)) ||
                (c.Email != null && c.Email.ToLower().Contains(searchText)) ||

                (!string.IsNullOrEmpty(cleanSearch) && !string.IsNullOrEmpty(c.Phone) && CleanPhone(c.Phone).Contains(cleanSearch))).ToList();

            if (FilterCombo.SelectedIndex > 0)
            {
                if (FilterCombo.SelectedIndex == 1)
                    clients = clients.Where(c => c.GenderCode == "м").ToList();

                if (FilterCombo.SelectedIndex == 2) 
                    clients = clients.Where(c => c.GenderCode == "ж").ToList();
            }

            switch (SortCombo.SelectedIndex)
            {
                case 1:
                    clients = clients.OrderBy(c => c.LastName).ToList();
                    break;

                case 2:
                    clients = clients.OrderByDescending(c => BashkircevLanguageEntities.GetContext().ClientService.Where(x => x.ClientID == c.ID).Select(x => x.StartTime).DefaultIfEmpty(DateTime.MinValue).Max()).ToList();
                    break;

                case 3:
                    clients = clients.OrderByDescending(c => BashkircevLanguageEntities.GetContext().ClientService.Count(x => x.ClientID == c.ID)).ToList();
                    break;
            }

            int totalCount = BashkircevLanguageEntities.GetContext().Client.Count();
            int filteredCount = clients.Count;

            totalPages = (int)Math.Ceiling((double)filteredCount / itemsPerPage);

            if (itemsPerPage == int.MaxValue)
            {
                ClientListView.ItemsSource = clients;

                CountText.Text = $"{clients.Count} из {clients.Count}";
            }
            else
            {
                var pageList = clients.Skip((currentPage - 1) * itemsPerPage).Take(itemsPerPage).ToList();

                ClientListView.ItemsSource = pageList;

                CountText.Text = $"{pageList.Count} из {clients.Count}";
            }


            DrawPageButtons();
        }

        private void DrawPageButtons()
        {
            PageNumbers.Children.Clear();

            for (int i = 1; i <= totalPages; i++)
            {
                Button btn = new Button();
                btn.Content = i;
                btn.Tag = i;
                btn.Width = 25;
                btn.Margin = new Thickness(2);
                btn.Click += PageButton_Click;

                if (i == currentPage)
                    btn.Background = Brushes.LightBlue;

                PageNumbers.Children.Add(btn);
            }
        }

        private void PageButton_Click(object sender, RoutedEventArgs e)
        {
            currentPage = (int)(sender as Button).Tag;
            UpdateClients();
        }

        private void PrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                UpdateClients();
            }
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage < totalPages)
            {
                currentPage++;
                UpdateClients();
            }
        }

        private void CountCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CountCombo.SelectedItem is ComboBoxItem item)
            {
                if (item.Content.ToString() == "Все")
                    itemsPerPage = int.MaxValue;
                else
                    itemsPerPage = int.Parse(item.Content.ToString());

                currentPage = 1;
                UpdateClients();
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            var client = (sender as Button).DataContext as Client;

            var hasVisits = BashkircevLanguageEntities.GetContext().ClientService.Any(x => x.ClientID == client.ID);

            if (hasVisits)
            {
                MessageBox.Show("Нельзя удалить клиента — есть посещения!");
                return;
            }

            if (MessageBox.Show("Удалить клиента?", "Внимание",
                MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                BashkircevLanguageEntities.GetContext().Client.Remove(client);
                BashkircevLanguageEntities.GetContext().SaveChanges();

                UpdateClients();
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateClients();
        }

        private void FilterCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateClients();
        }

        private void SortCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateClients();
        }


    }
}

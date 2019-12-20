using Plugin.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace GpsTryForms
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class OptionsPage : ContentPage
	{
        public string CustomRecipient;

        public OptionsPage ()
		{
			InitializeComponent ();
            NavigationPage.SetHasNavigationBar(this, false);
            CustomRecipient = CrossSettings.Current.GetValueOrDefault("CustomRecipient", "102");
            RecipientInput.Text = CustomRecipient;
            EmergencyRecipientInput.Text = CrossSettings.Current.GetValueOrDefault("EmergencyRecipient", "102");
        }

        public async void GoBack(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        public void ChangeRecipients(object sender, EventArgs e)
        {
            CustomRecipient = RecipientInput.Text;
            string EmergencyRecipient = EmergencyRecipientInput.Text;
            CrossSettings.Current.AddOrUpdateValue("CustomRecipient", CustomRecipient);
            CrossSettings.Current.AddOrUpdateValue("EmergencyRecipient", EmergencyRecipient);
        }
    }
}
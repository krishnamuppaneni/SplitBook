﻿using SplitBook.Request;
using SplitBook.Utilities;
using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SplitBook.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        private OAuthRequest authorize;
        public LoginPage()
        {
            this.InitializeComponent();
            authorize = new OAuthRequest();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendView("LoginPage");
            base.OnNavigatedTo(e);
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            progressRing.IsActive = true;
            await authorize.GetRequestToken(RequestTokenReceived, OnError);
        }

        private async void RequestTokenReceived(Uri uri)
        {
            progressRing.IsActive = false;
            string requestToken;
            if ((requestToken = await SplitwiseAuthenticationBroker.AuthenticateAsync(uri)) != null)
            {
                await authorize.GetAccessToken(requestToken, AccessTokenReceived, OnError);
            }
        }

        private void AccessTokenReceived(string accessToken, string accessTokenSecret)
        {
            Helpers.AccessToken = accessToken;
            Helpers.AccessTokenSecret = accessTokenSecret;
            App.accessToken = Helpers.AccessToken;
            App.accessTokenSecret = Helpers.AccessTokenSecret;
            this.Frame.Navigate(typeof(MainPage), true);
        }

        private async void OnError(Exception ex)
        {
            progressRing.IsActive = false;
            MessageDialog messageDialog = new MessageDialog(ex.Message, "Error");
            await messageDialog.ShowAsync();
        }
    }
}

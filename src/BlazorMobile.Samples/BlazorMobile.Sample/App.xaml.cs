﻿using BlazorMobile.Services;
using System;
using Xamarin.Forms;

namespace BlazorMobile.Sample
{
	public partial class App : Application
	{
        public App()
        {
            InitializeComponent();

#if DEBUG
            WebApplicationFactory.EnableDebugFeatures();
#endif
            WebApplicationFactory.SetHttpPort(8888);

            //Register Blazor application package resolver
            WebApplicationFactory.RegisterAppStreamResolver(() =>
            {
                //This app assembly
                var assembly = typeof(App).Assembly;

                //Name of our current Blazor package in this project, stored as an "Embedded Resource"
                //The file is resolved through AssemblyName.NamespaceFolder.app.zip
                return assembly.GetManifestResourceStream($"{assembly.GetName().Name}.Package.app.zip");
            });

            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
            WebApplicationFactory.StartWebServer();
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
            WebApplicationFactory.StopWebServer();
        }

        protected override void OnResume()
        {
            WebApplicationFactory.StartWebServer();
        }
    }
}


using CefSharp;
using Dynamo.ViewModels;
using Dynamo.Wpf.Interfaces;
using Dynamo.Wpf.ViewModels.Core;
using Microsoft.Practices.Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dynamo.HostedContents
{
    /// <summary>
    /// Interaction logic for LibraryContainer.xaml
    /// </summary>
    public partial class LibraryContainer : UserControl, ILibraryContainer
    {
        private bool browserLoaded = false;
        private string loadedTypesJson = String.Empty;
        private string loadedTypesRaw = String.Empty;
        private LibraryContainerViewModel viewModel = null;

        public LibraryContainer(LibraryContainerViewModel libraryContainerViewModel)
        {
            this.viewModel = libraryContainerViewModel;

            if (!Cef.IsInitialized)
            {
                var settings = new CefSettings
                {
#if DEBUG
                    RemoteDebuggingPort = 8088
#endif
                };
                Cef.Initialize(settings);
            }

            InitializeComponent();

            refreshButton.Click += (sender, e) => webBrowser.Reload(true); // Force refresh.
            webBrowser.MenuHandler = new DynamoLibraryContextMenuHandler();
            webBrowser.RegisterJsObject("boundContainer", this);
            webBrowser.FrameLoadEnd += OnWebBrowserFrameLoadEnd;
        }

        #region Public ILibraryContainer Members

        public event EventHandler WebBrowserLoaded;

        public void SetLoadedTypesJson(string loadedTypesJson)
        {
            this.loadedTypesJson = loadedTypesJson;
        }

        public void SetLoadedTypesRaw(string loadedTypesRaw)
        {
            this.loadedTypesRaw = loadedTypesRaw;
        }

        #endregion

        #region Gateway Methods: from JavaScript to .NET (public methods)

        public string GetLoadedTypesJson()
        {
            return loadedTypesJson;
        }

        public string GetLoadedTypesRaw()
        {
            return loadedTypesRaw;
        }

        public void OnClicked(string id)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                viewModel.OnLibraryContainerClicked(id);
            }));
        }

        #endregion

        #region Event Handlers

        private void OnWebBrowserFrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            browserLoaded = true;

            var loadEventHandler = WebBrowserLoaded;
            if (loadEventHandler != null) loadEventHandler(this, new EventArgs());

            if (e.Frame.IsMain)
            {

            }
        }

        #endregion

        private class DynamoLibraryContextMenuHandler : IContextMenuHandler
        {
            public void OnBeforeContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model)
            {
                // Disable right-click context menu on the library browser
                model.Clear();
            }

            public bool OnContextMenuCommand(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, CefMenuCommand commandId, CefEventFlags eventFlags)
            {
                return false;
            }

            public void OnContextMenuDismissed(IWebBrowser browserControl, IBrowser browser, IFrame frame)
            {
            }

            public bool RunContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model, IRunContextMenuCallback callback)
            {
                return false;
            }
        }
    }
}

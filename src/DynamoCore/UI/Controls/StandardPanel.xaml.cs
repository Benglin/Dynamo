using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dynamo.Nodes.Search;
using Dynamo.Search;
using Dynamo.Search.SearchElements;
using Dynamo.ViewModels;

namespace Dynamo.UI.Controls
{
    /// <summary>
    /// Interaction logic for StandardPanel.xaml
    /// </summary>
    public partial class StandardPanel : UserControl
    {
        #region Constants

        private const string ActionHeaderTag = "Action";
        private const string QueryHeaderTag = "Query";
        private const int TruncatedMembersCount = 4;

        #endregion

        // Specifies if all Lists (CreateMembers, QueryMembers and ActionMembers) are not empty
        // and should be presented on StandardPanel.
        private bool areAllListsPresented;
        private StandardPanelViewModel viewModel;

        public StandardPanel()
        {
            InitializeComponent();
        }

        private void OnHeaderMouseDown(object sender, MouseButtonEventArgs e)
        {
            // In this cases at addCetgoryList will be situated not more one
            // list. We don't need switch between lists.
            if (!areAllListsPresented)
                return;

            var senderTag = (sender as FrameworkElement).Tag.ToString();

            // User clicked on selected header. No need to change ItemsSource.
            if (senderTag == viewModel.CurrentDisplayMode.ToString())
                return;

            if (senderTag == QueryHeaderTag)
            {
                viewModel.CurrentDisplayMode = StandardPanelViewModel.DisplayMode.Query;
                secondaryMembers.ItemsSource = viewModel.QueryMembers;
            }
            else
            {
                viewModel.CurrentDisplayMode = StandardPanelViewModel.DisplayMode.Action;
                secondaryMembers.ItemsSource = viewModel.ActionMembers;
            }

            TruncateSecondaryMembers();
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var listBoxItem = sender as ListBoxItem;
            if (listBoxItem == null) return;

            var searchElement = listBoxItem.DataContext as SearchElementBase;
            if (searchElement != null)
                searchElement.Execute();
        }

        private void OnListBoxItemMouseEnter(object sender, MouseEventArgs e)
        {
            ListBoxItem fromSender = sender as ListBoxItem;
            libraryToolTipPopup.PlacementTarget = fromSender;
            libraryToolTipPopup.SetDataContext(fromSender.DataContext);
        }

        private void OnPopupMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            libraryToolTipPopup.SetDataContext(null);
        }

        private void GridDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            viewModel = this.DataContext as StandardPanelViewModel;
            if (viewModel == null)
                return;

            bool hasCreateMembers = viewModel.CreateMembers.Any();
            bool hasActionMembers = viewModel.ActionMembers.Any();
            bool hasQueryMembers = viewModel.QueryMembers.Any();

            areAllListsPresented = hasCreateMembers && hasActionMembers && hasQueryMembers;

            // Hide all headers by default.
            viewModel.IsPrimaryHeaderVisible = false;
            viewModel.IsSecondaryHeaderLeftVisible = false;
            viewModel.IsSecondaryHeaderRightVisible = false;

            // Set default values.
            viewModel.PrimaryHeaderGroup = SearchElementGroup.Create;
            viewModel.SecondaryHeaderLeftGroup = SearchElementGroup.Query;
            viewModel.SecondaryHeaderRightGroup = SearchElementGroup.Action;

            viewModel.CurrentDisplayMode = StandardPanelViewModel.DisplayMode.None;

            // Case when CreateMembers list is not empty.
            // We should present CreateMembers in primaryMembers.            
            if (hasCreateMembers)
            {
                viewModel.IsPrimaryHeaderVisible = true;
                primaryMembers.ItemsSource = viewModel.CreateMembers;

                if (hasQueryMembers)
                {
                    viewModel.IsSecondaryHeaderLeftVisible = true;

                    secondaryMembers.ItemsSource = viewModel.QueryMembers;
                }

                if (hasActionMembers)
                {
                    viewModel.IsSecondaryHeaderRightVisible = true;

                    if (!hasQueryMembers)
                        secondaryMembers.ItemsSource = viewModel.ActionMembers;
                }

                // For case when all lists are presented we should specify
                // correct CurrentDisplayMode.
                if (hasQueryMembers && hasActionMembers)
                    viewModel.CurrentDisplayMode = StandardPanelViewModel.DisplayMode.Query;

                TruncateSecondaryMembers();
                return;
            }

            // Case when CreateMembers list is empty and ActionMembers list isn't empty.
            // ActionMembers will be presented in primaryMembers.
            // Depending on availibility of QueryMembers it will be shown as secondaryHeaderLeft.
            if (hasActionMembers)
            {
                viewModel.IsPrimaryHeaderVisible = true;
                viewModel.PrimaryHeaderGroup = SearchElementGroup.Action;
                primaryMembers.ItemsSource = viewModel.ActionMembers;

                if (hasQueryMembers)
                {
                    viewModel.IsSecondaryHeaderLeftVisible = true;
                    secondaryMembers.ItemsSource = viewModel.QueryMembers;
                }

                TruncateSecondaryMembers();
                return;
            }

            // Case when CreateMembers and ActionMembers lists are empty.
            // If QueryMembers is not empty the list will be presented in primaryMembers. 
            if (hasQueryMembers)
            {
                viewModel.PrimaryHeaderGroup = SearchElementGroup.Query;
                primaryMembers.ItemsSource = viewModel.QueryMembers;
            }
        }

        private void OnMoreButtonClick(object sender, RoutedEventArgs e)
        {
            if (viewModel.HiddenMembers != null)
            {
                var members = secondaryMembers.ItemsSource as IEnumerable<BrowserInternalElement>;
                var allMembers = members.ToList();
                allMembers.AddRange(viewModel.HiddenMembers);

                secondaryMembers.ItemsSource = allMembers;
                viewModel.HiddenMembers = null;
            }
        }

        private void TruncateSecondaryMembers()
        {
            var members = secondaryMembers.ItemsSource as IEnumerable<BrowserInternalElement>;
            if (members != null && members.Count() > TruncatedMembersCount)
            {
                viewModel.HiddenMembers = members.Skip(TruncatedMembersCount);
                secondaryMembers.ItemsSource = members.Take(TruncatedMembersCount);
            }
        }
    }
}

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
﻿using Dynamo.Search.SearchElements;
using Dynamo.ViewModels;
using Dynamo.Search;
using Dynamo.Utilities;
using Dynamo.Nodes.Search;

namespace Dynamo.UI.Views
{
    /// <summary>
    /// Interaction logic for LibrarySearchView.xaml
    /// </summary>
    public partial class LibrarySearchView : UserControl
    {
        public LibrarySearchView()
        {
            InitializeComponent();
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var listBoxItem = sender as ListBoxItem;
            if (listBoxItem == null) return;

            var searchElement = listBoxItem.DataContext as SearchElementBase;
            if (searchElement != null)
            {
                searchElement.Execute();
                e.Handled = true;
            }
        }

        private void OnClassButtonCollapse(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var classButton = sender as ListViewItem;
            if ((classButton == null) || !classButton.IsSelected) return;

            classButton.IsSelected = false;
            e.Handled = true;
        }

        private void OnPreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void OnNoMatchFoundButtonClick(object sender, RoutedEventArgs e)
        {
            var searchViewModel = this.DataContext as SearchViewModel;

            // Clear SearchText in ViewModel, as result search textbox clears as well.
            searchViewModel.SearchText = "";
        }

        // Here we can move left, right, up, down.
        private void OnClassButtonKeyDown(object sender, KeyEventArgs e)
        {
            var classButton = sender as ListViewItem;
            var listViewButtons = WPF.FindUpVisualTree<ListView>(classButton);
            var selectedIndex = listViewButtons.Items.IndexOf((sender as FrameworkElement).DataContext);
            int itemsPerRow = (int)Math.Floor(listViewButtons.ActualWidth / classButton.ActualWidth);

            int newIndex = GetIndexNextSelectedItem(e.Key, selectedIndex, itemsPerRow);

            if ((newIndex < 0) || (newIndex > listViewButtons.Items.Count)) return;
            //    listViewButtons.SelectedIndex = newIndex;

            // Set focus on new selected item.
            var item = listViewButtons.ItemContainerGenerator.ContainerFromIndex(newIndex) as ListViewItem;
            item.Focus();

            e.Handled = true;
            return;
        }
        
        private int GetIndexNextSelectedItem(Key key, int selectedIndex, int itemsPerRow)
        {
            int newIndex = -1;
            int numberOfSelectedRow = selectedIndex / itemsPerRow + 1;

            switch (key)
            {
                case Key.Right:
                    {
                        newIndex = selectedIndex + 1;
                        int availableIndex = numberOfSelectedRow * itemsPerRow - 1;
                        if (newIndex > availableIndex) newIndex = selectedIndex;
                        break;
                    }
                case Key.Left:
                    {
                        newIndex = selectedIndex - 1;
                        int availableIndex = (numberOfSelectedRow - 1) * itemsPerRow;
                        if (newIndex < availableIndex) newIndex = selectedIndex;
                        break;
                    }
                case Key.Down:
                    {
                        newIndex = selectedIndex + itemsPerRow + 1;
                        // +1 because one of items is always ClassInformation.
                        break;
                    }
                case Key.Up:
                    {
                        newIndex = selectedIndex - itemsPerRow;
                        break;
                    }
            }
            return newIndex;
        }

        // Here we can move up and down.
        private void MethodButtonKeyDown(object sender, KeyEventArgs e)
        {
            // Case 1. We are on top result now and press Down.
            if (sender is ListBox)
                if ((sender as ListBox).Name == "topResultListBox")
                    if (e.Key == Key.Down)
                    {
                        if ((this.DataContext as SearchViewModel).Model.SearchRootCategories.Count == 0) return;

                        // Find first category.
                        var foundCategories = WPF.FindChild<ListView>(this, "CategoryListView");
                        var firstCategory = foundCategories.ItemContainerGenerator.ContainerFromIndex(0) as ListViewItem;

                        // Find first class of first category.
                        var firstCatClasses = WPF.FindChild<ListView>(firstCategory, "SubCategoryListView");
                        if (firstCatClasses.Items.Count > 0)
                        {
                            // Set Focus on first class of category.
                            (firstCatClasses.ItemContainerGenerator.ContainerFromIndex(0) as ListViewItem).Focus();
                            e.Handled = true;
                            return;
                        }
                        // If category doesn't consist of classes, then we set focus on first method member.
                        else
                        {
                            var firstCatMemberGroup = WPF.FindChild<ListBox>(firstCategory, "MemberGroupsListBox");
                            if (firstCatMemberGroup.Items.Count > 0)
                            {
                                var members = WPF.FindChild<ListBox>(firstCatMemberGroup, "MembersListBox");
                                // Set Focus on first member of member group.
                                var firstMember = members.ItemContainerGenerator.ContainerFromIndex(0) as ListBoxItem;
                                firstMember.Focus();
                                e.Handled = true;
                                return;
                            }
                        }
                    }

            // Case 2. We are inside of any member group, but not top result.
            if (sender is ListBox)
            {
                if (e.Key == Key.Down)
                // That means we need to set focus to next member group from the same category.
                // Or move to next category.
                {

                }
                // var listBoxMemberGroup = WPF.FindUpVisualTree<ListView>(sender is ListBox);
            }
        }
    }
}

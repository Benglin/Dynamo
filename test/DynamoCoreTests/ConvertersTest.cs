﻿using Dynamo.Controls;
using Dynamo.Nodes.Search;
using Dynamo.Search.SearchElements;
using Dynamo.Search;
using Dynamo.ViewModels;
using Dynamo.Utilities;
using Dynamo.Models;
using Dynamo.Interfaces;

using NUnit.Framework;
using System.Windows;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Controls;

namespace Dynamo
{
    class ConvertersTest
    {
        [Test]
        public void SearchResultsToVisibilityConverterTest()
        {
            SearchResultsToVisibilityConverter converter = new SearchResultsToVisibilityConverter();
            int numberOfFoundSearchCategories = 0;
            bool addonsVisibility = false;
            string searchText = "";
            object result;

            object[] array = { numberOfFoundSearchCategories, addonsVisibility, searchText };

            //1. There are no found search categories. Addons are invisible. Search text is empty.
            //2. There are no found search categories. Addons are invisible. Search text is not empty.
            //3. There are no found search categories. Addons are visible. Search text is empty.
            //4. There are no found search categories. Addons are visible. Search text is not empty.
            //5. There are some search categories. Addons are invisible. Search text is not empty.
            //6. There are some search categories. Addons are invisible. Search text is empty.
            //7. There are some search categories. Addons are visible. Search text is not empty.
            //8. There are some search categories. Addons are visible. Search text is empty.

            // 1 case
            result = converter.Convert(array, null, null, null);
            Assert.AreEqual(Visibility.Collapsed, result);

            // 2 case
            searchText = "search text";
            array[2] = searchText;
            result = converter.Convert(array, null, null, null);
            Assert.AreEqual(Visibility.Visible, result);

            // 3 case
            searchText = "";
            array[2] = searchText;
            addonsVisibility = true;
            array[1] = addonsVisibility;
            result = converter.Convert(array, null, null, null);
            Assert.AreEqual(Visibility.Collapsed, result);

            // 4 case
            searchText = "search text";
            array[2] = searchText;
            result = converter.Convert(array, null, null, null);
            Assert.AreEqual(Visibility.Collapsed, result);

            // 5 case
            numberOfFoundSearchCategories = 5;
            array[0] = numberOfFoundSearchCategories;
            addonsVisibility = false;
            array[1] = addonsVisibility;
            result = converter.Convert(array, null, null, null);
            Assert.AreEqual(Visibility.Collapsed, result);

            // 6 case
            searchText = "";
            array[2] = searchText;
            result = converter.Convert(array, null, null, null);
            Assert.AreEqual(Visibility.Collapsed, result);

            // 7 case
            addonsVisibility = true;
            array[1] = addonsVisibility;
            searchText = "search text";
            array[2] = searchText;
            result = converter.Convert(array, null, null, null);
            Assert.AreEqual(Visibility.Collapsed, result);

            // 8 case
            searchText = "";
            array[2] = searchText;
            result = converter.Convert(array, null, null, null);
            Assert.AreEqual(Visibility.Collapsed, result);
        }

        [Test]
        public void FullyQualifiedNameToDisplayConverterTest()
        {
            string name = "";
            string parameter = "";
            FullyQualifiedNameToDisplayConverter converter = new FullyQualifiedNameToDisplayConverter();
            object result;

            //1. Class name is "ClassA.ForTooltip". Parameter is "ToolTip".
            //2. Class name is "ClassWithReallyLoooooongName.ForTooltip". Parameter is "ToolTip".
            //3. Class name is "ClassA". Parameter is "ClassButton".
            //4. Class name is "ClAaB". Parameter is "ClassButton".
            //5. Class name is "ClassLongName". Parameter is "ClassButton".
            //6. Class name is "ClassWithReallyLongName". Parameter is "ClassButton".
            //7. Class name is empty. Parameter is "ToolTip".
            //8. Class name is empty. Parameter is "ClassButton".
            //9. Class name is empty. Parameter is empty.

            // 1 case
            name = "ClassA.ForTooltip";
            parameter = "ToolTip";
            result = converter.Convert(name, null, parameter, null);
            Assert.AreEqual("ClassA.ForTooltip", result);

            // 2 case
            name = "ClassWithReallyLoooooongName.ForTooltip";
            parameter = "ToolTip";
            result = converter.Convert(name, null, parameter, null);
            Assert.AreEqual("ClassWithReallyLoooooongName.\nForTooltip", result);

            // 3 case
            name = "ClassA";
            parameter = "ClassButton";
            result = converter.Convert(name, null, parameter, null);
            Assert.AreEqual("Class A", result);

            // 4 case
            name = "ClAaB";
            parameter = "ClassButton";
            result = converter.Convert(name, null, parameter, null);
            Assert.AreEqual("Cl Aa B", result);

            // 5 case
            name = "ClassLongName";
            parameter = "ClassButton";
            result = converter.Convert(name, null, parameter, null);
            Assert.AreEqual("Class \nLong Name", result);

            // 6 case
            name = "ClassWithReallyLongName";
            parameter = "ClassButton";
            result = converter.Convert(name, null, parameter, null);
            Assert.AreEqual("Class \nWith Really ..", result);

            // 7 case
            name = "";
            parameter = "ToolTip";
            result = converter.Convert(name, null, parameter, null);
            Assert.AreEqual("", result);

            // 8 case
            name = "";
            parameter = "ClassButton";
            result = converter.Convert(name, null, parameter, null);
            Assert.AreEqual("", result);

            // 9 case
            name = "";
            parameter = "";
            Assert.Throws<NotImplementedException>(delegate { converter.Convert(name, null, parameter, null); });
        }

        [Test]
        public void InOutParamTypeConverterTest()
        {
            string input = "";
            string parameter = "";
            InOutParamTypeConverter converter = new InOutParamTypeConverter();
            object result;

            //1. Input is empty. Parameter is empty.
            //2. Input is "input". Parameter is empty.
            //3. Input is "none". Parameter is empty.
            //4. Input is "none". Parameter is "inputParam".
            //5. Input is "someInput". Parameter is "inputParam".
            //6. Input is "someInput". Parameter is "someParam".

            // 1 case
            result = converter.Convert(input, null, parameter, null);
            Assert.AreEqual("", result);

            // 2 case
            input = "input";
            result = converter.Convert(input, null, parameter, null);
            Assert.AreEqual("input", result);

            // 3 case
            input = "none";
            result = converter.Convert(input, null, parameter, null);
            Assert.AreEqual("none", result);

            // 4 case
            input = "none";
            parameter = "inputParam";
            result = converter.Convert(input, null, parameter, null);
            Assert.AreEqual("none", result);

            // 5 case
            input = "someInput";
            parameter = "inputParam";
            result = converter.Convert(input, null, parameter, null);
            Assert.AreEqual(": someInput", result);

            // 6 case
            input = "someInput";
            parameter = "someParam";
            result = converter.Convert(input, null, parameter, null);
            Assert.AreEqual("someInput", result);
        }

        [Test]
        public void BrowserRootElementToSubclassesConverterTest()
        {
            BrowserRootElement BRE = new BrowserRootElement("BRE");
            NodeSearchElement NSE1 = new NodeSearchElement("name1", "description", new List<string>() { "tag" }, SearchElementGroup.Action);
            NodeSearchElement NSE2 = new NodeSearchElement("name2", "description", new List<string>() { "tag" }, SearchElementGroup.Action);
            BrowserInternalElement BIE = new BrowserInternalElement();

            BrowserRootElementToSubclassesConverter converter = new BrowserRootElementToSubclassesConverter();
            object result;

            //1. BRE contains only node elemnts.
            //2. BRE contains node elements and internal element.
            //3. BRE is null.

            // 1 case
            BRE.AddChild(NSE1);
            BRE.AddChild(NSE2);
            result = converter.Convert(BRE, null, null, null);
            Assert.AreEqual(BRE.ClassDetails, result);

            // 2 case
            BRE.AddChild(BIE);
            result = converter.Convert(BRE, null, null, null);
            Assert.AreEqual(BRE, result);

            // 3 case
            result = converter.Convert(null, null, null, null);
            Assert.AreEqual(null, result);
        }

        [Test]
        public void DisplayModeToTextDecorationsConverterTest()
        {
            DisplayModeToTextDecorationsConverter converter = new DisplayModeToTextDecorationsConverter();
            bool isSecondaryHeaderRightVisible = false;
            Dynamo.Nodes.Search.ClassInformation.DisplayMode displayMode = ClassInformation.DisplayMode.None;
            string parameter = "";
            object[] array = { displayMode, isSecondaryHeaderRightVisible };
            object result;

            //1. Array is null.
            //2. Parameter is null.
            //3. Right secondary header is invisible. Display mode is "None". Parameter is empty.
            //4. Right secondary header is invisible. Display mode is "Query". Parameter is empty.
            //5. Right secondary header is invisible. Display mode is "Action". Parameter is empty.
            //6. Right secondary header is visible. Display mode is "Action". Parameter is "Action".
            //7. Right secondary header is visible. Display mode is "Action". Parameter is "None".

            // 1 case
            Assert.Throws<NullReferenceException>(delegate { converter.Convert(null, null, null, null); });

            // 2 case
            result = converter.Convert(array, null, null, null);
            Assert.AreEqual(1, result);

            // 3 case
            result = converter.Convert(array, null, parameter, null);
            Assert.AreEqual(1, result);

            // 4 case
            array[0] = ClassInformation.DisplayMode.Query;
            result = converter.Convert(array, null, parameter, null);
            Assert.AreEqual(1, result);

            // 5 case
            array[0] = ClassInformation.DisplayMode.Action;
            result = converter.Convert(array, null, parameter, null);
            Assert.AreEqual(1, result);

            // 6 case
            parameter = "Action";
            isSecondaryHeaderRightVisible = true;
            array[1] = isSecondaryHeaderRightVisible;
            result = converter.Convert(array, null, parameter, null);
            Assert.AreEqual(1, result);

            // 7 case
            parameter = "None";
            result = converter.Convert(array, null, parameter, null);
            Assert.AreEqual(0, result);
        }

        [Test]
        public void ViewModeToVisibilityConverterTest()
        {
            ViewModeToVisibilityConverter converter = new ViewModeToVisibilityConverter();
            string parameter = "";
            SearchViewModel.ViewMode viewMode = SearchViewModel.ViewMode.LibraryView;
            object result;

            //1. Parameter is null.
            //2. View mode is null.
            //3. View mode is LibraryView. Parameter is empty.
            //4. View mode is LibraryView. Parameter is "LibraryView".
            //5. View mode is LibraryView. Parameter is "LibrarySearchView".

            // 1 case
            result = converter.Convert(viewMode, null, null, null);
            Assert.AreEqual(Visibility.Collapsed, result);

            // 2 case
            Assert.Throws<NullReferenceException>(delegate { converter.Convert(null, null, parameter, null); });

            // 3 case
            result = converter.Convert(viewMode, null, parameter, null);
            Assert.AreEqual(Visibility.Collapsed, result);

            // 4 case
            parameter = "LibraryView";
            result = converter.Convert(viewMode, null, parameter, null);
            Assert.AreEqual(Visibility.Visible, result);

            // 5 case
            parameter = "LibrarySearchView";
            result = converter.Convert(viewMode, null, parameter, null);
            Assert.AreEqual(Visibility.Collapsed, result);
        }

        [Test]
        public void ElementTypeToBoolConverterTest()
        {
            ElementTypeToBoolConverter converter = new ElementTypeToBoolConverter();
            NodeSearchElement NSE = new NodeSearchElement("name", "description", new List<string>() { "tag" }, SearchElementGroup.Action);
            BrowserInternalElement BIE = new BrowserInternalElement();
            BrowserInternalElementForClasses BIEFC = new BrowserInternalElementForClasses("name", BIE);
            BrowserRootElement BRE = new BrowserRootElement("name");
            object result;

            //1. Element is null.
            //2. Element is NodeSearchElement.
            //3. Element is BrowserInternalElement.
            //4. Element is BrowserInternalElementForClasses.
            //5. Element is BrowserRootElement.

            // 1 case
            result = converter.Convert(null, null, null, null);
            Assert.AreEqual(false, result);

            // 2 case
            result = converter.Convert(NSE, null, null, null);
            Assert.AreEqual(false, result);

            // 3 case
            result = converter.Convert(BIE, null, null, null);
            Assert.AreEqual(true, result);

            // 4 case
            result = converter.Convert(BIEFC, null, null, null);
            Assert.AreEqual(true, result);

            // 5 case
            result = converter.Convert(BRE, null, null, null);
            Assert.AreEqual(true, result);
        }

        [Test]
        public void NodeTypeToColorConverterTest()
        {
            NodeTypeToColorConverter converter = new NodeTypeToColorConverter();
            SolidColorBrush trueBrush = new SolidColorBrush(Colors.Green);
            SolidColorBrush falseBrush = new SolidColorBrush(Colors.Red);
            converter.FalseBrush = falseBrush;
            converter.TrueBrush = trueBrush;
            object result;

            //1. Element is null.
            //2. Element is CustomNodeSearchElement.

            // 1 case
            result = converter.Convert(null, null, null, null);
            Assert.AreEqual(falseBrush, result);

            // 2 case
            CustomNodeSearchElement CNE = new CustomNodeSearchElement(new CustomNodeInfo(new Guid(), "name", "cat", "desc", "path"), SearchElementGroup.Action);
            result = converter.Convert(CNE, null, null, null);
            Assert.AreEqual(trueBrush, result);
        }

        [Test]
        public void RootElementToBoolConverterTest()
        {
            RootElementToBoolConverter converter = new RootElementToBoolConverter();
            BrowserRootElement BRE = new BrowserRootElement("BRE");
            object result;

            //1. Element is null.
            //2. Element is BrowserRootElement.

            // 1 case
            result = converter.Convert(null, null, null, null);
            Assert.AreEqual(false, result);

            // 2 case
            result = converter.Convert(BRE, null, null, null);
            Assert.AreEqual(true, result);
        }

        [Test]
        public void HasParentRootElementTest()
        {
            HasParentRootElement converter = new HasParentRootElement();
            BrowserRootElement BRE = new BrowserRootElement("BRE");
            BrowserInternalElement BIE = new BrowserInternalElement();
            object result;

            //1. Element is null.
            //2. Element is BrowserRootElement.
            //3. Element is not child of BrowserRootElement.
            //4. Element is child of BrowserRootElement.

            // 1 case
            result = converter.Convert(null, null, null, null);
            Assert.AreEqual(false, result);

            // 2 case
            result = converter.Convert(BRE, null, null, null);
            Assert.AreEqual(true, result);

            // 3 case
            result = converter.Convert(BIE, null, null, null);
            Assert.AreEqual(false, result);

            // 4 case
            BRE.AddChild(BIE);
            result = converter.Convert(BIE, null, null, null);
            Assert.AreEqual(true, result);
        }

        [Test]
        public void MultiBoolToVisibilityConverterTest()
        {
            MultiBoolToVisibilityConverter converter = new MultiBoolToVisibilityConverter();
            object[] array = { false, false, false };
            object result;

            //1. Incoming array is null.
            //2. All are false.
            //3. One is true.
            //4. All are true.

            // 1 case
            Assert.Throws<NullReferenceException>(delegate { converter.Convert(null, null, null, null); });

            // 2 case
            result = converter.Convert(array, null, null, null);
            Assert.AreEqual(Visibility.Collapsed, result);

            // 3 case
            array[0] = true;
            result = converter.Convert(array, null, null, null);
            Assert.AreEqual(Visibility.Collapsed, result);

            // 4 case
            array[0] = true;
            array[1] = true;
            array[2] = true;
            result = converter.Convert(array, null, null, null);
            Assert.AreEqual(Visibility.Visible, result);
        }

        [Test]
        public void NullValueToCollapsedConverterTest()
        {
            NullValueToCollapsedConverter converter = new NullValueToCollapsedConverter();
            object result;

            //1. Value is null.
            //2. Value is not null.

            // 1 case
            result = converter.Convert(null, null, null, null);
            Assert.AreEqual(Visibility.Collapsed, result);

            // 2 case
            result = converter.Convert("not null", null, null, null);
            Assert.AreEqual(Visibility.Visible, result);
        }

        [Test]
        public void FullCategoryNameToMarginConverterTest()
        {
            FullCategoryNameToMarginConverter converter = new FullCategoryNameToMarginConverter();
            string name = "";
            Thickness thickness = new Thickness(5, 0, 0, 0);
            object result;

            //1. Name is null.
            //2. Name is empty.
            //3. Name is "Category".
            //4. Name is "Category.NestedClass1".
            //5. Name is "Category.NestedClass1.NestedClass2".

            // 1 case
            result = converter.Convert(null, null, null, null);
            Assert.AreEqual(thickness, result);

            // 2 case
            result = converter.Convert(name, null, null, null);
            Assert.AreEqual(thickness, result);

            // 3 case
            name = "Category";
            thickness = new Thickness(5, 0, 20, 0);
            result = converter.Convert(name, null, null, null);
            Assert.AreEqual(thickness, result);

            // 4 case
            name = "Category.NestedClass1";
            thickness = new Thickness(25, 0, 20, 0);
            result = converter.Convert(name, null, null, null);
            Assert.AreEqual(thickness, result);

            // 5 case
            name = "Category.NestedClass1.NestedClass2";
            thickness = new Thickness(45, 0, 20, 0);
            result = converter.Convert(name, null, null, null);
            Assert.AreEqual(thickness, result);
        }

        [Test]
        public void IntToVisibilityConverterTest()
        {
            IntToVisibilityConverter converter = new IntToVisibilityConverter();
            object result;

            //1. Number is null.
            //2. Number < 0.
            //3. Number == 0.
            //4. Number >0.

            // 1 case
            Assert.Throws<NullReferenceException>(delegate { converter.Convert(null, null, null, null); });

            // 2 case
            result = converter.Convert(-1, null, null, null);
            Assert.AreEqual(Visibility.Collapsed, result);

            // 3 case
            result = converter.Convert(0, null, null, null);
            Assert.AreEqual(Visibility.Collapsed, result);

            // 4 case
            result = converter.Convert(1, null, null, null);
            Assert.AreEqual(Visibility.Visible, result);
        }

        [Test]
        public void SearchHighlightMarginConverterTest()
        {
            SearchHighlightMarginConverter converter = new SearchHighlightMarginConverter();
            TextBlock textBlock = new TextBlock();
            textBlock.Width = 50;
            textBlock.Height = 10;

            # region dynamoViewModel and searchModel
            var model = DynamoModel.Start();
            var vizManager = new VisualizationManager(model);
            var watchHandler = new DefaultWatchHandler(vizManager, model.PreferenceSettings);
            DynamoViewModel dynamoViewModel = DynamoViewModel.Start();
            SearchModel searchModel = new SearchModel();
            # endregion

            SearchViewModel searhViewModel = new SearchViewModel(dynamoViewModel, searchModel);
            object[] array = { textBlock, searhViewModel };
            Thickness thickness = new Thickness(0, 0, textBlock.ActualWidth, textBlock.ActualHeight);
            object result;

            //1. Array is null.
            //2. TextBlock.Text is empty.
            //3. TextBlock contains highlighted phrase.

            // 1 case
            Assert.Throws<NullReferenceException>(delegate { converter.Convert(null, null, null, null); });

            // 2 case
            textBlock.Text = "";
            array[0] = textBlock;
            result = converter.Convert(array, null, null, null);
            Assert.AreEqual(thickness, result);

            // 3 case
            // This case we can't check properly, because TextBlock.ActualWidth and TextBlock.ActualHeight equals 0. 
            textBlock.Text = "abcd";
            array[0] = textBlock;
            searhViewModel.SearchText = "a";
            thickness = new Thickness(0, 0, -6.6733333333333338, 0);
            result = converter.Convert(array, null, null, null);
            Assert.AreEqual(thickness, result);
        }
    }
}
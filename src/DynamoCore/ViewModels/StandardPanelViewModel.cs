using System;
using System.Collections.Generic;
using Dynamo.Nodes.Search;
using System.Linq;
using Dynamo.Search;

namespace Dynamo.ViewModels
{
    public class StandardPanelViewModel : BrowserItem
    {
        #region BrowserItem abstract members implementation

        public override System.Collections.ObjectModel.ObservableCollection<BrowserItem> Items
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override string Name
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        private ClassInformation model;

        public IEnumerable<BrowserInternalElement> CreateMembers
        {
            get { return model.CreateMembers; }
        }

        public IEnumerable<BrowserInternalElement> ActionMembers
        {
            get { return model.ActionMembers; }
        }

        public IEnumerable<BrowserInternalElement> QueryMembers
        {
            get { return model.QueryMembers; }
        }

        /// <summary>
        /// Specifies whether or not instance should be shown as StandardPanel.
        /// </summary>
        public bool ClassDetailsVisibility
        {
            get
            {
                return CreateMembers.Any() || ActionMembers.Any() || QueryMembers.Any();
            }
        }

        public SearchElementGroup PrimaryHeaderGroup { get; set; }
        public SearchElementGroup SecondaryHeaderLeftGroup { get; set; }
        public SearchElementGroup SecondaryHeaderRightGroup { get; set; }
        public bool IsPrimaryHeaderVisible { get; set; }
        public bool IsSecondaryHeaderLeftVisible { get; set; }
        public bool IsSecondaryHeaderRightVisible { get; set; }

        public enum DisplayMode { None, Query, Action };

        /// <summary>
        /// Specifies which of QueryMembers of ActionMembers list is active for the moment.
        /// If any of CreateMembers, ActionMembers or QueryMembers lists is empty
        /// it returns 'None'.
        /// </summary>
        private DisplayMode currentDisplayMode;
        public DisplayMode CurrentDisplayMode
        {
            get
            {
                return currentDisplayMode;
            }
            set
            {
                currentDisplayMode = value;
                RaisePropertyChanged("CurrentDisplayMode");
            }
        }

        private IEnumerable<BrowserInternalElement> hiddenMembers;
        public IEnumerable<BrowserInternalElement> HiddenMembers
        {
            get { return hiddenMembers; }
            set
            {
                hiddenMembers = value;
                RaisePropertyChanged("IsMoreButtonVisible");
            }
        }

        public bool IsMoreButtonVisible
        {
            get
            {
                return HiddenMembers != null && HiddenMembers.Any();
            }
        }

        public StandardPanelViewModel()
            : this(new ClassInformation())
        { }

        public StandardPanelViewModel(ClassInformation classInfo)
        {
            model = classInfo;
        }

        public void PopulateMemberCollections(BrowserItem element)
        {
            model.PopulateMemberCollections(element);
        }
    }
}

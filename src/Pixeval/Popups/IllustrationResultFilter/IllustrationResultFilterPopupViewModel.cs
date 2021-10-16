using System;
using System.Collections.ObjectModel;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Pixeval.Misc;
using Pixeval.UserControls.TokenInput;

namespace Pixeval.Popups.IllustrationResultFilter
{
    public class IllustrationResultFilterPopupViewModel : ObservableObject
    {
#pragma warning disable CS8618
        public IllustrationResultFilterPopupViewModel()
#pragma warning restore CS8618
        {
            DefaultValueAttributeHelper.Initialize(this);
        }

        private ObservableCollection<Token> _includeTags;

        [DefaultValue(typeof(ObservableCollection<Token>))]
        public ObservableCollection<Token> IncludeTags
        {
            get => _includeTags;
            set => SetProperty(ref _includeTags, value);
        }

        private ObservableCollection<Token> _excludeTags;

        [DefaultValue(typeof(ObservableCollection<Token>))]
        public ObservableCollection<Token> ExcludeTags
        {
            get => _excludeTags;
            set => SetProperty(ref _excludeTags, value);
        }

        private int _leastBookmark;

        [DefaultValue(0)]
        public int LeastBookmark
        {
            get => _leastBookmark;
            set => SetProperty(ref _leastBookmark, value);
        }

        private int _maximumBookmark;

        [DefaultValue(int.MaxValue)]
        public int MaximumBookmark
        {
            get => _maximumBookmark;
            set => SetProperty(ref _maximumBookmark, value);
        }

        private ObservableCollection<Token> _userGroupName;

        [DefaultValue(typeof(ObservableCollection<Token>))]
        public ObservableCollection<Token> UserGroupName
        {
            get => _userGroupName;
            set => SetProperty(ref _userGroupName, value);
        }

        private Token _illustratorName;

        [DefaultValue(typeof(Token))]
        public Token IllustratorName
        {
            get => _illustratorName;
            set => SetProperty(ref _illustratorName, value);
        }

        private string _illustratorId;

        [DefaultValue("")]
        public string IllustratorId
        {
            get => _illustratorId;
            set => SetProperty(ref _illustratorId, value);
        }

        private Token _illustrationName;


        [DefaultValue(typeof(Token))]
        public Token IllustrationName
        {
            get => _illustrationName;
            set => SetProperty(ref _illustrationName, value);
        }

        private string _illustrationId;

        [DefaultValue("")]
        public string IllustrationId
        {
            get => _illustrationId;
            set => SetProperty(ref _illustrationId, value);
        }

        private DateTimeOffset _publishDateStart;

        [DefaultValue(typeof(MinDateTimeOffSetDefaultValueProvider))]
        public DateTimeOffset PublishDateStart
        {
            get => _publishDateStart;
            set => SetProperty(ref _publishDateStart, value);
        }

        private DateTimeOffset _publishDateEnd;

        [DefaultValue(typeof(MaxDateTimeOffSetDefaultValueProvider))]
        public DateTimeOffset PublishDateEnd
        {
            get => _publishDateEnd;
            set => SetProperty(ref _publishDateEnd, value);
        }
    }
}
using Flantter.MilkyWay.Common;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.Models
{
    public class TweetAreaModel : BindableBase
    {
        public TweetAreaModel()
        {
            this.Text = string.Empty;
            this.CharacterCount = 140;
        }

        #region Text変更通知プロパティ
        private string _Text;
        public string Text
        {
            get { return this._Text; }
            set
            {
                if (this._Text != value)
                {
                    this._Text = value;
                    this.OnPropertyChanged("Text");
                    this.CharacterCountChanged();
                }
            }
        }
        #endregion

        #region CharacterCount変更通知プロパティ
        private int _CharacterCount;
        public int CharacterCount
        {
            get { return this._CharacterCount; }
            set { this.SetProperty(ref this._CharacterCount, value); }
        }
        #endregion

        public void CharacterCountChanged()
        {
            StringInfo textInfo = new StringInfo(this._Text.Replace("\r\n", "\n"));
            int count = 140 - textInfo.LengthInTextElements;

            foreach (Match m in TweetRegexPatterns.ValidUrl.Matches(this._Text))
                count += m.Value.Length - (m.Value.ToLower().StartsWith("https://") ? 23 : 22);

            this.CharacterCount = count;
        }
    }
}

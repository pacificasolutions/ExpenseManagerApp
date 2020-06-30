using System;
using System.Collections.Generic;
using System.Text;

namespace RazorHTMLEmails.Views.Emails.ConfirmAccount
{
    public class ConfirmAccountEmailViewModel
    {
        public ConfirmAccountEmailViewModel(string confirmEmailUrl, string name)
        {
            ConfirmEmailUrl = confirmEmailUrl;
            Name = name;
        }

        public string ConfirmEmailUrl { get; set; }
        public string Name { get; set; }
    }
}
